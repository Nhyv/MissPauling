using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.AuditLogs;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class PinLoggingService : DiscordBotService
{
    private Dictionary<Snowflake, int> _channelPinData = new();

    protected override async ValueTask OnReady(ReadyEventArgs e)
    {
        var guild = Bot.GetGuild(Constants.TF2_GUILD_ID);
        var channels = guild!.GetChannels().Values.OfType<CachedTextChannel>();

        foreach (var channel in channels)
        {
            try
            {
                var pins = await channel.FetchPinnedMessagesAsync();
                _channelPinData.Add(channel.Id, pins.Count);
            }
            catch
            {
                //
            }
        }

        Logger.LogInformation("Loaded pins.");
    }

    protected override async ValueTask OnChannelPinsUpdated(ChannelPinsUpdatedEventArgs e)
    {
        await Task.Delay(TimeSpan.FromSeconds(2)); // For accuracy's sake

        var pinLogs = await Bot.FetchAuditLogsAsync<IMessageUnpinnedAuditLog>(Constants.TF2_GUILD_ID);

        if (_channelPinData.TryGetValue(e.ChannelId, out int pinCount))
        {
            var afterPin = await e.Channel!.FetchPinnedMessagesAsync();
            if (pinCount < afterPin.Count)
            {
                _channelPinData[e.ChannelId] += 1;
                return;
            }
        }

        _channelPinData[e.ChannelId] -= 1;
        await Bot.SendMessageAsync(Constants.PIN_LOG_CHANNEL_ID,
            new LocalMessage().WithContent(
                $"Message unpinned by {pinLogs[0].Actor!.Tag} (`{pinLogs[0].ActorId}`) in <#{pinLogs[0].ChannelId}>."));
    }
}