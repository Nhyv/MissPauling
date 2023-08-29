using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class SaxtonsOwnService : DiscordBotService
{
    protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    {
        using var scope = Bot.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        // This doesn't apply if : They aren't Saxton's Own, they still are
        if (e.OldMember is null || !e.OldMember.RoleIds.Contains(Constants.SAXTON_OWN_ROLE_ID) || e.NewMember.RoleIds.Contains(Constants.SAXTON_OWN_ROLE_ID))
            return;

        if (await db.SaxtonOwnRoles.FindAsync(e.MemberId.RawValue) is not { } saxtonOwn)
            return;

        db.Remove(saxtonOwn);
        await db.SaveChangesAsync();
    }
}