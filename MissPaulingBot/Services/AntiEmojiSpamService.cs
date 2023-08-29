using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Caching.Memory;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services
{
    public sealed class AntiEmojiSpamService : DiscordBotService
    {
        private static readonly Regex EmojiRegex = new Regex(@"(?<default>[^\u0000-\uD83C]{2,6})|(?<custom><(a|):[\w]+:[0-9]{16,18}>)", RegexOptions.Compiled);
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        private const int MIN_CONTENT_LENGTH = 20;
        private const int BUCKET_LIFETIME = 15;
        private const int INFRACTION_LIMIT = 4;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Message.Author.IsBot || e.GuildId is null) return;

            if (e.Message.Author is CachedMember member && (member.RoleIds.Contains(Constants.DEATH_MERCHANT_ROLE_ID) ||
                                                            member.RoleIds.Contains(Constants.COMMUNITY_MOD_ROLE_ID) ||
                                                            member.RoleIds.Contains(Constants.JUNIOR_MOD_ROLE_ID))) return;

            if (string.IsNullOrWhiteSpace(e.Message.Content)) return;

            var emojiCount = 0;
            var content = EmojiRegex.Replace(e.Message.Content, Remove);

            if (emojiCount >= 6 || emojiCount > 0 && content.Length < MIN_CONTENT_LENGTH)
            {
                if (Cache.TryGetValue((e.Message.Author.Id, e.Message.ChannelId),
                    out (HashSet<Snowflake> MessageIds, int EmojiCount) tuple))
                {
                    tuple.MessageIds.Add(e.Message.Id);
                    tuple.EmojiCount += emojiCount;

                    if (tuple.EmojiCount >= INFRACTION_LIMIT)
                    {
                        await Client.DeleteMessagesAsync(e.Message.ChannelId, tuple.MessageIds);
                        await TrySendMessageAsync(e.Message);
                        Cache.Remove((e.Message.Author.Id, e.Message.ChannelId));
                        return;
                    }

                    Cache.Set((e.Message.Author.Id, e.Message.ChannelId), tuple, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(BUCKET_LIFETIME)));
                    return;
                }

                var newTuple = Cache.Set((e.Message.Author.Id, e.Message.ChannelId),
                    (new HashSet<Snowflake> { e.Message.Id }, emojiCount), new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(BUCKET_LIFETIME)));

                if (newTuple.Item2 >= INFRACTION_LIMIT)
                {
                    await Client.DeleteMessagesAsync(e.Message.ChannelId, newTuple.Item1);
                    await TrySendMessageAsync(e.Message);
                    Cache.Remove((e.Message.Author.Id, e.Message.ChannelId));

                }
            }

            string Remove(Match m)
            {
                emojiCount++;

                return string.Empty;
            }
        }

        private async Task TrySendMessageAsync(IMessage message)
        {
            try
            {
                await message.Author.SendMessageAsync(new LocalMessage()
                    .WithContent(
                    $"{message.Author.Mention}, Please refrain from sending groups of messages full of emojis and not much else," +
                    $" as it can be spammy or not contribute very much to chat. Your messages have been automatically deleted." +
                    $" If you have any questions or concerns, please contact a moderator or {Client.CurrentUser.Mention}.")
                    .WithAllowedMentions(new LocalAllowedMentions().WithUserIds(message.Author.Id)));
            }
            catch
            {
                var toDelete = await Client.SendMessageAsync(message.ChannelId, new LocalMessage()
                    .WithContent(
                    $"{message.Author.Mention}, Please refrain from sending groups of messages full of emojis and not much else," +
                    $" as it can be spammy or not contribute very much to chat. Your messages have been automatically deleted." +
                    $" If you have any questions or concerns, please contact a moderator or {Client.CurrentUser.Mention}." +
                    $" (You were sent this message directly in chat because your DMs are restricted," +
                    $" make sure to go to your notification settings and allow DMs from members if you want your verbal warnings private in the future.)")
                    .WithAllowedMentions(new LocalAllowedMentions().WithUserIds(message.Author.Id)));
                await Task.Delay(TimeSpan.FromSeconds(10));
                await toDelete.DeleteAsync();
            }

            await Client.SendMessageAsync(742442765450870905, new LocalMessage()
                .WithContent($"User {message.Author} ({message.Author.Id}) has triggered the anti-spam emote filter.")
                );
        }
    }
}