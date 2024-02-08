using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation;

[RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
[SlashGroup("stickyrole")]
[Description("Commands for managing sticky roles.")]
public class StickyCommands : DiscordApplicationGuildModuleBase
{
    private readonly PaulingDbContext _db;

    public StickyCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("add")]
    [Description("[CHUNK] Adds a sticky role.")]
    public async Task<IResult> AddStickyRoleAsync(IRole role)
    {
        if (await _db.StickyRoles.FindAsync(role.Id.RawValue) is not null)
            return Response("This role is already a sticky role. Did you mean to remove it?").AsEphemeral();

        var newStickyRole = _db.StickyRoles.Add(new StickyRole
        {
            RoleId = role.Id.RawValue,
            RoleName = role.Name,
            StickyUsers = new List<StickyUser>()
        }).Entity;

        await _db.SaveChangesAsync();

        var guild = Bot.GetGuild(Constants.TF2_GUILD_ID);
        var members = guild!.Members.Where(x => x.Value.RoleIds.Contains(role.Id)).ToList();
        var stickyUsers = await _db.StickyUsers.Include(x => x.StickyRoles).ToListAsync();

        foreach (var member in members)
        {
            if (stickyUsers.FirstOrDefault(x => x.UserId == member.Key.RawValue) is { } stickyUser)
            {
                stickyUser.StickyRoles.Add(newStickyRole);
                continue;
            }

            _db.StickyUsers.Add(new StickyUser
            {
                UserId = member.Key.RawValue,
                StickyRoles = new List<StickyRole> { newStickyRole }
            });
        }

        await _db.SaveChangesAsync();
        return Response($"**{role.Name}** is now a sticky role and **{members.Count()}** members were added to it..");
    }

    [SlashCommand("remove")]
    [Description("Removes a sticky role. Dangerous!")]
    public async Task<IResult> RemoveStickyRoleAsync(IRole role)
    {
        if (!(_db.StickyRoles.FirstOrDefault(x => x.RoleId == role.Id.RawValue) is { } stickyRole))
            return Response($"**{role.Name}** is not a sticky role. Did you mean to add it?").AsEphemeral();

        _db.StickyRoles.Remove(stickyRole);
        await _db.SaveChangesAsync();

        return Response($"**{role.Name}** is no longer a sticky role.");
    }

    [SlashCommand("list")]
    [Description("Gives a list of sticky roles.")]
    public async Task<IResult> ListStickyRolesAsync()
    {
        var stickyRoles = await _db.StickyRoles.Include(x => x.StickyUsers).ToListAsync();
        var embed = EmbedUtilities.SuccessBuilder.WithTitle("Sticky Roles")
            .WithDescription("A sticky role is a role that remains on the user even if they leave and rejoin.")
            .WithFooter(
                "Fetching all of the members can take a long time. If the numbers seem wrong, wait a minute or two and use this again.");

        foreach (var stickyRole in stickyRoles)
        {
            embed.AddField(stickyRole.RoleName, $"{stickyRole.StickyUsers.Count} members currently have this role.");
        }

        return Response(embeds: embed);
    }
}