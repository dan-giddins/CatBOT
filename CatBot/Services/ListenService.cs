using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CatBot.Services
{
    public class ListenService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public ListenService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task Initialize() =>
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }
            if (message.Source != MessageSource.User)
            {
                return;
            }
            var argPos = 0;
            //if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            if (!message.HasCharPrefix('!', ref argPos))
            {
                return;
            }
            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(
            Optional<CommandInfo> command,
            ICommandContext context,
            IResult result)
        {
            if (!command.IsSpecified)
            {
                return;
            }
            if (result.IsSuccess)
            {
                return;
            }
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}
