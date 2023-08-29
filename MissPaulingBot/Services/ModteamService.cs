using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class ModteamService : DiscordBotService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Bot.WaitUntilReadyAsync(stoppingToken);
        var guild = Bot.GetGuild(Constants.TF2_GUILD_ID);
        var chunk = await Bot.Chunker.ChunkAsync(guild, stoppingToken);

        if (chunk)
        {
            Console.WriteLine("Chunk completed with success");
        }
        else
        {
            Console.WriteLine("Failed to chunk?");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await Bot.FetchMessagesAsync(Constants.RULES_N_INFO_CHANNEL_ID, 1, cancellationToken: stoppingToken);
            var message = messages[0] as IUserMessage;
            var members = Bot.GetMembers(Constants.TF2_GUILD_ID);
            var coOwners = members.Where(x => x.Value.RoleIds.Contains(Constants.CO_OWNER_ROLE_ID));
            var communityMods = members.Where(x => x.Value.RoleIds.Contains(Constants.COMMUNITY_MOD_ROLE_ID));
            var juniorMods = members.Where(x => x.Value.RoleIds.Contains(Constants.JUNIOR_MOD_ROLE_ID));
            var mentors = members.Where(x => x.Value.RoleIds.Contains((Snowflake)945810875883855952));

            var builder = new StringBuilder("**TF2 Community Moderation Team**\n\n");
            builder.Append("**Co-Owners**\n").AppendJoin(" ", coOwners.Select(x => x.Value.Mention));
            builder.Append("\n\n**Community Mods**\n");

            var counter = 1;

            foreach (var communityMod in communityMods)
            {
                if (communityMod.Value.Id == 227578898521653249 || communityMod.Value.Id == 167452465317281793)
                    continue;

                builder.Append(communityMod.Value.Mention).Append(" ");

                if (counter % 4 == 0 && counter != 0)
                {
                    builder.Append("\n");
                }

                counter++;
            }

            counter = 1;

            builder.Append("\n\n**Chief Mini Mods**\n");

            foreach (var juniorMod in juniorMods)
            {
                builder.Append(juniorMod.Value.Mention).Append(" ");

                if (counter % 4 == 0 && counter != 0)
                {
                    builder.Append("\n");
                }

                counter++;
            }

            builder.Append("\n\n**Mentors**\n");

            foreach (var mentor in mentors)
            {
                builder.Append(mentor.Value.Mention).Append(" ");

                if (counter % 4 == 0 && counter != 0)
                {
                    builder.Append("\n");
                }

                counter++;
            }

            if (!message.Content.StartsWith("**TF2 Community Moderation Team**") || !message.Author.IsBot)
            {
                await Bot.SendMessageAsync(Constants.RULES_N_INFO_CHANNEL_ID,
                    new LocalMessage().WithContent(builder.ToString()).WithAllowedMentions(LocalAllowedMentions.None), cancellationToken: stoppingToken);
            }
            else
            {
                await message.ModifyAsync(x =>
                    {
                        x.AllowedMentions = LocalAllowedMentions.None;
                        x.Content = builder.ToString();
                    },
                    cancellationToken: stoppingToken);
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

    }
}