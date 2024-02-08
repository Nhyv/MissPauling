using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Utilities;

namespace MissPaulingBot.Services;

public class PollService : DiscordBotService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = Bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

            var polls = await db.Polls.Include(x => x.Options).ToListAsync(stoppingToken);
            var wait = true;

            foreach (var poll in polls.Where(x => x.State == PollState.Voting && !x.MessageId.HasValue))
            {
                var embed = EmbedUtilities.SuccessBuilder.WithTitle($"Poll '{poll.Name}'")
                    .WithDescription(poll.Content).AddField("\u200b", $"This poll ends {Markdown.Timestamp(poll.RemoveVotingAfter, Markdown.TimestampFormat.RelativeTime)}");

                var rows = new List<LocalRowComponent>();
                var components = new List<LocalComponent>();
                foreach (var option in poll.Options)
                {
                    components.Add(option.ToComponent());

                    if (components.Count == 5)
                    {
                        rows.Add(new LocalRowComponent().WithComponents(components));
                        components.Clear();
                    }
                }

                if (components.Count > 0)
                {
                    rows.Add(new LocalRowComponent().WithComponents(components));
                }

                var message = await Bot.SendMessageAsync(poll.ChannelId, new LocalMessage().WithEmbeds(embed).WithComponents(rows), cancellationToken:stoppingToken);
                poll.MessageId = message.Id;
            }
            await db.SaveChangesAsync(stoppingToken);

            foreach (var poll in polls.Where(x => x.State == PollState.Completed && x.MessageId.HasValue))
            {
                var message = (IUserMessage)(await Bot.FetchMessageAsync(poll.ChannelId, poll.MessageId!.Value, cancellationToken:stoppingToken))!;

                if (message.Components.Count == 0)
                    continue;
                
                var embed = EmbedUtilities.SuccessBuilder.WithTitle("Poll Results").WithDescription(poll.Content);

                foreach (var option in poll.Options)
                {
                    var votes = await db.PollVotes.Where(x => x.OptionId == option.Id).ToListAsync(cancellationToken:stoppingToken);
                    embed.AddField(option.Content, $"{votes.Count} votes.");
                }

                if (poll.DisplayVotesPublicly)
                {
                    await message.ModifyAsync(x =>
                    {
                        x.Embeds = new List<LocalEmbed>(message.Embeds.Select(x => LocalEmbed.CreateFrom(x)))
                            .Append(embed).ToList();
                        x.Components = new List<LocalRowComponent>();
                    }, cancellationToken: stoppingToken);
                }
                else
                {
                    await Bot.SendMessageAsync(Constants.NSFW_CHANNEL_ID, new LocalMessage().WithEmbeds(embed));
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "**Poll has ended.**";
                        x.Components = new List<LocalRowComponent>();
                    });
                }
            }

            if (wait)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); 
            }
        }
    }
}