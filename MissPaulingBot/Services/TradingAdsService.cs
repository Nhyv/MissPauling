using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;

namespace MissPaulingBot.Services;

public class TradingAdsService : DiscordBotService
{
    protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
    {
        if (e.ChannelId.RawValue != 994674314886512650)
            return;

        if (e.OldMessage!.Embeds.Count >= e.NewMessage!.Embeds.Count)
            return;

        foreach (var embed in e.NewMessage.Embeds)
        {
            if (embed.Image is null)
            {
                await e.NewMessage.DeleteAsync();

                var dm = await Bot.CreateDirectChannelAsync(e.NewMessage.Author.Id);

                _ = dm.SendMessageAsync(new LocalMessage().WithContent(
                    "Your trading ad has been deleted by our automatic filter because it did not respect the following criterion: Your trading ad links must have an image embed or be wrapped in < > (See the trading guidelines channel). If you believe this is an error, send a message to the modteam by replying to this DM channel."));
                return;
            }
        }

    }

    protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.ChannelId.RawValue != 994674314886512650)
            return;

        if (e.Message is IUserMessage { Attachments.Count: <= 0, Embeds.Count: <= 0 })
            return;

        var msg = (IUserMessage)e.Message;

        if (msg.Attachments.Count > 1)
        {
            await msg.DeleteAsync();

            var dm = await Bot.CreateDirectChannelAsync(e.AuthorId);

            try
            {
                await dm.SendMessageAsync(new LocalMessage().WithContent(
                    "Your trading ad has been deleted by our automatic filter because it did not respect the following criterion: Your trading ad must not contain more than one attachment. If you believe this is an error, send a message to the modteam by replying to this DM channel."));
                return;
            }
            catch (Exception)
            {
                //
            }
        }

        var imageCount = 0;
        foreach (var embed in msg.Embeds)
        {
            if (imageCount > 1)
            {
                await msg.DeleteAsync();

                var dm = await Bot.CreateDirectChannelAsync(e.AuthorId);

                _ = dm.SendMessageAsync(new LocalMessage().WithContent(
                    "Your trading ad has been deleted by our automatic filter because it did not respect the following criterion: Your trading ad links must have an image embed or be wrapped in < > (See the trading guidelines channel). If you believe this is an error, send a message to the modteam by replying to this DM channel."));
                return;
            }

            if (embed.Type?.Equals("image") == true)
                imageCount++;

            if (embed.Image is not null)
                imageCount++;
        }

        if (imageCount == 0)
        {
            await msg.DeleteAsync();

            var dm = await Bot.CreateDirectChannelAsync(e.AuthorId);

            _ = await dm.SendMessageAsync(new LocalMessage().WithContent(
                "Your trading ad has been deleted by our automatic filter because it did not respect the following criterion: Your trading ad links must have an image embed or be wrapped in < > (See the trading guidelines channel). If you believe this is an error, send a message to the modteam by replying to this DM channel."));
        }
    }
}