using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CatBot.Services
{
    public class SpeakService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public SpeakService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(5000);
            await GreetingMeow();
            RandomMeow();
        }

        private async Task GreetingMeow()
        {
            foreach (var channel in GetAllTextChannels())
            {
                try
                {
                    channel.SendMessageAsync("*meows happily*");
                }
                catch (System.NotSupportedException e)
                { }
            }
        }

        private async Task RandomMeow()
        {
            var rnd = new Random();
            while (true)
            {
                await Task.Delay(rnd.Next() / 10000);
                foreach (var channel in GetAllTextChannels())
                {
                    try
                    {
                        channel.SendMessageAsync("*meows randomly*");
                    }
                    catch (System.NotSupportedException e)
                    { }
                }
            }
        }

        private IEnumerable<SocketTextChannel> GetAllTextChannels()
            => _discord.Guilds.SelectMany(x => x.TextChannels);
    }
}
