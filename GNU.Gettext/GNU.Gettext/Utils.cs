using System;
using System.IO;
using System.Text;

namespace GNU.Gettext.Utils
{
	public static class FileUtils
	{
		public static string GetRelativeUri(string uriString, string relativeUriString)
		{
			if ((!uriString.EndsWith("\\") || !uriString.EndsWith("/")) &&
			    (relativeUriString.EndsWith("\\") || relativeUriString.EndsWith("/")))
			    relativeUriString += "dummy";
			Uri fileUri = new Uri(uriString);
			Uri dirUri = new Uri(relativeUriString);
			Uri relativeUri = dirUri.MakeRelativeUri(fileUri);
			return relativeUri.ToString();
		}
	}
}

