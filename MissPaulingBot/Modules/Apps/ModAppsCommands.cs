using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Menus;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Apps
{
    [SlashGroup("modapps")]
    [Description("The modapps commands.")]
    public class ModAppsCommands : DiscordApplicationModuleBase
    {
        private readonly PaulingDbContext _db;

        public ModAppsCommands(PaulingDbContext db)
        {
            _db = db;
        }

        [SlashCommand("apply")]
        [Description("Apply to become a moderator on the TF2 Community Discord!")]
        public async Task<IResult> ApplyAsync()
        {
            await Deferral(true);
            var guild = Bot.GetGuild(Constants.TF2_GUILD_ID);
            var member = await Bot.GetOrFetchMemberAsync(guild.Id, Context.Author.Id);

            if (member is null)
            {
                return Response("You must be part of TF2 Community to apply for moderation.");
            }

            if (!member.RoleIds.Contains(Constants.EXPERT_ASSASSIN_ROLE_ID) && !member.RoleIds.Contains(Constants.DEATH_MERCHANT_ROLE_ID))
            {
                return Response("You must be at least level 40 to apply for moderation.");
            }

            if (_db.ModApplications.AsNoTracking().FirstOrDefault(x => x.UserId == Context.Author.Id.RawValue) is not { } modapp)
            {
                modapp = new ModApplication
                {
                    UserId = Context.Author.Id,
                    Username = Context.Author.Tag
                };
            }

            if (modapp.HasApplied)
            {
                return Response("You already applied.");
            }
            
            return Menu(new ModAppMenu(modapp, Context.Interaction)
            {
                AuthorId = Context.AuthorId
            }, TimeSpan.FromHours(1));
        }

        [SlashCommand("view")]
        [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
        [Description("View a potential moderator application.")]
        public async Task<IResult> ViewApplication([Description("The applicant.")] IUser applicant)
        {
            if (_db.ModApplications.Find(applicant.Id.RawValue) is not { } modapp)
            {
                return Response("There are no applications for that ID.").AsEphemeral();
            }

            var fields = modapp.ToFields();
            var user = await Context.Bot.GetOrFetchMemberAsync(Constants.TF2_GUILD_ID, applicant.Id.RawValue);

            var embed = EmbedUtilities.SuccessBuilder.WithAuthor(user.Tag, user.GetAvatarUrl());

            foreach (var field in fields)
            {
                if (embed.Length + field.Length > 6000)
                {
                    await Response(embed);
                    embed = EmbedUtilities.SuccessBuilder.WithAuthor(user.Tag, user.GetAvatarUrl());
                }

                embed.AddField(field);
            }

            return Response(embed);
        }

        [SlashCommand("delete")]
        [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
        [Description("Deletes a modapp or all modapps. Normally happens if it was created on accident.")]
        public async Task<IResult> DeleteOneOrManyApplications([Description("The applicant or all if none is supplied.")] IUser applicant = default)
        {
            if (await _db.ModApplications.FindAsync(applicant.Id.RawValue) is not { } modapp)
            {
                return Response("This modapp does not exist. Perhaps it was already deleted?").AsEphemeral();
            }

            _db.Remove(modapp);
            await _db.SaveChangesAsync();
            return Response("Mod app deleted!").AsEphemeral();
        }
    }
}