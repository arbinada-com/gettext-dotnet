using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using GNU.Getopt;

namespace GNU.Gettext.Xgettext
{
	
	public enum InputMode
	{
		File,
		Dir
	}
	
	public class Options
	{
		public string InputFile { get; set; }
		public string InputDir { get; set; }
        public string OutFile { get; set; }
		public bool Overwrite { get; set; }
		public bool Recursive { get; set; }
		public bool Verbose { get; set; }
		public bool ShowUsage { get; set; }
        public string FileMask { get; set; }
		public InputMode InputMode { get; set; }
	}
	
	class MainClass
	{
		public const String SOpts = "-:hwri:d:o:v";
		public static LongOpt[] LOpts
		{
			get
			{
				LongOpt[] lopts = new LongOpt[] 
				{
					new LongOpt("help", Argument.No, null, 'h'),
					new LongOpt("overwrite", Argument.No, null, 'w'),
					new LongOpt("recursive", Argument.No, null, 'r'),
					new LongOpt("input-file", Argument.Required, null, 'i'),
					new LongOpt("input-dir", Argument.Required, null, 'd'),
					new LongOpt("output-file", Argument.Required, null, 'o'),
					new LongOpt("verbose", Argument.No, null, 'v')
				};
				return lopts;
			}
		}

		public static int Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			
			StringBuilder message;
			
            Options options = new Options();
			if (args.Length == 0)
			{
                PrintUsage();
				return 1;
			}
			else if (!GetOptions(args, SOpts, LOpts, options, out message))
			{
				Console.WriteLine(message.ToString());
				return 1;
			}
			else if (options.ShowUsage)
			{
                PrintUsage();
				return 0;
			}
			if (!AnalyseOptions(options, out message))
			{
				Console.WriteLine(message.ToString());
				return 1;
			}

            try
            {
				ExtractorCsharp extractor = new ExtractorCsharp(options);
				extractor.GetMessages();
				extractor.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during execution: {0}", ex.Message);
                return 1;
            }
			
			Console.WriteLine("Telmplate file '{0}' generated", options.OutFile);
			return 0;
			
		}
		
		public static bool GetOptions(string[] args, String sopts, LongOpt[] lopts, Options options, out StringBuilder message)
		{
			message = new StringBuilder();
            Getopt.Getopt getopt = new Getopt.Getopt(
                Assembly.GetExecutingAssembly().GetName().Name,
                args, sopts, lopts)
                {
                    Opterr = false
                };

			options.Verbose = false;
			options.ShowUsage = false;
			options.Recursive = false;
			options.FileMask = "*.cs";

            int option;
            while ((option = getopt.getopt()) != -1)
            {
                switch (option)
                {
				case ':':
					message.AppendFormat("Option {0} requires an argument",
					                     args[getopt.Optind - 1]);
					return false;
				case '?':
					break; // getopt() already printed an error
				case 'r':
					options.Recursive = true;
					break;
				case 'i':
                    options.InputFile = getopt.Optarg;
                    break;
                case 'o':
                    options.OutFile = getopt.Optarg;
                    break;
                case 'd':
                    options.InputDir = getopt.Optarg;
                    break;
                case 'v':
                    options.Verbose = true;
                    break;
                case 'h':
					options.ShowUsage = true;
                    return true;
				default:
                    PrintUsage();
                    return false;
                }
            }

			if (getopt.Opterr)
			{
				message.AppendLine();
                message.Append("Error in command line options. Use -h to read options usage");
                return false;
			}
			return true;
		}

		public static bool AnalyseOptions(Options options, out StringBuilder message)
		{
			options.InputMode = !String.IsNullOrEmpty(options.InputFile) ? InputMode.File : InputMode.Dir;
			message = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(options.InputFile) && String.IsNullOrEmpty(options.InputDir))
                {
                    message.Append("Undefined input file name or directory");
                    return false;
                }
				

                if (options.InputMode == InputMode.File && !File.Exists(options.InputFile))
                {
                    message.AppendFormat("File '{0}' not found", options.InputFile);
                    return false;
                }
				
                if (options.InputMode == InputMode.Dir && !Directory.Exists(options.InputDir))
                {
                    message.AppendFormat("Source directory '{0}' not found", options.InputDir);
                    return false;
                }
            }
            catch(Exception e)
            {
                message.Append(e.Message);
                return false;
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Gettext .NET tools");
            Console.WriteLine("Extract strings from C# source code files and then creates or updates PO template file");
            Console.WriteLine();
            Console.WriteLine("Usage: {0}[.exe] [OPTIONS]",
                Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine("   -w, --overwrite                Overwrite output file instead of updating");
            Console.WriteLine("   -iFILE, --input-file=FILE       Input C# source code file name");
            Console.WriteLine("   -oFILE, --output-file=FILE     Output PO template file name. Using of default '.pot' file type is recommended");
            Console.WriteLine("   -dPATH, --input-dir=PATH       Input root directory for C# source code files");
            Console.WriteLine("   -r, --recursive                Process all C# source code files in directory and in subdirectories");
            Console.WriteLine("   -v, --verbose                  Verbose output");
        }
	}

}
