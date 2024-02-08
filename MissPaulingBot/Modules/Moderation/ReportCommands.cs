using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Extensions;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation;

[Name("Report Commands")]
public class ReportCommands : DiscordApplicationGuildModuleBase
{
    private readonly PaulingDbContext _db;

    public ReportCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [MessageCommand("Report Message")]
    public async Task<IResult> ReportMessageAsync(IMessage message)
    {
        if (await _db.BlacklistedUsers.FindAsync(Context.AuthorId.RawValue) is { } blacklistedUser)
            return Response("You cannot report users if you are blacklisted.").AsEphemeral();

        var userMessage = message as IUserMessage;

        if (userMessage is null)
            return Response("You cannot report this type of message.").AsEphemeral();

        if (message.Author.IsBot)
            return Response("You cannot report a bot.").AsEphemeral();

        if (message.Author.Id == Context.AuthorId)
            return Response("You cannot report yourself.").AsEphemeral();

        var modal = new LocalInteractionModalResponse().WithTitle("Report Message")
            .WithCustomId($"Report:Message:{message.Id}:{message.ChannelId}").WithComponents(new LocalRowComponent().WithComponents(
                new LocalTextInputComponent().WithLabel("Why are you reporting this message?").WithStyle(TextInputComponentStyle.Paragraph).WithPlaceholder(
                    "Enter your report reason here. Please put a lot of details so that we can work on it efficiently.").WithMaximumInputLength(1500).WithCustomId("reason")));

        await Context.Interaction.Response().SendModalAsync(modal);
        return default!;
    }

    [UserCommand("Report User")]
    public async Task<IResult> ReportUserAsync(IUser user)
    {
        if (await _db.BlacklistedUsers.FindAsync(Context.AuthorId.RawValue) is { } blacklistedUser)
            return Response("You cannot report users if you are blacklisted.").AsEphemeral();

        if (user.IsBot)
            return Response("You cannot report a bot.").AsEphemeral();

        if (user.Id == Context.AuthorId)
            return Response("You cannot report yourself.").AsEphemeral();

        var modal = new LocalInteractionModalResponse().WithTitle("Report User")
            .WithCustomId($"Report:User:{user.Id}:{user is IMember}").WithComponents(new LocalRowComponent().WithComponents(
                new LocalTextInputComponent().WithLabel("Why are you reporting this user?").WithStyle(TextInputComponentStyle.Paragraph).WithPlaceholder(
                    "Enter a report reason. Use modmail for scammer and more complex reports.").WithMaximumInputLength(1500).WithCustomId("reason")));

        await Context.Interaction.Response().SendModalAsync(modal);
        return default!;
    }
    
}