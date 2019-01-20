using Models.Exceptions;
using Stamp.Services.Settings;

namespace Stamp.Services
{
	public class GithubAuthentication
	{
		private SettingsStorage _settingsStorage;

		public GithubAuthentication(SettingsStorage settingsStorage)
		{
			_settingsStorage = settingsStorage;
		}

		public string GetAuthorizationHeader()
		{
			var settings = _settingsStorage.Get();

			if (string.IsNullOrEmpty(settings.GithubAuth?.Username) || string.IsNullOrEmpty(settings.GithubAuth?.Token))
			{
				throw new AuthenticationMissingException();
			}

			var authString = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{settings.GithubAuth.Username}:{settings.GithubAuth.Token}"));

			return $"Basic {authString}";
		}
	}
}
