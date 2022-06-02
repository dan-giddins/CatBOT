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
            ListenForVoiceJoin();
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
                await Task.Delay(rnd.Next() / 1000);
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

        private async Task ListenForVoiceJoin() =>
            _discord.UserVoiceStateUpdated += (user, before, after)
            =>
            {
                if (user.Id != _discord.CurrentUser.Id
                    && after.VoiceChannel is not null)
                {
                    return after.VoiceChannel.ConnectAsync();
                }
                return null;
            };

        private IEnumerable<SocketTextChannel> GetAllTextChannels()
            => _discord.Guilds.SelectMany(x => x.TextChannels);

        private IEnumerable<SocketVoiceChannel> GetAllVoiceChannels()
            => _discord.Guilds.SelectMany(x => x.VoiceChannels);

        private IReadOnlyCollection<SocketGuild> GetAllGuilds()
            => _discord.Guilds;
    }
}
