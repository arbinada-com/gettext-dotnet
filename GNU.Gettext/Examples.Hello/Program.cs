// Example for use of GNU gettext.
// This file is in the public domain.
//
// Source code of the C# program.

using System;
using System.Diagnostics;
using System.Resources;
using System.Globalization;
using System.Reflection;

using GNU.Gettext;

namespace GNU.Gettext.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
			ShowMessages();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
			ShowMessages();
            Console.ReadKey();
        }

		static void ShowMessages()
		{
            Console.WriteLine("Current culture {0}", System.Threading.Thread.CurrentThread.CurrentUICulture);
            GettextResourceManager catalog = new GettextResourceManager("Examples.Hello.Messages");
            Console.WriteLine(catalog.GetString("Hello, world!"));
            Console.WriteLine(catalog.GetStringFmt("This program is running as process number \"{0}\".",
			                  Process.GetCurrentProcess().Id));
            Console.WriteLine(String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 1),
				0));
            Console.WriteLine(String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 1),
				1));
            Console.WriteLine(String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 5),
				5));
            Console.WriteLine(catalog.GetString("Here is an example of how one might continue a very long string\nfor the common case the string represents multi-line output.\n"));
		}
    }
}
