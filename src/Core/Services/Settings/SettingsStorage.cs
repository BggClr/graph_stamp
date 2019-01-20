using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Stamp.Services.Settings
{
	public class SettingsStorage
	{
		private Models.Settings _settings;

		public static string SettingsFilePath => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "stamp.json");

		public SettingsStorage(Models.Settings settings)
		{
			_settings = settings;
		}

		public Models.Settings Get()
		{
			return _settings;
		}

		public void Set(Models.Settings settings)
		{
			_settings = settings;
			File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(_settings, new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}));
		}
	}
}
