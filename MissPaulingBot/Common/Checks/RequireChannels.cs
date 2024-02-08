using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;

namespace MissPaulingBot.Common.Checks;

public class RequireChannels(RequireChannels.ChannelMode mode) : DiscordCheckAttribute
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

    public enum ChannelMode
    {
        Mod,
        Approved,
        Utility
    }

    public override ValueTask<IResult> CheckAsync(IDiscordCommandContext context)
    {
        if (ModChannels.Contains(context.ChannelId)) return Results.Success;

        return mode switch
        {
            ChannelMode.Approved when context.ChannelId == Constants.BOTCHAT_CHANNEL_ID => Results.Success,
            ChannelMode.Approved => Results.Failure("This command can only be used in botchat or in the modchats."),
            ChannelMode.Utility when context.ChannelId == Constants.SERVER_META_CHANNEL_ID ||
                                     context.ChannelId == Constants.BOTCHAT_CHANNEL_ID => Results.Success,
            ChannelMode.Utility => Results.Failure(
                "This command can only be used in botchat, server-meta, or in the modchats."),
            _ => Results.Failure("This command can only be used in modchat.")
        };
    }
}