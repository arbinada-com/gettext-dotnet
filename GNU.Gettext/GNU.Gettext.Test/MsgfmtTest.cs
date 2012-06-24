using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

using NUnit.Framework;

using GNU.Gettext.Msgfmt;


namespace GNU.Gettext.Test
{
	[TestFixture()]
	public class MsgfmtTest
	{
		[Test()]
		public void AssemblyGenerationTest()
		{
			CmdLineOptions options = new CmdLineOptions();
			options.Mode = Mode.SateliteAssembly;
			options.InputFile = "../../../Examples.Hello/po/fr.po";
			options.BaseName = "Examples.Hello.Messages";
			options.OutDir = "../../../Examples.Hello/bin/Debug";
			options.CompilerName = "mcs";
			options.LibDir = "./";
			options.Locale = new CultureInfo("fr-FR");
			options.Verbose = true;

			AssemblyGen gen = new AssemblyGen(options);
			gen.DeleteFile = false;
			gen.Run();
		}

		[Test()]
		public void ResourcesGenerationTest()
		{
			CmdLineOptions options = new CmdLineOptions();
			options.Mode = Mode.Resources;
			options.InputFile = "./Data/Test01.po";
			options.OutFile = "./Messages.fr-FR.resources";
			options.Verbose = true;

			ResourcesGen gen = new ResourcesGen(options);
			gen.Run();
		}
	}
}

