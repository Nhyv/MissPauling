using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Rest;

namespace MissPaulingBot.Extensions;

public static class DiscordExtensions
{
    public static DiscordResponseCommandResult AsEphemeral(this DiscordResponseCommandResult result)
    {
        (result.Message as LocalInteractionMessageResponse)!.IsEphemeral = true;
        return result;
    }

    public static bool HasUpdatedRoles(this MemberUpdatedEventArgs e, out List<Snowflake> removedRoleIds, out List<Snowflake> addedRoleIds)
    {
        if (e.OldMember is null)
        {
            removedRoleIds = new List<Snowflake>();
            addedRoleIds = new List<Snowflake>();
            return false;
        }

        addedRoleIds = e.NewMember.RoleIds.Except(e.OldMember.RoleIds).ToList();
        removedRoleIds = e.OldMember.RoleIds.Except(e.NewMember.RoleIds).ToList();

        return addedRoleIds.Count > 0 || removedRoleIds.Count > 0;
    }

    public static async ValueTask<IUser?> GetOrFetchUserAsync(this DiscordClientBase client, Snowflake id)
    {
        return client.GetUser(id) ?? (IUser) (await client.FetchUserAsync(id))!;
    }

    public static async ValueTask<IMember?> GetOrFetchMemberAsync(this DiscordClientBase client, Snowflake guildId, Snowflake memberId)
    {
        if (client.GetMember(guildId, memberId) is { } cachedMember)
            return cachedMember;
    
        if (client.ApiClient.GetShard(guildId)!.RateLimiter.GetRemainingRequests() < 3)
        {
            return await client.FetchMemberAsync(guildId, memberId);
        }
    
        var members = await client.Chunker.QueryAsync(guildId, new[] {memberId});
        return members.GetValueOrDefault(memberId);
    }
        
    public static string? Format(this IUser? user, bool bold = true)
        => user is null
            ? null
            : $"{(bold ? Markdown.Bold(user.Tag) : user.Tag)} (`{user.Id}`)";
        
    public static bool HasImageExtension(this string str)
    {
        str = str.ToLowerInvariant();
        return str.Split(".")[^1].EqualsAny("bmp", "gif", "jpeg", "jpg", "png");
    }

    public static async Task ClearComponentsAndStopAsync(this MenuBase menu)
    {
        menu.View?.ClearComponents();
        await menu.ApplyChangesAsync();
        menu.Stop();
    }
}