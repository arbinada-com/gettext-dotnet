using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.IO;

namespace GNU.Gettext.Msgfmt
{
    public class ResourcesGen
    {
		public CmdLineOptions Options { get; private set; }

        public ResourcesGen(CmdLineOptions options)
        {
			this.Options = options;
        }

        public void Run()
        {
			Catalog catalog = new Catalog();
			catalog.Load(Options.InputFile);

            using (ResourceWriter writer = new ResourceWriter(Options.OutFile))
            {
                foreach (CatalogEntry entry in catalog)
                {
                    try
					{
						writer.AddResource(entry.String, entry.IsTranslated ? entry.GetTranslation(0) : entry.String);
					}
                    catch (Exception e)
					{
						throw new Exception(String.Format("Error adding item {0}", entry.String), e);
					}
                }
                writer.Generate();
            }
        }
    }
}
