using System;
using System.Windows.Forms;

namespace GNU.Gettext.WinForms
{
	public class Localizer
	{

		public Localizer(Control control, string resourceBaseName)
		{
            GettextResourceManager catalog = new GettextResourceManager(resourceBaseName);
			Localize(control, catalog);
		}

		public static void Localize(Control control, GettextResourceManager catalog)
		{
			if (control.Tag == null)
				control.Tag = control.Text;
			control.Text = catalog.GetString(control.Text);
			if (control is ContainerControl)
			{
				foreach(Control child in (control as ContainerControl).Controls)
				{
					Localize(child, catalog);
				}
			}
		}

		public static void Revert(Control control)
		{
			if (control.Tag  != null && control.Tag is string)
				control.Text = (control.Tag as string);
			if (control is ContainerControl)
			{
				foreach(Control child in (control as ContainerControl).Controls)
				{
					Revert(child);
				}
			}
		}
	}
}

