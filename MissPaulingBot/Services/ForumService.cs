using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class ForumService : DiscordBotService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var posts = await Bot.FetchActiveThreadsAsync(Constants.TF2_GUILD_ID);

            foreach (var post in posts.Where(x => x.ChannelId == Constants.HELP_ADVICE_FORUM_ID))
            {
                if (post.Metadata.IsArchived)
                {
                    continue;
                }

                if (!post.LastMessageId.HasValue)
                {
                    continue;
                }

                IMessage message;

                try
                { 
                    message = Bot.GetMessage(post.Id, post.LastMessageId.Value) ??
                            await post.FetchMessageAsync(post.LastMessageId.Value);

                    var lastFive = await Bot.FetchMessagesAsync(post.Id, 5);

                    if (lastFive.Any(x => x.Author.IsBot))
                        continue;
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Could not fetch ForumService message");
                    continue;
                }
                

                if (message is null)
                {
                    continue;
                }

                if (message.Author.IsBot)
                {
                    continue;
                }

                if (DateTimeOffset.UtcNow - message.CreatedAt() < TimeSpan.FromHours(12))
                {
                    continue;
                }

                await post.SendMessageAsync(new LocalMessage()
                        .WithContent(
                            $"Reminder: {Mention.User(post.CreatorId)} Please make sure to close your post once you have received your answer or found the solution. Make sure to include the solution so others can also see it and use it. You can also select the solution message, go to Apps > Mark As Solution.")
                        .WithAllowedMentions(new LocalAllowedMentions().WithUserIds(post.CreatorId)));
                Logger.LogInformation("Message sent for post {PostId}!", post.Id.RawValue);
            }

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    protected override async ValueTask OnThreadCreated(ThreadCreatedEventArgs e)
    {
        if (e.IsThreadCreation)
        {
            await e.Thread.JoinAsync();
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(10));

        var messages = await e.Thread.FetchMessagesAsync(5);

        if (messages.Any(x => x.Author.Id == Bot.CurrentUser.Id))
            return;

        if (e.Thread.ChannelId == Constants.HELP_ADVICE_FORUM_ID)
        {
            await Bot.SendMessageAsync(e.ThreadId,
                new LocalMessage().WithAllowedMentions(new LocalAllowedMentions().WithUserIds(e.Thread.CreatorId))
                    .WithContent(
                        $"{Mention.User(e.Thread.CreatorId)}, please use the close button below or the {Mention.SlashCommand(1025851488624455730, "close")} command once your question has been answered.\n" +
                        "While you are waiting for an answer, check out our official FAQ! Your question may have been answered already: https://wiki.tf2.community/faq/")
                    .WithComponents(new LocalRowComponent().WithComponents(new LocalButtonComponent().WithLabel("Close")
                        .WithCustomId($"Forum:Close:{e.ThreadId}").WithStyle(LocalButtonComponentStyle.Success))));
        }

        if (e.Thread.TagIds.Contains((Snowflake)1021178781626535936))
        {
            await e.Thread.AddReactionAsync(e.ThreadId, LocalEmoji.Custom(796593169894604810));
            await e.Thread.AddReactionAsync(e.ThreadId, LocalEmoji.Custom(796593148452536350));
        }
    }

    public async Task CloseThreadAsync(IThreadChannel thread)
    {
        await thread.ModifyAsync(x =>
        {
            x.IsArchived = true;
            x.IsLocked = true;
        });

        // await thread.ModifyAsync(x => x.IsArchived = true);
    }
}