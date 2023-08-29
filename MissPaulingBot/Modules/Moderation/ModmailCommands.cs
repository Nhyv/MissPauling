using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Menus.Views;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation
{
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public class ModmailCommands : DiscordApplicationModuleBase
    {
        private readonly PaulingDbContext _db;

        public ModmailCommands(PaulingDbContext db)
        {
            _db = db;
        }

        [SlashCommand("dm")]
        [Description("Reply to a modmail or contact a user.")]
        public async Task<IResult> DmAsync([Description("The user you wish to contact.")] IUser user, [Description("The template.")] DmMessageTemplate template = null,
            [Description("The modmail message.")] string message = "")
        {
            if (template is null && string.IsNullOrWhiteSpace(message))
                return Response("You did not provide a value to send.");

            if (user is null)
                return Response("This user does not exist.");

            var embed = EmbedUtilities.SuccessBuilder
                .WithAuthor(Context.Bot.CurrentUser)
                .WithFooter($"This message will be sent to {user}", user.GetAvatarUrl())
                .WithDescription($"{template?.Response} {message}");
            var view = new SimplePromptView(x => x.WithEmbeds(embed));
            await View(view);
            if (view.Result)
            {
                try
                {
                    await user.SendMessageAsync(new LocalMessage().WithContent($"{template?.Response} {message}"));

                    return Response("You have sent the following message successfully:", embed.WithFooter("Post-modmail feedback"));
                }
                catch
                {
                    return Response(
                        "This user either blocked me, has their DMs set to Friends Only, or no longer shares a server with me.");
                }
            }

            return default;
        }

        [SlashCommand("addtemplate")]
        [Description("Adds a template for modmail.")]
        public async Task<IResult> AddTemplateAsync()
        {
            var modal = new LocalInteractionModalResponse().WithTitle("Add a Template")
                .WithCustomId("ModmailTemplate:Add").WithComponents(new LocalRowComponent().WithComponents(
                        new LocalTextInputComponent()
                            .WithLabel("Enter a name")
                            .WithStyle(TextInputComponentStyle.Short)
                            .WithIsRequired()
                            .WithCustomId("ModmailTemplate:Name")
                            .WithPlaceholder("This will be used when searching...")),
                    new LocalRowComponent().WithComponents(new LocalTextInputComponent()
                        .WithLabel("Enter the template message")
                        .WithStyle(TextInputComponentStyle.Paragraph)
                        .WithIsRequired()
                        .WithCustomId("ModmailTemplate:Message")
                        .WithMaximumInputLength(1500)
                        .WithPlaceholder("This will be sent to the user...")));

            await Context.Interaction.Response().SendModalAsync(modal);

            return default;
        }

        [SlashCommand("removetemplate")]
        [Description("Removes a template for modmail.")]
        public async Task<IResult> RemoveTemplateAsync([Description("The template to remove.")] DmMessageTemplate template)
        {
            var embed = EmbedUtilities.SuccessBuilder.WithTitle($"Do you want to delete template {template.Name}?")
                .WithDescription(template.Response.TrimTo(Discord.Limits.Message.Embed.MaxDescriptionLength));

            var view = new SimplePromptView(x => x.WithEmbeds(embed));

            await View(view);

            if (view.Result)
            {
                _db.DmMessageTemplates.Remove(template);
                await _db.SaveChangesAsync();
                return Response("I have appropriately nuked the template you selected.");
            }

            return Response("Operation cancelled. Thanks for using Airbnb.");
        }

        [SlashCommand("verbal")]
        [Description("Sends a verbal warning to the user.")]
        public async Task<IResult> VerbalAsync([Description("The user you wish to contact.")] IUser user, [Description("The template.")] DmMessageTemplate template = null,
            [Description("The modmail message.")] string message = "")
        {
            if (user is null)
                return Response("This user does not exist.");

            var embed = EmbedUtilities.SuccessBuilder
                .WithAuthor(Context.Bot.CurrentUser)
                .WithFooter($"This message will be sent to {user}", user.GetAvatarUrl())
                .WithDescription($"Hello, this is a verbal warning from the TF2 Community. {template?.Response} {message}");
            var view = new SimplePromptView(x => x.WithEmbeds(embed));
            await View(view);

            if (view.Result)
            {
                _db.Add(new UserNote()
                {
                    UserId = user.Id.RawValue,
                    ModeratorId = Context.Author.Id,
                    Note = $"{(template is null ? message : template + " " + message)}",
                    Username = user.Tag
                });

                await _db.SaveChangesAsync();
                await Context.Bot.SendMessageAsync(Context.ChannelId,
                    new LocalMessage().WithContent("Note created."));

                try
                {
                    await user.SendMessageAsync(
                        new LocalMessage().WithContent(
                            $"Hello, this is a verbal warning from the TF2 Community. {template?.Response} {message}"));
                }
                catch
                {

                    return Response(
                        "This user either blocked me, has their DMs set to Friends Only, or no longer shares a server with me.");
                }

                return Response("You have sent the following message successfully:", embed.WithFooter("Post-modmail feedback"));
            }

            return default;
        }

        [AutoComplete("dm")]
        [AutoComplete("verbal")]
        [AutoComplete("removetemplate")]
        public async Task AutoCompleteDmMessageTemplateAsync(AutoComplete<string> template)
        {
            var messageTemplates = await _db.DmMessageTemplates.ToListAsync();

            var text = template.RawArgument;

            if (string.IsNullOrWhiteSpace(text))
            {
                template.Choices.AddRange(messageTemplates.Take(25)
                    .ToDictionary(
                        x => x.ToString().TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength),
                        x => x.Name));

                return;
            }

            if (messageTemplates.FirstOrDefault(x => x.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)) is { } existingTemplate)
            {
                template.Choices.Add(existingTemplate.ToString().TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength), existingTemplate.Name);
                return;
            }

            var matchingTemplates =
                messageTemplates.Where(x => x.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (matchingTemplates.Count > 0)
            {
                template.Choices.AddRange(matchingTemplates.Take(25)
                    .ToDictionary(
                        x => x.ToString().TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength),
                        x => x.Name));

                return;
            }

            template.Choices.AddRange(messageTemplates.Take(25)
                .ToDictionary(
                    x => x.ToString().TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength),
                    x => x.Name));
        }
    }

    public class ModmailTemplateModalCommands : DiscordComponentGuildModuleBase
    {
        private readonly PaulingDbContext _db;

        public ModmailTemplateModalCommands(PaulingDbContext db)
        {
            _db = db;
        }

        [ModalCommand("ModmailTemplate:Add")]
        public async Task<IResult> AddModmailTemplateAsync()
        {
            var modal = (IModalSubmitInteraction)Context.Interaction;

            var nameRow = (IRowComponent)modal.Components[0];
            var messageRow = (IRowComponent)modal.Components[1];
            var name = ((ITextInputComponent)nameRow.Components[0]).Value;

            if (_db.DmMessageTemplates.Find(name) is { } template)
            {
                await modal.Response()
                    .SendMessageAsync(
                        new LocalInteractionMessageResponse().WithContent("A template with the same name was found."));

                return default;
            }

            _db.DmMessageTemplates.Add(new DmMessageTemplate
            {
                AuthorId = Context.Author.Id,
                Name = name,
                Response = ((ITextInputComponent)messageRow.Components[0]).Value
            });

            await _db.SaveChangesAsync();

            await modal.Response()
                .SendMessageAsync(
                    new LocalInteractionMessageResponse().WithContent($"Template {name} added."));

            return default;
        }
    }
} 