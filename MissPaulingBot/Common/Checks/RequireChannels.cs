using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;

namespace MissPaulingBot.Common.Checks
{
    public class RequireChannels : DiscordCheckAttribute
    {
        private static readonly Snowflake[] ModChannels =
        {
            832373987077259325,
            570424279762468874,
            732441662931992638,
            420441288660484096,
            565360990959697940,
            930864646632640542
        };

        private readonly ChannelMode CurrentMode;

        public enum ChannelMode
        {
            Mod,
            Approved,
            Utility
        }

        public RequireChannels(ChannelMode mode)
        {
            CurrentMode = mode;
        }

        public override ValueTask<IResult> CheckAsync(IDiscordCommandContext context)
        {
            if (ModChannels.Contains(context.ChannelId)) return Results.Success;

            switch (CurrentMode)
            {
                case ChannelMode.Approved:
                    if (context.ChannelId == Constants.BOTCHAT_CHANNEL_ID) return Results.Success;

                    return Results.Failure("This command can only be used in botchat or in the modchats.");
                case ChannelMode.Utility:
                    if (context.ChannelId == Constants.SERVER_META_CHANNEL_ID ||
                        context.ChannelId == Constants.BOTCHAT_CHANNEL_ID) return Results.Success;

                    return Results.Failure("This command can only be used in botchat, server-meta, or in the modchats.");
            }

            return Results.Failure("This command can only be used in modchat.");
        }
    }
}