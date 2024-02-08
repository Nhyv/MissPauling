using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Menus.Views;
using MissPaulingBot.Extensions;
using Qmmands;

namespace MissPaulingBot.Modules.Boosters;

[RequireAuthorRole(Constants.BOOSTER_ROLE_ID)]
[Name("Booster Commands")]
[Description("Commands for users with the Proof of Purchase role (Nitro Boosters)")]
public class BoosterCommands : DiscordApplicationModuleBase
{
    private static readonly Snowflake[] BoosterRoleIds =
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

    [SlashCommand("paint")]
    [Description("Gives you a booster role.")]
    public IResult SetBoosterRole()
        => View(new PaintView());

    [SlashCommand("wash")]
    [Description("Removes your booster role if you don't want one anymore.")]
    public async Task<IResult> RemoveBoosterRoleAsync()
    {
        if ((Context.Author as IMember)?.GetRoles().Values.FirstOrDefault(x => BoosterRoleIds.Contains(x.Id)) is
            { } boosterRole)
        {
            await Context.Bot.RevokeRoleAsync(Constants.TF2_GUILD_ID, Context.Author.Id, boosterRole.Id);
            return Response($"{Context.Author.Mention}, you no longer have the role **{boosterRole.Name}**.").AsEphemeral();
        }

        return Response("You do not have a booster paint.").AsEphemeral();
    }
}