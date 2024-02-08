using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using Qommon;

namespace MissPaulingBot.Services;

public sealed class ContestService : DiscordBotService
{
    private readonly Random _random;

    public ContestService(Random random)
    {
        _random = random;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = Bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

            var contests = await db.Contests.ToListAsync(stoppingToken);
            var wait = true;

            foreach (var contest in contests.Where(x => x.State == ContestState.Voting))
            {
                var messages = await Bot.FetchMessagesAsync(contest.ChannelId, cancellationToken: stoppingToken);

                if (messages.MinBy(x => x.Id) is null or IUserMessage {Components.Count: > 0}) // no entries, or the first entry already has a button 
                {
                    continue;
                }

                wait = false;

                var submissions = await db.ContestSubmissions.Where(x => x.ContestId == contest.Id).ToListAsync(stoppingToken);
                foreach (var submission in submissions)
                {
                    try
                    {
                        var message = await Bot.FetchMessageAsync(contest.ChannelId, submission.MessageId,
                            cancellationToken: stoppingToken) as IUserMessage;
                        await message!.ModifyAsync(x =>
                        {
                            x.Content = new Optional<string?>(null);
                            x.Components = new[]
                            {
                                LocalComponent.Row(LocalComponent
                                    .Button($"Submission:Vote:{contest.Id}:{submission.Id}", "Vote!")
                                    .WithStyle(LocalButtonComponentStyle.Success))
                            };
                        }, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to fetch/modify contest submission message {MessageId} in channel {ChannelId}.",
                            contest.ChannelId, submission.MessageId);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(7, 13)), stoppingToken);
                }

                await Bot.SendMessageAsync(Constants.NSFW_CHANNEL_ID, new LocalMessage()
                        .WithContent(
                            $"{Mention.User(227578898521653249)}, {Mention.User(167452465317281793)}, make the voting channels for the contest \"{contest.Name}\" visible!!!!")
                        .WithAllowedMentions(new LocalAllowedMentions().WithParsedMentions(ParsedMentions.Users)),
                    cancellationToken: stoppingToken);
            }

            foreach (var contest in contests.Where(x => x.State == ContestState.Results))
            {
                var messages = await Bot.FetchMessagesAsync(contest.ChannelId, cancellationToken: stoppingToken);

                if (messages.MinBy(x => x.Id) is null or IUserMessage { Components.Count: 0 }) // no entries, or the first entry already has had buttons removed
                {
                    continue;
                }

                wait = false;

                var submissions = await db.ContestSubmissions.Where(x => x.ContestId == contest.Id).ToListAsync(stoppingToken);
                var rawVotes = await db.ContestVotes.Where(x => x.ContestId == contest.Id).ToListAsync(stoppingToken);
                var votes = rawVotes.GroupBy(x => x.SubmissionId)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var submission in submissions)
                {
                    try
                    {
                        var message = await Bot.FetchMessageAsync(contest.ChannelId, submission.MessageId,
                            cancellationToken: stoppingToken) as IUserMessage;
                        await message!.ModifyAsync(x =>
                        {
                            var place = votes.IndexOf(submission.Id) + 1;
                            if (place == 0) // no votes at all :(
                                place = submissions.Count;

                            var builder = new StringBuilder().AppendNewLine(place switch
                            {
                                1 => "🥇 1st place!!!!",
                                2 => "🥈 2nd place!!!",
                                3 => "🥉 3rd place!!",
                                _ => $"{place.Ordinalize(CultureInfo.CreateSpecificCulture("en-US"))} place!"
                            }).AppendNewLine(Mention.User(submission.CreatorId));
                            x.Content = builder.ToString();
                            x.Components = new List<LocalRowComponent>();
                        }, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to fetch/modify contest submission message {MessageId} in channel {ChannelId}.",
                            contest.ChannelId, submission.MessageId);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(7, 13)), stoppingToken);
                }

                await Bot.SendMessageAsync(Constants.NSFW_CHANNEL_ID, new LocalMessage()
                        .WithContent(
                            $"{Mention.User(227578898521653249)}, {Mention.User(167452465317281793)}, voting has concluded for the contest \"{contest.Name}\"!!!!")
                        .WithAllowedMentions(new LocalAllowedMentions().WithParsedMentions(ParsedMentions.Users)),
                    cancellationToken: stoppingToken);
            }

            if (wait)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}