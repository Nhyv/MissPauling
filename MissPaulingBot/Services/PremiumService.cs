using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class PremiumService : DiscordBotService
{
    protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    {
        if (!e.NewMember.RoleIds.Contains(Constants.PREMIUM_MEMBER_ROLE_ID) || e.OldMember is null)
            return;

        if (!e.OldMember.RoleIds.Contains(Constants.PREMIUM_MEMBER_ROLE_ID) && e.NewMember.RoleIds.Contains(Constants.PREMIUM_MEMBER_ROLE_ID))
        {
            var message = $"<@{e.MemberId}> has subscribed to TF2 Community Premium! Welcome!";

            if (e.NewMember.RoleIds.Contains(Constants.SAXTON_OWN_ROLE_ID))
            {
                message +=
                    " Since you have selected the $10 subscription 'Saxton's Own', you may receive a role with a custom name, color, and icon." +
                    $" Please make sure to choose an appropriate name, a color that is not already in use by our moderators and an appropriate static icon under 256KB. Once that is done, use {Mention.SlashCommand(1069899747801960548, "premium role")}!";
            }

            await Bot.SendMessageAsync(Constants.PREMIUM_CHANNEL_ID, new LocalMessage().WithContent(message));
        }
    }
}