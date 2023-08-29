using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;

namespace MissPaulingBot.Services;

public class StickyService : DiscordBotService
{
    protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
    {
        using var scope = Bot.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();
        var stickyRoles = await db.StickyRoles.Include(x => x.StickyUsers).ToListAsync();

        foreach (var stickyRole in stickyRoles)
        {
            if (stickyRole.StickyUsers.Any(x => x.UserId == e.MemberId.RawValue))
            {
                await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, e.MemberId.RawValue, stickyRole.RoleId);
                Logger.LogInformation($"{e.MemberId} had sticky role {stickyRole.RoleName} (`{stickyRole.RoleId}`) so I added it back to them.");
            }
        }
    }

    protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    {
        if (e.OldMember is null) // If the old member is null, then there's no way to check.
            return;

        // Getting the database
        using var scope = Bot.Services.CreateScope(); 
        var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        // Getting the stickyroles to look through them.
        var stickyRoles = await db.StickyRoles.Include(x => x.StickyUsers).ToListAsync();

        // If the member lost roles during that update.
        if (e.OldMember.RoleIds.Count >= e.NewMember.RoleIds.Count)
        {
            foreach (var stickyRole in stickyRoles) 
            {
                // If the sticky role was removed from them.
                if (e.OldMember.RoleIds.Contains(stickyRole.RoleId) && !e.NewMember.RoleIds.Contains(stickyRole.RoleId))
                {
                    var stickyUser = await db.StickyUsers.FindAsync(e.NewMember.Id.RawValue);
                    stickyRole.StickyUsers.Remove(stickyUser);
                    // Remove the sticky user from the list.
                    Logger.LogInformation($"User {e.NewMember.Id} was removed from the {stickyRole.RoleName} sticky list.");
                }
            }

            await db.SaveChangesAsync();
        }
        else
        {
            foreach (var stickyRole in stickyRoles)
            {
                // If they were added to it.
                if (!e.OldMember!.RoleIds.Contains(stickyRole.RoleId) && e.NewMember.RoleIds.Contains(stickyRole.RoleId))
                {
                    // If they are already a sticky user
                    if (await db.StickyUsers.Include(x => x.StickyRoles).FirstOrDefaultAsync(x => x.UserId == e.NewMember.Id.RawValue) is { } existingStickyUser)
                    {
                        // Just add them to the stickyrole user list if they aren't part of it already.
                        if (!existingStickyUser.StickyRoles.Contains(stickyRole))
                        {
                            existingStickyUser.StickyRoles.Add(stickyRole);
                            Logger.LogInformation($"{e.NewMember.Id} was added to the {stickyRole.RoleName} Sticky Role list (`{stickyRole.RoleId}`");
                        }
                    }
                    else
                    {
                        // Otherwise, create the stickyuser and add it to their list.
                        db.StickyUsers.Add(new StickyUser
                        {
                            UserId = e.NewMember.Id.RawValue,
                            StickyRoles = new List<StickyRole> { stickyRole }

                        });
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}