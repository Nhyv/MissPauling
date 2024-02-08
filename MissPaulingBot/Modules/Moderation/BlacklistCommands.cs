using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation;

[SlashGroup("blacklist")]
[RequireAuthorRole(Constants.MODERATOR_ROLE_ID, Group = "Roles")]
[Description("Blacklist commands for the Modmail.")]
public class BlacklistCommands : DiscordApplicationGuildModuleBase
{
    private readonly PaulingDbContext _db;

    public BlacklistCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("add")]
    [Description("Blacklists a user.")]
    public async Task<IResult> DmBlacklistAsync([Description("The user to blacklist.")] IUser user)
    {
        if (user is null) 
            return Response("User not found").AsEphemeral();

        if (_db.BlacklistedUsers.Find(user.Id.RawValue) is { } blacklistedUser)
            return Response($"{user.Tag} is already blacklisted.").AsEphemeral();

        _db.Add(new BlacklistedUser
        {
            UserId = user.Id.RawValue,
            ModeratorId = Context.Author.Id,
            Username = user.Tag,
        });

        await _db.SaveChangesAsync();
        return Response($"{user.Tag} was blacklisted.");
    }

    [SlashCommand("remove")]
    [Description("Removes a user from the blacklist.")]
    public async Task<IResult> RemoveDmBlacklistedUser([Description("The user to remove from the blacklist.")] IUser user)
    {
        if (user is null)
            return Response("This user does not exist.").AsEphemeral();

        if (_db.BlacklistedUsers.FirstOrDefault(x =>
                x.UserId == user.Id.RawValue) is not { } blacklistedUser)
        {
            return Response($"{user.Tag} is not blacklisted.").AsEphemeral();
        }

        _db.Remove(blacklistedUser);
        await _db.SaveChangesAsync();
        return Response($"{user.Tag} was removed from the Modmail blacklist.");
    }

    [SlashCommand("show")]
    [Description("Shows every user in the blacklist.")]
    public async Task<IResult> GetBlacklist()
    {
        var blacklist = await _db.BlacklistedUsers.ToListAsync();
        var split = blacklist.SplitBy(10);
        var pages = new List<Page>();

        foreach (var group in split)
        {
            var builder = EmbedUtilities.SuccessBuilder.WithTitle("Blacklist");
            foreach (var blacklistedUser in group)
            {
                builder.AddField(blacklistedUser.UserId.ToString(), blacklistedUser.Username);
            }
            pages.Add(new Page().WithEmbeds(builder));
        }


        return Pages(pages);
    }

    [SlashCommand("search")]
    [Description("Is this user in the blacklist?")]
    public async Task<IResult> SearchForUser([Description("The user you are verifying.")] IUser user)
    {
        if (await _db.BlacklistedUsers.FindAsync(user.Id.RawValue) is { } blacklistedUser)
            return Response($"{user.Tag} is blacklisted.");

        return Response($"{user.Tag} is not blacklisted");
    }
}