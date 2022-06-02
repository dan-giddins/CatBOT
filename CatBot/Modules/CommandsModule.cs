using CatBot.Services;
using Discord.Commands;

namespace TextCommandFramework.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        public PictureService _pictureService { get; set; }

        public CommandsModule(PictureService pictureService) =>
            _pictureService = pictureService;

        [Command("help")]
        [Alias("h")]
        public Task Help()
            => ReplyAsync("!help - Shows this message" +
                "\n!stroke - Give CatBOT some attention" +
                "\n!cat - CatBOT brings you a random picture of a cat");

        [Command("meow")]
        [Alias("ping", "hello", "m", "p")]
        public Task Meow()
            => ReplyAsync("*meows back at you*");

        [Command("stroke")]
        [Alias("s")]
        public Task Stroke()
            => ReplyAsync("*meows happily*");

        [Command("cat")]
        [Alias("c")]
        public async Task Cat()
        {
            // Get a stream containing an image of a cat
            var stream = await _pictureService.GetCatPictureAsync();
            // Streams must be seeked to their beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "cat.png");
        }

        //Get info on a user, or the user who invoked the command if one is not specified
        //[Command("userinfo")]
        // public async Task UserInfoAsync(IUser user = null)
        // {
        //     user ??= Context.User;

        //     await ReplyAsync(user.ToString());
        // }

        // Ban a user
        //[Command("ban")]
        //[RequireContext(ContextType.Guild)]
        //// make sure the user invoking the command can ban
        //[RequireUserPermission(GuildPermission.BanMembers)]
        //// make sure the bot itself can ban
        //[RequireBotPermission(GuildPermission.BanMembers)]
        //public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        //{
        //    await user.Guild.AddBanAsync(user, reason: reason);
        //    await ReplyAsync("ok!");
        //}

        //// [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        //[Command("echo")]
        //public Task EchoAsync([Remainder] string text)
        //    // Insert a ZWSP before the text to prevent triggering other bots!
        //    => ReplyAsync('\u200B' + text);

        //// 'params' will parse space-separated elements into a list
        //[Command("list")]
        //public Task ListAsync(params string[] objects)
        //    => ReplyAsync("You listed: " + string.Join("; ", objects));

        //// Setting a custom ErrorMessage property will help clarify the precondition error
        //[Command("guild_only")]
        //[RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        //public Task GuildOnlyCommand()
        //    => ReplyAsync("Nothing to see here!");
    }
}
