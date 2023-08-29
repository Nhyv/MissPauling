using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services
{
    public sealed class StickerService : DiscordClientService
    {
        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (e.Member is null) return;

            if (e.GuildId != Constants.TF2_GUILD_ID)
                return;
            
            if (e.Message is CachedUserMessage message)
            {
                if (message.Stickers.Count > 0 && message.ChannelId != Constants.SHITPOSTS_CHANNEL_ID &&
                    message.ChannelId != Constants.BOTCHAT_CHANNEL_ID && message.ChannelId != Constants.LOUNGE_CHANNEL_ID && message.ChannelId != Constants.PREMIUM_CHANNEL_ID && message.ChannelId != Constants.BOOSTER_CHANNEL_ID)
                {
                    await e.Message.DeleteAsync();
                } 
            }
        }
    }
}