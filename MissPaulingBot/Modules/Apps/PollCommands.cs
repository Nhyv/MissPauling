using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Apps;

[SlashGroup("poll")]
[Description("Poll commands.")]
public class PollCommands : DiscordApplicationModuleBase
{
    private readonly PaulingDbContext _db;

    public PollCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("create")]
    [RequireGuild]
    [Description("Creates a poll.")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> CreatePollAsync([Description("Poll name")] string name, [Description("Poll content")] string content,
        [Description("Where the poll will be sent.")] [ChannelTypes(ChannelType.Text)] IChannel channel,
        [Description("Time in Unix")] long openPollAfter, [Description("Time in Unix")] long removeVotingAfter, [Description("Displays results on the poll")] bool displayResultsPublicly)
    {
        _db.Polls.Add(new Poll
        {
            Name = name,
            Content = content,
            ChannelId = channel.Id.RawValue,
            OpenPollAfter = DateTimeOffset.FromUnixTimeSeconds(openPollAfter),
            RemoveVotingAfter = DateTimeOffset.FromUnixTimeSeconds(removeVotingAfter),
            DisplayVotesPublicly = displayResultsPublicly,
        });

        await _db.SaveChangesAsync();
        return Response("Poll was created.");
    }

    [SlashCommand("createoption")]
    [RequireGuild]
    [Description("Creates a poll option.")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> CreatePollOptionAsync([Description("The poll")] int pollChoice, [Description("The option content on the button")] string content)
    {
        var poll = await _db.Polls.FindAsync(pollChoice);

        if (poll is null)
            return Response("Invalid poll.");

        var createdOption = _db.PollOptions.Add(new PollOption
        {
            Content = content,
            Poll = poll
        }).Entity;

        await _db.SaveChangesAsync();

        poll.Options.Add(createdOption);
        await _db.SaveChangesAsync();

        return Response("Option created.");
    }

    [SlashCommand("status")]
    [Description("View a poll's status")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> ViewPollStatus([Description("The poll")] int pollChoice)
    {
        var poll = await _db.Polls.Include(x => x.Options).FirstOrDefaultAsync(x => x.Id == pollChoice);

        if (poll is null)
            return Response("Invalid poll.");

        var embed = EmbedUtilities.SuccessBuilder.WithTitle($"{poll.Name} status")
            .AddField("**Poll description (will be in the post)**", poll.Content).AddField("**State**", poll.State).AddField("**Options**",
                poll.Options.Count == 0
                    ? "No options configured"
                    : string.Join(',', poll.Options.Select(x => x.Content))).AddField("**The poll will open after**",
                $"{Markdown.Timestamp(poll.OpenPollAfter, Markdown.TimestampFormat.LongDateTime)}\n{Markdown.Timestamp(poll.OpenPollAfter, Markdown.TimestampFormat.RelativeTime)}")
            .AddField("**The voting will close after**",
                $"{Markdown.Timestamp(poll.RemoveVotingAfter, Markdown.TimestampFormat.LongDateTime)}\n{Markdown.Timestamp(poll.RemoveVotingAfter, Markdown.TimestampFormat.RelativeTime)}")
            .AddField("**Results will be publicly available if true**", poll.DisplayVotesPublicly);

        return Response(embed);
    }

    [SlashCommand("obliterate")]
    [Description("OBLITERATES one poll. One.")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> ObliteratePollAsync([Description("The poll")] int pollChoice)
    {
        var poll = await _db.Polls.FindAsync(pollChoice);

        if (poll is null)
            return Response("Invalid poll.");

        _db.Remove(poll);
        await _db.SaveChangesAsync();
        return Response("Obliterated. Nuked. Turned to dust. Dust is gone. Farewell.");
    }


    [AutoComplete("status")]
    [AutoComplete("obliterate")]
    [AutoComplete("createoption")]
    public async Task AutoCompleteAllPollsAsync([Name("poll-choice")] AutoComplete<int> pollChoice)
    {
        var pollChoices = await _db.Polls.ToListAsync();

        pollChoice.Choices!.AddRange(pollChoices.Take(25).ToDictionary(x => x.Name, x => x.Id));
    }
}