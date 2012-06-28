﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Diagnostics;

using GNU.Gettext;

namespace GNU.Gettext.Msgfmt
{
    public class AssemblyGen
    {
        private IndentedTextWriter cw;
        private StringWriter sw;
		private Catalog catalog;

        public Dictionary<string, string> Entries { get; private set; }
		public string FileName { get; private set; }
		public string AssemblyOutDir  { get; private set; }
		public CmdLineOptions Options { get; private set; }
		public string ClassName { get; private set; }

		#region Constructors
        public AssemblyGen(CmdLineOptions options)
        {
            sw = new StringWriter();
            cw = new IndentedTextWriter(sw);
			this.Options = options;
			ClassName = GettextResourceManager.MakeResourceSetClassName(Options.BaseName, Options.Locale);
#if DEBUG			
            FileName = Path.Combine(
				Options.OutDir,
                String.Format("{0}.{1}.resources.cs", Options.BaseName, Options.Locale.Name));
#else
            FileName = Path.GetTempFileName();
#endif
        }
		#endregion

        public void Run()
        {
			catalog = new Catalog();
			catalog.Load(Options.InputFile);

			Generate();
			SaveToFile();
			Compile();
			if (!Options.DebugMode)
				File.Delete(FileName);
        }

		private void Generate()
		{
			cw.WriteLine("// This file was generated by GNU msgfmt at {0}", DateTime.Now);
			cw.WriteLine("// Do not modify it!");
			cw.WriteLine();
			cw.WriteLine("using GNU.Gettext;");
			cw.WriteLine();

			/* Assign a strong name to the assembly, so that two different localizations
			 * of the same domain can be loaded one after the other.  This strong name
			 * tells the Global Assembly Cache that they are meant to be different.
			 */
			cw.WriteLine("[assembly: System.Reflection.AssemblyCulture(\"{0}\")]", Options.Locale.Name);

			if (Options.HasNamespace)
			{
				cw.WriteLine("namespace {0}", Options.BaseName);
				cw.WriteLine("{");
				cw.Indent++;
			}

			cw.WriteLine("public class {0} : {1}",
                ClassName,
                typeof(GettextResourceSet).FullName);
            cw.WriteLine("{");
			cw.Indent++;

			// Constructor
			cw.WriteLine("public {0} () : base()", ClassName);
			cw.WriteLine("{ }");
			cw.WriteLine();

			cw.WriteLine("private bool TableInitialized;");
			cw.WriteLine();
			
			if (catalog.HasHeader(Catalog.PluralFormsHeader))
			{
				cw.WriteLine("public override string PluralForms {{ get {{ return {0}; }} }}", ToConstStr(catalog.GetPluralFormsHeader()));
				cw.WriteLine();
			}

			cw.WriteLine("protected override void ReadResources() {");
			/* In some implementations, such as mono < 2009-02-27, the ReadResources
			 * method is called just once, when Table == null.  In other implementations,
			 * such as mono >= 2009-02-27, it is called at every GetObject call, and it
			 * is responsible for doing the initialization only once, even when called
			 * simultaneously from multiple threads.
			 */
			cw.Indent++; cw.WriteLine("if (!TableInitialized) {");
			cw.Indent++; cw.WriteLine("lock (this) {");
			cw.Indent++; cw.WriteLine("if (!TableInitialized) {");
			/* In some implementations, the ResourceSet constructor initializes Table
			 * before calling ReadResources().  In other implementations, the
			 * ReadResources() method is expected to initialize the Table.  */
			cw.Indent++; cw.WriteLine("if (Table == null)");
			cw.Indent++; cw.WriteLine("Table = new System.Collections.Hashtable();");
			cw.Indent--; cw.WriteLine("System.Collections.Hashtable t = Table;");
			foreach(CatalogEntry entry in catalog)
			{
			  cw.WriteLine("t.Add({0}, {1});", ToMsgid(entry), ToMsgstr(entry));
			}
			cw.WriteLine("TableInitialized = true;");
			cw.Indent--; cw.WriteLine("}");
			cw.Indent--; cw.WriteLine("}");
			cw.Indent--; cw.WriteLine("}");
			cw.Indent--; cw.WriteLine("}");
			cw.WriteLine();

			// Emit the msgid_plural strings. Only used by msgunfmt.
			if (catalog.PluralFormsCount > 0)
			{
				cw.WriteLine("public static System.Collections.Hashtable GetMsgidPluralTable() {");
				cw.Indent++; cw.WriteLine("System.Collections.Hashtable t = new System.Collections.Hashtable();");
				foreach(CatalogEntry entry in catalog)
				{
					if (entry.HasPlural)
					{
				        cw.WriteLine("t.Add({0}, {1});", ToMsgid(entry), ToMsgstr(entry));
					}
				}
				cw.WriteLine("return t;");
				cw.Indent--; cw.WriteLine("}");
			}

			cw.Indent--;
            cw.WriteLine("}");

			if (Options.HasNamespace)
			{
				cw.Indent--;
				cw.WriteLine("}");
			}

		}

		private void SaveToFile()
		{
            using (StreamWriter writer = new StreamWriter(FileName, false, Encoding.UTF8))
            {
                writer.WriteLine(sw.ToString());
            }

			AssemblyOutDir = Path.Combine(Path.GetFullPath(Options.OutDir), Options.Locale.Name);
			if (!Directory.Exists(AssemblyOutDir))
				Directory.CreateDirectory(AssemblyOutDir);
			if (!Directory.Exists(AssemblyOutDir))
				throw new Exception(String.Format("Error creating output directory {0}", AssemblyOutDir));
		}


		private void Compile()
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardInput = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = Options.CompilerName;
			p.StartInfo.Arguments = String.Format(
				"-target:library -out:\"{0}/{1}.resources.dll\" -lib:\"{2}\" -reference:GNU.Gettext.dll -optimize+ \"{3}\"",
				AssemblyOutDir,
				Options.BaseName,
				Path.GetFullPath(Options.LibDir),
				FileName
				);
			if (Options.Verbose)
				Console.WriteLine("Compiler: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
			p.Start();
			p.WaitForExit(30000);
			if (p.HasExited)
			{
				if (p.ExitCode != 0)
				{
					throw new Exception(String.Format(
						"Assembly compilation failed. ExitCode: {0}\nSee source file: {1}\n{2}\n{3}",
						p.ExitCode,
						FileName,
						p.StandardOutput.ReadToEnd(),
						p.StandardError.ReadToEnd()));
				}
			}
			else
			{
				p.Close();
				p.Kill();
				throw new Exception("Assembly compilation timeout");
			}
		}


		static string ToConstStr(string s)
		{
			return String.Format("@\"{0}\"", s.Replace("\"", "\"\""));
		}

		static string ToMsgid(CatalogEntry entry)
		{
			return ToConstStr(
				entry.HasContext ? 
				GettextResourceManager.MakeContextMsgid(entry.Context, entry.String) : entry.String);
		}


		/// <summary>
		/// Write C# code that returns the value for a message.  If the message
		/// has plural forms, it is an expression of type System.String[], otherwise it
		/// is an expression of type System.String.
		/// </summary>
		/// <returns>
		/// The expression (string or string[]) to initialize hashtable associated object.
		/// </returns>
		/// <param name='entry'>
		/// Catalog entry.
		/// </param>
		static string ToMsgstr(CatalogEntry entry)
		{
			StringBuilder sb = new StringBuilder();
			if (entry.HasPlural)
			{
				sb.Append("new System.String[] { ");
				for(int i = 0; i < entry.TranslationsCount; i++)
				{
					if (i > 0)
						sb.Append(", ");
					sb.Append(ToConstStr(entry.GetTranslation(i)));
				}
				sb.Append(" }");
			}
			else
			{
				sb.Append(ToConstStr(entry.GetTranslation(0)));
			}
			return sb.ToString();
		}

    }
}
