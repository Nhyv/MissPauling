using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;

namespace MissPaulingBot.Services
{
    public class EmoteStatTrackerService : DiscordBotService
    {
        private static readonly Regex EmojiRegex = new(@"<(a|):[\w]+:(?<id>\d{16,20})>", RegexOptions.Compiled);
        private Dictionary<ulong, int> _emoteCounts = new();

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = Bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();
            _emoteCounts = db.ServerEmojis.ToDictionary(x => x.EmojiId, x => x.Usage);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = Bot.Services.CreateScope();
                await using (var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>())
                {
                    foreach (var emoteCount in _emoteCounts)
                    {
                        if (await db.ServerEmojis.FirstOrDefaultAsync(x => x.EmojiId == emoteCount.Key) is { } emote)
                        {
                            if (emote.Usage < emoteCount.Value)
                            {
                                emote.Usage = emoteCount.Value;
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                }
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        protected override async ValueTask OnEmojisUpdated(EmojisUpdatedEventArgs e)
        {
			if (e.GuildId != Constants.TF2_GUILD_ID)
				return;
			
            using var scope = Bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

            var oldEmoteList = db.ServerEmojis.Select(x => x.EmojiId).ToList();

            if (oldEmoteList.Count > e.NewEmojis.Count)
            {
                foreach (var oldEmote in oldEmoteList)
                {
                    if (!e.NewEmojis.ContainsKey(oldEmote))
                    {
                        db.Remove(db.ServerEmojis.First(x => x.EmojiId == oldEmote));
                    }
                }
            }

            if (e.NewEmojis.Count > oldEmoteList.Count)
            {
                foreach (var newEmoji in e.NewEmojis)
                {
                    if (!oldEmoteList.Contains(newEmoji.Key))
                    {
                        db.Add(new ServerEmoji
                        {
                            EmojiId = newEmoji.Key
                        });
                    }
                }
            }
            await db.SaveChangesAsync();
        }
       
        protected override ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Message.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content) || e.GuildId != Constants.TF2_GUILD_ID) 
                return ValueTask.CompletedTask;

            var emojis = Bot.GetGuild(Constants.TF2_GUILD_ID).Emojis;
            var alreadyUpdated = new HashSet<Snowflake>();
            
            EmojiRegex.Replace(e.Message.Content, Remove);
            return ValueTask.CompletedTask;
            
            string Remove(Match m)
            {
                _ = Snowflake.TryParse(m.Groups["id"].Value, out var id); // ?<id> is the group

                if (!alreadyUpdated.Add(id))
                {
                    return string.Empty;
                }
                
                if (emojis.ContainsKey(id))
                {
                    _emoteCounts[id] = _emoteCounts.TryGetValue(id, out var count)
                        ? count + 1
                        : 1;
                }
                return string.Empty;
            }
        }
    }
}