using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services
{
    public sealed class BoosterService : DiscordBotService
    {
        private static readonly Snowflake[] RoleIds =
        {
            673660723477610506, 673661192434089991, 673661256929902647,
            673661306481410089, 673661397447606300, 673661441877868567,
            673661574426263592, 673661664603799603, 673661819415691285,
            673662043219427358, 673662119140524042, 673662183942520868,
            673662258554994730, 673662344685027368, 673662398586159121,
            673662456907825193, 673662520476827669, 673662607474819084,
            673662652781953030, 673662715855896576, 673662878397759488,
            673662915097657366
        };

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            if (e.OldMember?.RoleIds.Contains(Constants.BOOSTER_ROLE_ID) != true || 
                e.NewMember.RoleIds.Contains(Constants.BOOSTER_ROLE_ID)) 
                return;

            foreach (var roleId in RoleIds)
            {
                if (!e.NewMember.RoleIds.Contains(roleId)) continue;

                await e.NewMember.RevokeRoleAsync(roleId);
                Logger.LogInformation($"User {e.NewMember.Id} no longer has the Proof of Purchase role. I removed their colored role.");
            }
        }

        
    }
}