using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services
{
    public class AntiMultiChannelSpammerService : DiscordBotService
    {
        private readonly Dictionary<Snowflake, List<IGatewayMessage>> _messages = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly HashSet<Snowflake> _alreadySoftbanned = new();

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Member is null || e.Member.RoleIds.Contains(Constants.MODERATOR_ROLE_ID) || _alreadySoftbanned.Contains(e.Message.Author.Id) || e.Member.IsBot) return;
            
            _ = Task.Run(async () =>
            {
                await _semaphore.WaitAsync();

                if (string.IsNullOrWhiteSpace(e.Message.Content))
                {
                    _semaphore.Release();
                    return;
                }

                if (e.Message.GuildId != Constants.TF2_GUILD_ID)
                {
                    _semaphore.Release();
                    return;
                }

                if (!_messages.TryGetValue(e.Message.Author.Id, out var messages))
                {
                    _messages.Add(e.Message.Author.Id, new List<IGatewayMessage>
                    {
                        e.Message
                    });
                    
                    _semaphore.Release();
                    return;
                }

                var now = DateTimeOffset.UtcNow;

                if (messages.All(x =>
                    x.Content == e.Message.Content && x.ChannelId != e.ChannelId &&
                    now - x.CreatedAt() <= TimeSpan.FromSeconds(15)))
                {
                    Logger.LogWarning($"User {e.Message.Author.Tag} sent the same thing more than once.");
                    if (messages.Count >= 2)
                    {
                        Logger.LogWarning($"I should be kicking {e.Message.Author.Tag}");
                        _ = e.Message.Author.SendMessageAsync(
                            new LocalMessage().WithContent(
                                "This is a message from the TF2 Community Discord. You were kicked because we believe your account was compromised or you spammed" +
                                " the same message in multiple channels. If you believe that this was a mistake, you may rejoin and contact our modteam by responding" +
                                " to this DM message."));

                        Logger.LogWarning($"I'm about to ban them.");
                        await Bot.CreateBanAsync(Constants.TF2_GUILD_ID, e.Message.Author.Id,
                            "Potential Scammer/Spammer (Same message in multiple channels)", 1);
                        await Bot.DeleteBanAsync(Constants.TF2_GUILD_ID, e.Message.Author.Id);
                        _messages.Remove(e.Message.Author.Id);

                        Logger.LogWarning($"Adding them to the already softbanned.");
                        _semaphore.Release();
                        _alreadySoftbanned.Add(e.Message.Author.Id);
                        Logger.LogWarning($"All done.");
                        return;
                    }

                    messages.Add(e.Message);
                    _semaphore.Release();

                    return;
                }

                _semaphore.Release();
                _messages.Remove(e.Message.Author.Id);
            });
        }
    }
}