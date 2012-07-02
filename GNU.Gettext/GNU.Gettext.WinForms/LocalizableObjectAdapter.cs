using System;
using System.Reflection;

namespace GNU.Gettext.WinForms
{
	public class LocalizableObjectAdapter
	{
		public object Source { get; private set; }
		
		#region Constructors
		public LocalizableObjectAdapter(object source)
		{
			this.Source = source;
		}
		#endregion
		
		public string GetText()
		{
			return GetPropertyValue("Text");
		}
		
		public void SetText(string text)
		{
			string originalText = GetOriginalText();
			if (originalText == null)
				SetOriginalText(GetText());
			SetPropertyValue("Text", text);
		}
		
		public void Revert()
		{
			string originalText = GetOriginalText();
			if (originalText != null)
				SetText(originalText);
		}
		
		private string GetOriginalText()
		{
			return GetPropertyValue("Tag");
		}
		
		private void SetOriginalText(string text)
		{
			SetPropertyValue("Tag", text);
		}
		
		private string GetPropertyValue(string name)
		{
			PropertyInfo pi = Source.GetType().GetProperty(name);
			if (pi != null && pi.CanRead)
			{
				object value = pi.GetValue(Source, null);
				return value != null && value is string ? (string)value : null;
			}
			return null;
		}
		
		private void SetPropertyValue(string name, string value)
		{
			PropertyInfo pi = Source.GetType().GetProperty(name);
			if (pi != null && pi.CanWrite)
			{
				pi.SetValue(Source, value, null);
			}
		}
	}
}

