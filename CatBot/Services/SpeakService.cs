using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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

        public async Task Initialize(TaskCompletionSource<bool> tcs)
        {
            // TODO: make this a callback
            //await Task.Delay(5000);
            await tcs.Task;
            GreetingMeow();
            RandomMeow();
            AddListenForVoiceJoin();
        }

        private void GreetingMeow()
        {
            foreach (var channel in GetAllTextChannels())
            {
                try
                {
                    channel.SendMessageAsync("*meows happily*");
                }
                catch (NotSupportedException)
                { }
            }
        }

        private async Task RandomMeow()
        {
            var rnd = new Random();
            while (true)
            {
                await Task.Delay((int)(rnd.NextDouble() * 1000000));
                foreach (var channel in GetAllTextChannels())
                {
                    try
                    {
                        channel.SendMessageAsync("*meows randomly*");
                    }
                    catch (NotSupportedException)
                    { }
                }
            }
        }

        private void AddListenForVoiceJoin() =>
            _discord.UserVoiceStateUpdated += async (user, before, after) =>
            {
                if (user.Id != _discord.CurrentUser.Id
                    && after.VoiceChannel is not null
                    && !after.VoiceChannel.ConnectedUsers.Any(x => x.Id == _discord.CurrentUser.Id))
                {
                    var currentGuild = after.VoiceChannel.Guild;
                    // connect to new vc
                    Console.WriteLine($"Connecting...");
                    Task.Run(() => ConnectToVoice(after.VoiceChannel));
                }
            };

        private async Task ConnectToVoice(SocketVoiceChannel voiceChannel)
        {
            try
            {
                await Task.Delay(1000);
                var connection = await voiceChannel.ConnectAsync();
                Console.WriteLine($"Connected!");
                await MeowOnVoice(connection);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task canceled");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Already connected");
            }
        }

        private async Task MeowOnVoice(IAudioClient connection)
        {
            Console.WriteLine($"Meowing...");
            await SendAsync(connection, "Audio/meow.m4a");
            Console.WriteLine($"Finished meowing");
        }

        private Process CreateStream(string path) =>
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

        private async Task SendAsync(IAudioClient client, string path)
        {
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        private IEnumerable<SocketTextChannel> GetAllTextChannels()
            => _discord.Guilds.SelectMany(x => x.TextChannels);

        private IEnumerable<SocketVoiceChannel> GetAllVoiceChannels()
            => _discord.Guilds.SelectMany(x => x.VoiceChannels);

        private IReadOnlyCollection<SocketGuild> GetAllGuilds()
            => _discord.Guilds;
    }
}
