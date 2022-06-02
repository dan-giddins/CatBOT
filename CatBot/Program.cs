using Discord;
using Discord.WebSocket;

namespace CatBot
{
    internal class Program
    {
        private readonly DiscordSocketClient _client;

        private static void Main(string[] args)
            => new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        public Program()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.InteractionCreated += InteractionCreatedAsync;
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("CatBotToken"));
            await _client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }
            if (message.Content == "!ping")
            {
                var cb = new ComponentBuilder().WithButton("Click me!", "unique-id", ButtonStyle.Primary);
                await message.Channel.SendMessageAsync("pong!", components: cb.Build());
            }
        }

        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            if (interaction is SocketMessageComponent component)
            {
                if (component.Data.CustomId == "unique-id")
                {
                    await interaction.RespondAsync("Thank you for clicking my button!");
                }
                else
                {
                    Console.WriteLine("An ID has been received that has no handler!");
                }
            }
        }
    }
}