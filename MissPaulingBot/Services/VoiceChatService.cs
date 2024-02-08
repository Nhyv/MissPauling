using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Utilities;

namespace MissPaulingBot.Services;

public sealed class VoiceChatService : DiscordBotService
{
    private const ulong PAULING_LOG_CHANNEL_ID = 571337916278898699;

    protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
    {
        if (e.OldVoiceState?.ChannelId is not null &&
            e.NewVoiceState?.ChannelId is null)
        {
            await Bot.SendMessageAsync(PAULING_LOG_CHANNEL_ID,
                new LocalMessage().WithEmbeds(EmbedUtilities.LoggingBuilder
                    .WithTitle("Voicechat Update: Member Left")
                    .AddField("Username", e.Member.Name)
                    .AddField("ID", e.Member.Id.RawValue)
                    .AddField("Channel:", $"<#{e.OldVoiceState?.ChannelId}>")
                    .WithThumbnailUrl(e.Member.GetAvatarUrl())));

            return;
        }

        if (e.OldVoiceState?.ChannelId is null && e.NewVoiceState?.ChannelId is not null)
        {
            await Bot.SendMessageAsync(PAULING_LOG_CHANNEL_ID,
                new LocalMessage().WithEmbeds(EmbedUtilities.LoggingBuilder
                    .WithTitle("Voicechat Update: Member Joined")
                    .AddField("Username", e.Member.Name)
                    .AddField("ID", e.Member.Id.RawValue)
                    .AddField("Channel:", $"<#{e.NewVoiceState?.ChannelId}>")
                    .WithThumbnailUrl(e.Member.GetAvatarUrl())));
        }
    }
}