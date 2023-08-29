using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common.Checks;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation
{
    
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID, Group = "Roles")]
    [Name("Moderation Commands")]
    public class ModerationCommands : DiscordApplicationGuildModuleBase
    {
        private readonly PaulingDbContext _db;

        public ModerationCommands(PaulingDbContext db)
        {
            _db = db;
        }

        private const ulong FRESH_MEAT_ROLE_ID = 539495124103725076;

        private static readonly Snowflake[] LockableChannels =
        {
            132349439506382848, 160784298692182016,
            160784337447550976, 418120895970934795,
            175457862657507330, 480132976110469124, 161039623781351424,
            175457894429360128, 160785859665199104, 160888368123609089,
            357529809959780362, 405834381127581696, 429745217168605207,
            642027509885698058, 162629779021889537, 849750633967910942
        };

        [SlashCommand("kick")]
        [Description("Kicks a user from the server.")]
        public async Task<IResult> KickAsync([RequireBotRoleHierarchy][RequireAuthorRoleHierarchy] [Description("The user you wish to kick.")] IMember member, [Description("The reason you wish to kick them.")] string reason = "No reason provided.")
        {
            if (member.GetRole(Constants.SHARED_BOT_ROLE_ID) != null || member.GetRole(Constants.JUNIOR_MOD_ROLE_ID) != null)
            {
                return Response("Kick failed because target is bot or moderator.");
            }

            await member.KickAsync(new DefaultRestRequestOptions
            {
                Reason = reason
            });
            return Response($"{member.Tag} (`{member.Id}`) has left the server [kicked from server].");
        }
        
        [SlashCommand("emotestats")]
        [RequireChannels(RequireChannels.ChannelMode.Mod)]
        [Description("Get the emote stats")]
        public async Task<IResult> GetStatsAsync()
        {
            var emoteStats = await _db.ServerEmojis.ToListAsync();
            var guild = Context.Bot.GetGuild(Context.GuildId);
            var allEmojis = guild.Emojis;
            var pages = emoteStats.OrderByDescending(x => x.Usage).SplitBy(25).Select(x =>
                new Page().WithEmbeds(EmbedUtilities.SuccessBuilder.WithTitle("Emoji Usage Statistics")
                    .WithDescription(string.Join("\n",
                        x.Select(y => $"{allEmojis[y.EmojiId].Tag} - {y.Usage} time(s)")))));

            return Pages(pages);
        }

        [SlashCommand("lock")]
        [Description("Block Mercenaries from a channel.")]
        public async Task<IResult> LockAsync([ChannelTypes(ChannelType.Text)][Description("The channel you want to lock.")] IChannel channel = null)
        {
            var result = await LockChannelAsync(channel?.Id ?? Context.ChannelId);

            return Response(result);
        }

        [SlashCommand("lockdown")]
        [Description("Block Mercenaries from all channels.")]
        public async Task<IResult> LockDownAsync()
        {
            var builder = new StringBuilder();

            await Response("Locking down now...");

            foreach (var channelId in LockableChannels)
            {
                var result = await LockChannelAsync(channelId);

                builder.AppendNewLine($"{Mention.Channel(channelId)}: {result}");
            }

            return Response(builder.AppendNewLine("All done").ToString());
        }

        private async Task<string> LockChannelAsync(Snowflake channelId)
        {
            if (!LockableChannels.Contains(channelId))
                return "This channel is not lockable.";

            var channel = Bot.GetChannel(Constants.TF2_GUILD_ID, channelId);

            if (channel.Overwrites.FirstOrDefault(x => x.TargetId == FRESH_MEAT_ROLE_ID) is { } overwrite)
            {
                if (overwrite.Permissions.Denied.HasFlag(Permissions.SendMessages) && overwrite.Permissions.Denied.HasFlag(Permissions.AddReactions))
                {
                    await channel.SetOverwriteAsync(new LocalOverwrite(FRESH_MEAT_ROLE_ID, OverwriteTargetType.Role,
                        new OverwritePermissions().Unset(Permissions.SendMessages | Permissions.AddReactions)));

                    return "🔓";
                }
            }

            await channel.SetOverwriteAsync(new LocalOverwrite(FRESH_MEAT_ROLE_ID,
                OverwriteTargetType.Role, new OverwritePermissions(Permissions.None, Permissions.SendMessages | Permissions.AddReactions)));
            return "🔒";
        }
    }
}