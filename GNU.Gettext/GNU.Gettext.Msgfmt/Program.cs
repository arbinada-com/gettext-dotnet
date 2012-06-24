using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;

using GNU.Getopt;

namespace GNU.Gettext.Msgfmt
{
    public enum Mode
    {
        Resources,
        SateliteAssembly
    }

    public class CmdLineOptions
    {
        public string CompilerName { get; set; }
        public string InputFile { get; set; }
        public string OutFile { get; set; }
        public string OutDir { get; set; }
        public string LibDir { get; set; }
        public string LocaleStr { get; set; }
        public CultureInfo Locale { get; set; }
        public string BaseName { get; set; }
		public Mode Mode { get; set; }
		public bool Verbose { get; set; }
		public bool ShowUsage { get; set; }
//		public string ClassName
//		{
//			get { return GettextResourceManager.ExtractClassName(this.BaseName); }
//		}
//
//		public string NamespaceName
//		{
//			get { return GettextResourceManager.ExtractNamespace(this.BaseName); }
//		}
		public bool HasNamespace
		{
			get { return !String.IsNullOrEmpty(BaseName); }
		}
    }

    public class Program
    {
		public const String SOpts = "-:hravi:o:d:l:L:b:c:";
		public static LongOpt[] LOpts
		{
			get
			{
				LongOpt[] lopts = new LongOpt[11];
				lopts[0] = new LongOpt("help", Argument.No, null, 'h');
				lopts[1] = new LongOpt("mode-resource", Argument.No, null, 'r');
				lopts[2] = new LongOpt("mode-assembly", Argument.No, null, 'a');
				lopts[3] = new LongOpt("input-file", Argument.Required, null, 'i');
				lopts[4] = new LongOpt("output-file", Argument.Required, null, 'o');
				lopts[5] = new LongOpt("output-dir", Argument.Required, null, 'd');
				lopts[6] = new LongOpt("locale", Argument.Required, null, 'l');
				lopts[7] = new LongOpt("base-name", Argument.Required, null, 'b');
				lopts[8] = new LongOpt("lib-dir", Argument.Required, null, 'L');
				lopts[9] = new LongOpt("verbose", Argument.No, null, 'v');
				lopts[10] = new LongOpt("compiler-name", Argument.Required, null, 'c');
				return lopts;
			}
		}

        static int Main(string[] args)
        {

			StringBuilder message;
            CmdLineOptions options = new CmdLineOptions();
			if (!GetOptions(args, SOpts, LOpts, options, out message))
			{
				Console.WriteLine(message.ToString());
				return 1;
			}
			else if (options.ShowUsage)
			{
				return 0;
			}
			if (!AnalyseOptions(options, out message))
			{
				Console.WriteLine(message.ToString());
				return 1;
			}

            try
            {
                switch (options.Mode)
                {
                    case Mode.Resources:
                        (new ResourcesGen(options)).Run();
                        Console.WriteLine("Resource created OK");
                        break;
                    case Mode.SateliteAssembly:
                        (new AssemblyGen(options)).Run();
                        Console.WriteLine("Generated OK");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during execution: {0}", ex.Message);
                return 1;
            }
			return 0;
		}

		public static bool GetOptions(string[] args, String sopts, LongOpt[] lopts, CmdLineOptions options, out StringBuilder message)
		{
			message = new StringBuilder();
            Getopt.Getopt getopt = new Getopt.Getopt(
                Assembly.GetExecutingAssembly().GetName().Name,
                args, sopts, lopts)
                {
                    Opterr = false
                };

            options.Mode = Mode.SateliteAssembly;
			options.Verbose = false;
			options.CompilerName = "mcs";
			options.ShowUsage = false;

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
					//message.AppendFormat("Invalid option {0}", args[getopt.Optind]);
					//return false;
					break; // getopt() already printed an error
				case 'a':
					options.Mode = Mode.SateliteAssembly;
					break;
				case 'r':
					options.Mode = Mode.Resources;
					break;
				case 'i':
                    options.InputFile = getopt.Optarg;
                    break;
                case 'o':
                    options.OutFile = getopt.Optarg;
                    break;
                case 'd':
                    options.OutDir = getopt.Optarg;
                    break;
                case 'l':
                    options.LocaleStr = getopt.Optarg;
                    break;
                case 'L':
                    options.LibDir = getopt.Optarg;
                    break;
                case 'b':
                    options.BaseName = getopt.Optarg;
                    break;
                case 'c':
                    options.CompilerName = getopt.Optarg;
                    break;
                case 'v':
                    options.Verbose = true;
                    break;
                case 'h':
                    PrintUsage();
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

		public static bool AnalyseOptions(CmdLineOptions options, out StringBuilder message)
		{
			message = new StringBuilder();
            bool accepted = true;
            try
            {
                if (String.IsNullOrEmpty(options.InputFile))
                {
                    message.Append("Undefined input file name");
                    accepted = false;
                }

                if (accepted && !File.Exists(options.InputFile))
                {
                    message.AppendFormat("File {0} not found", options.InputFile);
                    accepted = false;
                }


                if (accepted && options.Mode == Mode.Resources && String.IsNullOrEmpty(options.OutFile))
                {
                    message.Append("Undefined output file name");
                    accepted = false;
                }

                if (accepted && options.Mode == Mode.SateliteAssembly)
                {
                    if (accepted && String.IsNullOrEmpty(options.BaseName))
                    {
                        message.Append("Undefined catalog name");
                        accepted = false;
                    }
                    if (accepted && String.IsNullOrEmpty(options.OutDir))
                    {
                        message.Append("Output dirictory name required");
                        accepted = false;
                    }
                    if (accepted && String.IsNullOrEmpty(options.LibDir))
                    {
                        message.Append("Assembly reference dirictory name required");
                        accepted = false;
                    }
                    if (accepted && String.IsNullOrEmpty(options.LocaleStr))
                    {
                        message.Append("Locale is not defined");
                        accepted = false;
                    }
                    else if (accepted)
                    {
                        options.Locale = new CultureInfo(options.LocaleStr);
                    }

                    if (accepted && options.Locale == null)
                    {
                        message.AppendFormat("Cannot create culture from {0}", options.LocaleStr);
                        accepted = false;
                    }
                }
            }
            catch(Exception e)
            {
                message.Append(e.Message);
                accepted = false;
            }

            if (!accepted)
            {
				message.AppendLine();
                message.Append("Error accepting options");
                return false;
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Gettext .NET tools");
            Console.WriteLine("Custom message formatter from .po to .resources file or to satellite assembly DLL");
            Console.WriteLine();
            Console.WriteLine("Usage: {0}[.exe] [OPTIONS]",
                Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine("   -a, --mode-assembly            Generate satellite assembly DLL (default)");
            Console.WriteLine("   -r, --mode-resource            Generate .resources file");
            Console.WriteLine("   -iFILE, --inputfile=FILE       Input PO file name (must be in PO format)");
            Console.WriteLine("   -oFILE, --output-file=FILE     Output file name for .NET resources file. Ignored when -d is specified");
            Console.WriteLine("   -dPATH, --output-dir=PATH      Output root directory for satellite assemblies");
            Console.WriteLine("   -lLOCALE, --locale=LOCALE      .NET locale (culture) name i.e. \"en-US\", \"en\", \"fr-FR\" etc.");
            Console.WriteLine("                                  Used for satellite assemblies' subdirectories");
            Console.WriteLine("   -bNAME, --base-name=NAME       Base name for resources catalog i.e. \"Messages\"");
            Console.WriteLine("   -LPATH, --lib-dir=PATH         Path to directory where GNU.Gettext.dll is located");
            Console.WriteLine("   -cNAME, --compiler-name=NAME   C# compiler name. Default is \"mcs\".");
            Console.WriteLine("                                  Check that compiler directory is in PATH environment variable for creating an assembly");
            Console.WriteLine("   -v, --verbose                  Verbose output");
        }
    }
}
