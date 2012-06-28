using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GNU.Gettext.Xgettext
{
	public enum ExtractMode
	{
		Msgid,
		MsgidPlural,
		ContextMsgid,
	}
	
	public class ExtractorCsharp
	{
		const string CsharpStringPatternExplained = @"
			(\w+)\s*=\s*    # key =
			(               # Capturing group for the string
			    @""               # verbatim string - match literal at-sign and a quote
			    (?:
			        [^""]|""""    # match a non-quote character, or two quotes
			    )*                # zero times or more
			    ""                #literal quote
			|               #OR - regular string
			    ""              # string literal - opening quote
			    (?:
			        \\.         # match an escaped character,
			        |[^\\""]    # or a character that isn't a quote or a backslash
			    )*              # a few times
			    ""              # string literal - closing quote
			)";
	
		const string CsharpStringPattern = @"(@""(?:[^""]|"""")*""|""(?:\\.|[^\\""])*"")";
		const string TwoStringsArgumentsPattern = CsharpStringPattern + @"\s*,\s*" + CsharpStringPattern;
		
		public Catalog Catalog { get; private set; }
		public Options Options { get; private set; }
		
		#region Constructors
		public ExtractorCsharp(Options options)
		{
			this.Options = options;
			this.Catalog = new Catalog();
			if (!Options.Overwrite && File.Exists(Options.OutFile))
				Catalog.Load(Options.OutFile);
			else
			{
				Catalog.Project = "PACKAGE VERSION";
			}
			
			this.Options.OutFile = Path.GetFullPath(this.Options.OutFile);
		}
		#endregion

		public void GetMessages()
		{
			if (Options.InputMode == InputMode.File)
			{
				GetMessagesFromFile(Options.InputFile);
			}
			else
			{
				string[] files = Directory.GetFiles(
					Options.InputDir, 
					Options.FileMask, 
					Options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

				foreach (string file in files)
				{
				    GetMessagesFromFile(file);
				}
			}
		}
		
		private void GetMessagesFromFile(string fileName)
		{
			fileName = Path.GetFullPath(fileName);
			StreamReader inputFile = new StreamReader(fileName);
			string text = inputFile.ReadToEnd();
			inputFile.Close();
			GetMessages(text, fileName);
		}
		
			
		public void GetMessages(string text, string sourceFile) 
		{
			ProcessPattern(ExtractMode.Msgid, @"\.\s*Text\s*=\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(ExtractMode.Msgid, @"GetString\(\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(ExtractMode.Msgid, @"GetStringFmt\(\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(ExtractMode.MsgidPlural, @"GetPluralString\(\s*" + TwoStringsArgumentsPattern, text, sourceFile);
			ProcessPattern(ExtractMode.ContextMsgid, @"GetParticularString\(\s*" + TwoStringsArgumentsPattern, text, sourceFile);
		}
		
		public void Save()
		{
			if (File.Exists(Options.OutFile))
			{
				string bakFileName = Options.OutFile + ".bak";
				File.Delete(bakFileName);
				File.Copy(Options.OutFile, bakFileName);
				File.Delete(Options.OutFile);
			}
			Catalog.Save(Options.OutFile);
		}
		
		private void ProcessPattern(ExtractMode mode, string pattern, string text, string sourceFile)
		{
			Regex r = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
			MatchCollection matches = r.Matches(text);
			
			foreach (Match match in matches)
			{
				GroupCollection groups = match.Groups;
				
				// Initialisation
				string msgid = String.Empty;
				string msgidPlural = String.Empty;
				string context = String.Empty;
				switch(mode)
				{
				case ExtractMode.Msgid:
					msgid = Unescape(groups[1].Value);
					break;
				case ExtractMode.MsgidPlural:
					if (groups.Count < 3)
						throw new Exception(String.Format("Invalid 'GetPluralString' call.\nSource: {0}", match.Value));
					msgid = Unescape(groups[1].Value);
					msgidPlural = Unescape(groups[2].Value);
					break;
				case ExtractMode.ContextMsgid:
					if (groups.Count < 3)
						throw new Exception(String.Format("Invalid get context message call.\nSource: {0}", match.Value));
					context = Unescape(groups[1].Value);
					msgid = Unescape(groups[2].Value);
					break;
				}
				
				// Processing
				CatalogEntry entry = Catalog.FindItem(msgid, context);
				bool entryFound = entry != null;
				if (!entryFound)
					entry = new CatalogEntry(Catalog, msgid, msgidPlural);
				
				switch(mode)
				{
				case ExtractMode.Msgid:
					break;
				case ExtractMode.MsgidPlural:
					if (!entryFound)
					{
						AddPluralsTranslations(entry);
					}
					else
						UpdatePluralEntry(entry, msgidPlural);
					break;
				case ExtractMode.ContextMsgid:
					entry.Context = context;
					entry.AddAutoComment(String.Format("Context: {0}", context), true);
					break;
				}
				
				// Add source reference if it not exists yet
				Uri fileUri = new Uri(sourceFile);
				Uri outDirUri = new Uri(Path.GetDirectoryName(Options.OutFile));
				Uri relativeUri = outDirUri.MakeRelativeUri(fileUri);
				// Each reference is in the form "path_name:line_number"
				string sourceRef = String.Format("{0}:{1}", relativeUri.ToString(), CalcLineNumber(text, match.Index));
				entry.AddReference(sourceRef); // Wont be added if exists
				
				if (!entryFound)
					Catalog.AddItem(entry);
			}
		}
		
		private int CalcLineNumber(string text, int pos)
		{
			if (pos >= text.Length)
				pos = text.Length - 1;
			int line = 0;
			for (int i = 0; i < pos; i++)
				if (text[i] == '\n')
					line++;
			return line + 1;
		}
		
		private void UpdatePluralEntry(CatalogEntry entry, string msgidPlural)
		{
			if (!entry.HasPlural)
			{
				AddPluralsTranslations(entry);
				entry.SetPluralString(msgidPlural);
			}
			else if (entry.HasPlural && entry.PluralString != msgidPlural)
			{
				entry.SetPluralString(msgidPlural);
			}
		}

		private void AddPluralsTranslations(CatalogEntry entry)
		{
			// Creating 2 plurals forms by default
			// Translator should change it using expression for it own country
			// http://translate.sourceforge.net/wiki/l10n/pluralforms
			List<string> translations = new List<string>();
			for(int i = 0; i < Catalog.PluralFormsCount; i++)
				translations.Add("");
			entry.SetTranslations(translations.ToArray());
		}
		
		private static string Unescape(string msgid)
		{
			StringEscaping.EscapeMode mode = StringEscaping.EscapeMode.CSharp;
			if (msgid.StartsWith("@"))
				mode = StringEscaping.EscapeMode.CSharpVerbatim;
			return StringEscaping.UnEscape(mode, msgid.Trim(new char[] {'@', '"'}));
		}
	}
}

