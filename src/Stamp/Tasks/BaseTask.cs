using System;
using System.Threading.Tasks;
using Models;
using Models.Exceptions;
using Stamp.Services.Settings;

namespace Stamp.Tasks
{
	public abstract class BaseTask<T>
	{
		private readonly SettingsStorage _settingsStorage;

		protected BaseTask(SettingsStorage settingsStorage)
		{
			_settingsStorage = settingsStorage;
		}

		protected void AskAuthenticationDetails()
		{
			string username = null;
			string token = null;

			while (string.IsNullOrWhiteSpace(username))
			{
				Console.Write("username: ");
				username = Console.ReadLine()?.Trim();
			}
			while (string.IsNullOrWhiteSpace(token))
			{
				Console.Write("token: ");
				token = Console.ReadLine()?.Trim();
			}

			var settings = _settingsStorage.Get();
			settings.GithubAuth = new GithubAuth
			{
				Username = username,
				Token = token
			};
			_settingsStorage.Set(settings);
		}

		public async Task Execute(T args)
		{
			while (true)
			{
				try
				{
					await ExecuteInternal(args);
					break;
				}
				catch (AuthenticationMissingException)
				{
					AskAuthenticationDetails();
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine("Operation has been cancelled");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
					break;
				}
			}
		}

		protected abstract Task ExecuteInternal(T args);
	}
}
