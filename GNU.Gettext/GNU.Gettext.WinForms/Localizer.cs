using System;
using System.Windows.Forms;

namespace GNU.Gettext.WinForms
{
	public class Localizer
	{
		
		public delegate void OnIterateControl(Control control);
		
		#region Constructors
		public Localizer(Control control, string resourceBaseName)
		{
            GettextResourceManager catalog = new GettextResourceManager(resourceBaseName);
			Localize(control, catalog);
		}
		#endregion
		
		#region Public interface
		public static void Localize(Control control, GettextResourceManager catalog)
		{
			IterateControls(control, catalog, IterateMode.Localize);
		}

		public static void Revert(Control control)
		{
			IterateControls(control, null, IterateMode.Revert);
		}
		#endregion
		
		#region Handlers for different types
		enum IterateMode
		{
			Localize,
			Revert
		}
		
		private static void IterateControlHandler(LocalizableObjectAdapter adapter, GettextResourceManager catalog, IterateMode mode)
		{
			switch (mode)
			{
			case IterateMode.Localize:
				string text = adapter.GetText();
				if (text != null)
					adapter.SetText(catalog.GetString(text));
				break;
			case IterateMode.Revert:
				adapter.Revert();
				break;
			}
		}
		
		#endregion
		
		private static void IterateControls(Control control, GettextResourceManager catalog, IterateMode mode)
		{
			if (control is ContainerControl)
			{
				foreach(Control child in (control as ContainerControl).Controls)
				{
					IterateControls(child, catalog, mode);
				}
			}
			
			if (control is ToolStrip)
			{
				foreach(ToolStripItem item in (control as ToolStrip).Items)
				{
					IterateToolStripItems(item, catalog, mode);
				}
			}
			
			IterateControlHandler(new LocalizableObjectAdapter(control), catalog, mode);
		}
		
		private static void IterateToolStripItems(ToolStripItem item, GettextResourceManager catalog, IterateMode mode)
		{
			if (item is ToolStripDropDownItem)
			{
				foreach(ToolStripItem subitem in (item as ToolStripDropDownItem).DropDownItems)
				{
					IterateToolStripItems(subitem, catalog, mode);
				}
			}
			IterateControlHandler(new LocalizableObjectAdapter(item), catalog, mode);
		}
	}
}

