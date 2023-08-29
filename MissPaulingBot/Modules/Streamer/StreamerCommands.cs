using System;
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
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Streamer;

[SlashGroup("streamer")]
[RequireAuthorRole(Constants.STREAMER_ROLE_ID)]
public class StreamerCommands : DiscordApplicationModuleBase
{
    [SlashCommand("vcmute")]
    [Description("Allows streamer to mute users in vc.")]
    public async Task<IResult> VcMuteUserAsync([Description("User to mute.")] [RequireNotBot][RequireAuthorRoleHierarchy] IMember member,
        [Description("Provide a LEGITIMATE reason.")] string reason)
    {
        var logEmbed = EmbedUtilities.ErrorBuilder.WithTitle("Streamer Mute")
            .WithDescription($"User **{member.Tag}** (`{member.Id}`) has been muted.").AddField("Reason", reason)
            .WithFooter($"Streamer: {Context.Author.Tag}", Context.Author.GetAvatarUrl()).WithTimestamp(DateTimeOffset.UtcNow);

        await Bot.SendMessageAsync(Constants.NSFW_CHANNEL_ID, new LocalMessage().WithEmbeds(logEmbed));
        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, member.Id, Constants.VOICE_BANNED_ROLE_ID);

        return Response(
            "You have successfully muted this user and your report has been sent to the moderation team! Thank you for your hard work.").AsEphemeral();
    }

    [SlashCommand("announcement")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    [Description("Sends an announcement to streamers.")]
    public async Task<IResult> SendStreamerAnnouncementAsync([Description("Message to send.")] string announcement)
    {
        var streamers = Bot.GetMembers(Constants.TF2_GUILD_ID).Values
            .Where(x => x.RoleIds.Contains(Constants.STREAMER_ROLE_ID));

        var embed = EmbedUtilities.SuccessBuilder
            .WithAuthor(Context.Bot.CurrentUser)
            .WithFooter($"This message will be sent to {streamers.Count()} Community Streamers")
            .WithDescription($"{announcement}");

        var view = new SimplePromptView(x => x.WithEmbeds(embed));
        await View(view);
        var unreachables = "";

        if (view.Result)
        {
            foreach (var streamer in streamers)
            {
                try 
                {
                    await streamer.SendMessageAsync(new LocalMessage().WithContent($"**Trusted Streamer** Announcement:\n{announcement}"));
                }
                catch
                {
                    unreachables += $" {streamer.Id}";
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            return Response($"Sent! I could not reach the following people: {unreachables}");
        }

        return default;
    }
}