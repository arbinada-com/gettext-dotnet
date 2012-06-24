using System;

using NUnit.Framework;

namespace GNU.Gettext.Test
{
	[TestFixture()]
	public class CatalogTest
	{
		[Test()]
		public void ParsingTest()
		{
			Catalog cat = new Catalog();
			cat.Load("./Data/Test01.po");

			Assert.AreEqual(4, cat.Count, "Entries count");
			Assert.AreEqual(2, cat.PluralFormsCount, "Plurals entries count");

			int nonTranslatedCount = 0;
			foreach(CatalogEntry entry in cat)
			{
				if (!entry.IsTranslated)
					nonTranslatedCount++;
				if (entry.HasPlural)
				{
					Assert.AreEqual("{0} erreur fatale trouvée", entry.GetTranslation(0));
					Assert.AreEqual("{0} erreurs fatales trouvées", entry.GetTranslation(1));
				}
			}

			Assert.AreEqual(1, nonTranslatedCount, "Non translated strings count");
		}
	}
}

