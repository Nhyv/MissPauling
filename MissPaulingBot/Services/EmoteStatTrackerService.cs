using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

namespace MissPaulingBot.Services;

public class EmoteStatTrackerService : DiscordBotService
{
    private static readonly Regex EmojiRegex = new(@"<(a|):[\w]+:(?<id>\d{16,20})>", RegexOptions.Compiled);
    private Dictionary<ulong, int> _emoteStats = new();

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", MessageId = "count: 295")]
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = Bot.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();
        _emoteStats = db.ServerEmojis.ToDictionary(x => x.EmojiId, x => x.Usage);
        await base.StartAsync(cancellationToken);
    }

    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands", MessageId = "count: 1388")]
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var statSum = _emoteStats.Sum(x => x.Value);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);

            if (_emoteStats.Sum(x => x.Value) == statSum) 
                continue;
            
            using var scope = Bot.Services.CreateScope();

            await using (var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>())
            {
                var serverEmojis = await db.ServerEmojis.ToListAsync(cancellationToken);
                
                foreach (var emoteStat in _emoteStats)
                {
                    if (serverEmojis.FirstOrDefault(x => x.EmojiId == emoteStat.Key) is not { } emote)
                    {
                        _emoteStats.Remove(emoteStat.Key);
                        continue;
                    }
                    
                    if (emote.Usage < emoteStat.Value)
                    {
                        emote.Usage = emoteStat.Value;
                    }
                }

                statSum = _emoteStats.Sum(x => x.Value);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    protected override async ValueTask OnEmojisUpdated(EmojisUpdatedEventArgs e)
    {
        if (e.GuildId != Constants.TF2_GUILD_ID)
            return;
            
        using var scope = Bot.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        var oldEmoteList = db.ServerEmojis.AsNoTracking().ToList();

        if (oldEmoteList.Count == e.NewEmojis.Count) 
            return;

        if (oldEmoteList.Count > e.NewEmojis.Count)
        {
            foreach (var oldEmote in oldEmoteList.Where(oldEmote => !e.NewEmojis.ContainsKey(oldEmote.EmojiId))) 
            {
                db.ServerEmojis.Remove(oldEmote);
            }
            await db.SaveChangesAsync();
            return;
        }
        foreach (var emoji in e.NewEmojis.Where(x => oldEmoteList.All(y => y.EmojiId != x.Key.RawValue)))
        {
            db.Add(new ServerEmoji
            {
                EmojiId = emoji.Key.RawValue
            });
        }
        await db.SaveChangesAsync();
    }
       
    protected override ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.Message.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content) || e.GuildId != Constants.TF2_GUILD_ID) 
            return ValueTask.CompletedTask;

        var emojis = Bot.GetGuild(Constants.TF2_GUILD_ID)!.Emojis;
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
                _emoteStats[id] = _emoteStats.TryGetValue(id, out var count)
                    ? count + 1
                    : 1;
            }
            return string.Empty;
        }
    }
}