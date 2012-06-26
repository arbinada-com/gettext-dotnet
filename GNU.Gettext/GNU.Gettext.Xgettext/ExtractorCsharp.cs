using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GNU.Gettext.Xgettext
{
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
			StreamReader inputFile = new StreamReader(fileName);
			string text = inputFile.ReadToEnd();
			inputFile.Close();
			GetMessages(text, fileName);
		}
		
			
		public void GetMessages(string text, string sourceFile) 
		{
			ProcessPattern(@"\.\s*Text\s*=\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(@"GetString\(\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(@"GetStringFmt\(\s*" + CsharpStringPattern, text, sourceFile);
			ProcessPattern(@"GetPluralString\(\s*" + 
			               CsharpStringPattern +
			               @"\s*,\s*" +
			               CsharpStringPattern
			               ,
			               text, sourceFile);
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
		
		private void ProcessPattern(string pattern, string text, string sourceFile)
		{
			Regex r = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
			MatchCollection matches = r.Matches(text);
			
			foreach (Match match in matches)
			{
				GroupCollection groups = match.Groups;
				string msgid = StripMsgid(groups[1].Value);
				CatalogEntry entry = Catalog.FindItem(msgid);
				bool entryFound = entry != null;
				if (groups.Count == 3)
				{
					string msgidPlural = StripMsgid(groups[2].Value);
					if (!entryFound)
					{
						entry = new CatalogEntry(Catalog, msgid, msgidPlural);
						AddPluralsTranslations(entry);
					}
					else if (!entry.HasPlural)
					{
						AddPluralsTranslations(entry);
						entry.SetPluralString(msgidPlural);
					}
					else if (entry.HasPlural && entry.PluralString != msgidPlural)
					{
						entry.SetPluralString(msgidPlural);
					}
				}
				else
				{
					if (entry == null)
						entry = new CatalogEntry(Catalog, StripMsgid(groups[1].Value), null);
				}
				if (!entryFound)
				{
					Catalog.AddItem(entry);
				}
				
				string sourceRef = String.Format("{0}: {1}", sourceFile.Replace('\\', '/'), match.Index);
				entry.ClearReferences();
				entry.AddReference(sourceRef);
			}
		}

		private void AddPluralsTranslations(CatalogEntry entry)
		{
			// Creating 2 plurals forms by default
			// Translator should change it using expression for it own country
			// http://translate.sourceforge.net/wiki/l10n/pluralforms
			List<string> translations = new List<string>();
			for(int i = 0; i < Options.DefaultPluralsNumberOfTranslations; i++)
				translations.Add("");
			entry.SetTranslations(translations.ToArray());
		}
		
		private static string StripMsgid(string msgid)
		{
			return msgid.Trim(new char[] {'@', '"'}).Replace("\r", "");
		}
	}
}

