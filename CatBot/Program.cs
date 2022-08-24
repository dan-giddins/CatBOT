using CatBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CatBot
{
	internal class Program
	{
		private static void Main(string[] args) =>
			new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			using (var services = ConfigureServices())
			{
				var client = services.GetRequiredService<DiscordSocketClient>();
				client.Log += LogAsync;
				services.GetRequiredService<CommandService>().Log += LogAsync;
				var waitForReady = new TaskCompletionSource<bool>();
				client.Ready += async () => waitForReady.SetResult(true);
				await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("CatBotToken"));
				await client.StartAsync();
				await services.GetRequiredService<ListenService>().Initialize();
				await services.GetRequiredService<SpeakService>().Initialize(waitForReady);
				await Task.Delay(Timeout.Infinite);
			}
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices() =>
			new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<ListenService>()
				.AddSingleton<SpeakService>()
				.AddSingleton<HttpClient>()
				.AddSingleton<PictureService>()
				.BuildServiceProvider();
	}
}