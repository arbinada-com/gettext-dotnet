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
			options.InputFile = @".\Test\File\Name.cs"; // File wont be used, feed the plain text
			options.OutFile = @"./Test.pot";
			options.Overwrite = true;
			ExtractorCsharp extractor = new ExtractorCsharp(options);
			extractor.GetMessages(text, options.InputFile);
			extractor.Save();
			
			int ctx = 0;
			foreach(CatalogEntry entry in extractor.Catalog)
			{
				if (entry.HasContext)
					ctx++;
			}
			
			Assert.AreEqual(2, ctx, "Context count");
			
			Assert.AreEqual(2, extractor.Catalog.PluralFormsCount, "PluralFormsCount");
			Assert.AreEqual(14, extractor.Catalog.Count, "Duplicates may not detected");
		}
	}
}

