using System;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using GNU.Getopt;
using GNU.Gettext.Msgfmt;

namespace GNU.Gettext.Test
{
	[TestFixture()]
	public class GetoptTest
	{
		[SetUpAttribute]
		public void TestSetup()
		{ }

		[Test()]
		public void GetoptParamsTest()
		{

			string[] args = new string[]
			{
				"-i./po/fr.po",
				"-lfr-FR",
				"-d./bin/Debug",
				"-bExamples.Hello.Messages",
				"-cgmcs",
				"-L./../../Bin",
				"-v"
			};
			CmdLineOptions options = new CmdLineOptions();
			StringBuilder message;
			Assert.IsTrue(Msgfmt.Program.GetOptions(args, Program.SOpts, Program.LOpts, options, out message), message.ToString());
			CheckOptions(options);
		}

		[Test()]
		public void GetoptLongParamsTest()
		{
			string[] args = new string[]
			{
				"--input-file=./po/fr.po",
				"--locale=fr-FR",
				"--output-dir=./bin/Debug",
				"--base-name=Examples.Hello.Messages",
				"--compiler-name=gmcs",
				"--lib-dir=./../../Bin",
				"--verbose"
			};
			CmdLineOptions options = new CmdLineOptions();
			StringBuilder message;
			Assert.IsTrue(Msgfmt.Program.GetOptions(args, Program.SOpts, Program.LOpts, options, out message), message.ToString());
			Assert.AreEqual(0, message.Length, message.ToString());
			CheckOptions(options);
		}

		private void CheckOptions(CmdLineOptions options)
		{
			Assert.AreEqual("./po/fr.po", options.InputFile);
			Assert.AreEqual("fr-FR", options.LocaleStr);
			Assert.AreEqual("./bin/Debug", options.OutDir);
			Assert.AreEqual("Examples.Hello.Messages", options.BaseName);
			Assert.AreEqual("gmcs", options.CompilerName);
			Assert.AreEqual("./../../Bin", options.LibDir);
			Assert.IsTrue(options.Verbose);
		}
	}
}

