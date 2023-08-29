using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class FreshMeatService : DiscordBotService
{
    protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    {
        if (e.OldMember is null)
            return;

        if (e.OldMember.RoleIds.Count > e.NewMember.RoleIds.Count)
            return;

        if (!e.NewMember.RoleIds.Contains(new Snowflake(1110635371982749796)))
            return;

        await Bot.RevokeRoleAsync(Constants.TF2_GUILD_ID, e.MemberId, Constants.DEATH_MERCHANT_ROLE_ID);
    }
}