using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;

namespace MissPaulingBot.Services;

public class PremiumService : DiscordBotService
{
    protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    {
        if (!e.HasUpdatedRoles(out var removedRoleIds, out var addedRoleIds))
            return;

        var scope = Bot.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        // If they had saxton's own but now they don't no more
        if (removedRoleIds.Contains(Constants.SAXTON_OWN_ROLE_ID))
        {
            if (db.SaxtonOwnRoles.FirstOrDefault(x => x.OwnerId == e.MemberId.RawValue) is { } saxtonOwnRole)
            {
                await Bot.DeleteRoleAsync(e.GuildId, saxtonOwnRole.Id);
                db.SaxtonOwnRoles.Remove(saxtonOwnRole);
                await db.SaveChangesAsync();
                return;
            }
        }
        
        // If they are a new premium member.
        if (addedRoleIds.Contains(Constants.PREMIUM_MEMBER_ROLE_ID))
        {
            var message = $"{Mention.User(e.NewMember)} has subscribed to TF2 Community Premium! Welcome!";

            if (addedRoleIds.Contains(Constants.SAXTON_OWN_ROLE_ID))
            {
                message +=
                    " Since you have selected the $10 subscription 'Saxton's Own', you may receive a role with a custom name, color, and icon." +
                    $" Please make sure to choose an appropriate name, a color that is not already in use by our moderators and an appropriate static icon under 256KB. Once that is done, use {Mention.SlashCommand(1069899747801960548, "premium role")}!";
            }

            await Bot.SendMessageAsync(Constants.PREMIUM_CHANNEL_ID, new LocalMessage().WithContent(message));
        }
    }
}