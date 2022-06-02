using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace CatBot
{
    public class Program : IDisposable
    {
        private DiscordSocketClient? _client = null;

        public static void Main(string[] args) =>
            new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            var auth = JObject.Parse(File.ReadAllText("auth.json"));
            var token = auth["token"].ToString();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            _client.MessageUpdated += MessageUpdated;
            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };
            await Task.Delay(-1);
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        private async Task Log(LogMessage msg) =>
            Console.WriteLine(msg.ToString());

        public void Dispose() => throw new NotImplementedException();
    }
}