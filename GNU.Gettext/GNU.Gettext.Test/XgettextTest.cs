using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

using GNU.Gettext.Xgettext;

namespace GNU.Gettext.Test
{
	[TestFixture()]
	public class XgettextTest
	{
		[Test()]
		public void ParsingTest()
		{
			string ressourceId = String.Format("{0}.{1}", this.GetType().Assembly.GetName().Name, "Data.XgettextTest.txt");
			string text = "";
			using (Stream stream = this.GetType().Assembly.GetManifestResourceStream(ressourceId))
			{
		        using (StreamReader reader = new StreamReader(stream))
				{
					text = reader.ReadToEnd();
				}
			}
			
			Options options = new Options();
			options.InputFile = @"\Test\File\Name.cs";
			options.OutFile = @"D:\MEDS\Main\Sources\Framework\PresentationLayer\DxCareShell\po\Text.pot";
			options.DefaultPluralsNumberOfTranslations = 2;
			ExtractorCsharp extractor = new ExtractorCsharp(options);
			extractor.GetMessages(text, options.InputFile);
			extractor.Save();
			Assert.AreEqual(1, extractor.Catalog.PluralFormsCount, "Plural string");
			Assert.AreEqual(11, extractor.Catalog.Count, "Not all strings was extracted");
		}
	}
}

