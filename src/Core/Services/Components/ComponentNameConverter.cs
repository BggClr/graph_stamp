using System.Text.RegularExpressions;

namespace Stamp.Services.Components
{
	public class ComponentNameConverter
	{
		private static readonly Regex _componentNameParser = new Regex($"{Settings.ComponentPrefix}(?<Category>[^_]+)_(?<Name>.*)", RegexOptions.IgnoreCase);

		public static string GetCategory(string rawName)
		{
			return _componentNameParser.Match(rawName).Groups["Category"].Value;
		}

		public static string GetName(string rawName)
		{
			return _componentNameParser.Match(rawName).Groups["Name"].Value;
		}

		public static string GetLocalName(string name)
		{
			return System.Globalization.CultureInfo.InvariantCulture.TextInfo
				.ToTitleCase(name).Replace("_", string.Empty);
		}
	}
}
