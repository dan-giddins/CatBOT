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

        public async Task Initialize(TaskCompletionSource<bool> waitForReady)
        {
            await waitForReady.Task;
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
                    channel.SendMessageAsync("*meows hello*");
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
                await Task.Delay((int)(rnd.NextDouble() * 100000));
                foreach (var channel in GetAllTextChannels())
                {
                    try
                    {
                        channel.SendMessageAsync("*meows for attention*");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
                foreach (var channel in GetAllVoiceChannels())
                {
                    try
                    {
                        await Task.Delay(1000);
                        await MeowOnVoice(await channel.ConnectAsync(), "Audio/meow_attention.m4a");
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeoutException");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("OperationCanceledException");
                    }
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
                    // connect to new vc
                    Console.WriteLine($"Connecting to {after.VoiceChannel}...");
                    Task.Run(() => ConnectToVoice(after.VoiceChannel, user));
                }
            };

        private async Task ConnectToVoice(
            SocketVoiceChannel voiceChannel,
            SocketUser user)
        {
            try
            {
                await Task.Delay(1000);
                var connection = await voiceChannel.ConnectAsync();
                Console.WriteLine($"Connected to {voiceChannel}.");
                MeowInTextChat(connection, user);
                await MeowOnVoice(connection, "Audio/meow_hello.m4a");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("TaskCanceledException");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("InvalidOperationException");
            }
        }

        private void MeowInTextChat(IAudioClient connection, SocketUser user)
        {
            foreach (var channel in GetAllTextChannels())
            {
                try
                {
                    channel.SendMessageAsync($"*meows at {user}*");
                }
                catch (NotSupportedException)
                { }
            }
        }

        private async Task MeowOnVoice(IAudioClient connection, string audioPath)
        {
            Console.WriteLine($"Meowing {audioPath}...");
            await SendAsync(connection, audioPath);
            Console.WriteLine($"Finished meowing {audioPath}.");
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
                await output.CopyToAsync(discord);
            }
        }

        private IEnumerable<SocketTextChannel> GetAllTextChannels() =>
            _discord.Guilds.SelectMany(x => x.TextChannels);

        private IEnumerable<SocketVoiceChannel> GetAllVoiceChannels() =>
            _discord.Guilds.SelectMany(x => x.VoiceChannels);

        private IReadOnlyCollection<SocketGuild> GetAllGuilds() =>
            _discord.Guilds;
    }
}
