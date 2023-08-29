using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Checks;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Suggestions;

[Description("Suggestion commands for #server-meta")]
[SlashGroup("suggestion")]
public class SuggestionCommands : DiscordApplicationGuildModuleBase
{
    private readonly PaulingDbContext _db;
    private readonly HttpClient _http;

    public SuggestionCommands(PaulingDbContext db, HttpClient http)
    {
        _db = db;
        _http = http;
    }
    [SlashCommand("create")]
    [Description("Create a suggestion for the Discord.")]
    [RequireChannel(Constants.SERVER_META_CHANNEL_ID)]
    public async Task<IResult> CreateSuggestion([Description("Your suggestion message")] string content,
        [Description("Image for the suggestion")] [SupportedFileExtensions("png", "jpg", "jpeg", "gif", "gifv", "webm")]
        IAttachment attachment = null)
    {
        var bulliedRole = Bot.GetRole(Constants.TF2_GUILD_ID, Constants.CONTRACT_KILLER_ROLE_ID);

        if (Context.Author.RoleIds.Contains(Constants.SUGGESTION_BANNED_ROLE_ID) || Context.Author.RoleIds.Contains(Constants.CONTRACT_KILLER_ROLE_ID))
            return Response($"You cannot create suggestions if you are suggestion banned or have the role **{bulliedRole.Name}**.").AsEphemeral();

        Suggestion suggestion = null;

        var embed = EmbedUtilities.SuccessBuilder.WithAuthor(Context.Author).WithDescription(content);
        var message = new LocalMessage();

        if (attachment != null)
        {
            var data = new MemoryStream();
            await using var stream = await _http.GetStreamAsync(attachment.Url);
            await stream.CopyToAsync(data);
            data.Seek(0, SeekOrigin.Begin);

            suggestion = _db.Suggestions.Add(new Suggestion()
            {
                AuthorId = Context.AuthorId.RawValue,
                Content = content,
                Data = data,
                Extension = Path.GetExtension(new Uri(attachment.Url).AbsolutePath)[1..].ToLower()
            }).Entity;

            await _db.SaveChangesAsync();
            data.Seek(0, SeekOrigin.Begin);
            message.AddAttachment(new LocalAttachment(data, $"suggestion_{suggestion.Id}.{suggestion.Extension}"));
            embed.WithImageUrl($"attachment://suggestion_{suggestion.Id}.{suggestion.Extension}");
        }
        else
        {
            suggestion = _db.Suggestions.Add(new Suggestion()
            {
                AuthorId = Context.AuthorId.RawValue,
                Content = content
            }).Entity;
            await _db.SaveChangesAsync();
        }

        embed.WithFooter($"Suggestion ID: {suggestion.Id} • {suggestion.CreatedAt:g}");
        var suggestionMessage = await Context.Bot.SendMessageAsync(Constants.SUGGESTIONS_CHANNEL_ID,
            message.WithEmbeds(embed).WithComponents(new LocalRowComponent()
                .AddComponent(new LocalButtonComponent().WithCustomId($"Suggestion:Vote:{suggestion.Id}:{true}")
                    .WithEmoji(LocalEmoji.FromString("<:tfYes:562680952082530304>"))
                    .WithStyle(LocalButtonComponentStyle.Secondary)).AddComponent(new LocalButtonComponent()
                    .WithCustomId($"Suggestion:Vote:{suggestion.Id}:{false}")
                    .WithEmoji(LocalEmoji.FromString("<:tfNo:562681001076195330>"))
                    .WithStyle(LocalButtonComponentStyle.Secondary))));


        suggestion.MessageId = suggestionMessage.Id;
        await _db.SaveChangesAsync();

        return Response($"Suggestion #{suggestion.Id} created.").AsEphemeral();
    }

    [SlashCommand("remove")]
    [Description("Remove a suggestion.")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> RemoveSuggestion([Description("The suggestion to remove.")] int suggestionChoice)
    {
        var view = new PromptView(x =>
            (x as LocalInteractionMessageResponse)
            .WithContent("Are you sure you wish to delete this suggestion? This action cannot be undone.")
            .WithIsEphemeral());

        await View(view);

        if (!view.Result)
            return default;

        var suggestion = await _db.Suggestions.FindAsync(suggestionChoice);
        _db.Suggestions.Remove(suggestion);
        await _db.SaveChangesAsync();

        try
        {
            var toDelete = await Bot.FetchMessageAsync(Constants.SUGGESTIONS_CHANNEL_ID, suggestion.MessageId);
            await toDelete.DeleteAsync();
        }
        catch
        {
            Logger.LogWarning($"Could not remove suggestion message for {suggestion.Id}");
        }

        return Response($"Suggestion {suggestion.Id} removed.");
    }

    [SlashCommand("approve")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> ApproveSuggestionAsync([Description("The suggestion to approve")] int suggestionChoice, [Description("Approval reason")] string reason = "")
    {
        var suggestion = await _db.Suggestions.FindAsync(suggestionChoice);
        var author = await Context.Bot.FetchUserAsync(suggestion.AuthorId);

        try
        {
            var message = await Bot.FetchMessageAsync(Constants.SUGGESTIONS_CHANNEL_ID, suggestion.MessageId);
            Logger.LogInformation($"Suggestion Message ID: {message.Id}");
            await message.DeleteAsync();
            var dm = await author.CreateDirectChannelAsync();

            await dm.SendMessageAsync(
                new LocalMessage().WithContent($"Your suggestion '{suggestion.Content.Truncate(50)}' has been approved."));
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Suggestion approved failed. Yippe!!");
        }

        suggestion.IsCompleted = true;
        await _db.SaveChangesAsync();
        var toSend = new LocalMessage();
        var embed = EmbedUtilities.SuccessBuilder.WithTitle(
                $"Suggestion #{suggestionChoice} by {author.Tag} was approved with {suggestion.UpvoteUsers.Count} upvote(s) and {suggestion.DownvoteUsers.Count} downvote(s).")
            .WithDescription(suggestion.Content)
            .WithImageUrl($"attachment://suggestion_{suggestion.Id}.{suggestion.Extension}")
            .AddField("Suggestion created at:", Markdown.Timestamp(suggestion.CreatedAt))
            .WithFooter($"Approved by {Context.Author.Tag}", Context.Author.GetAvatarUrl());

        if (!string.IsNullOrWhiteSpace(reason))
        {
            embed.AddField("Reason:", reason);
        }

        if (suggestion.Data is not null)
        {
            toSend.AddAttachment(new LocalAttachment(suggestion.Data, $"suggestion_{suggestion.Id}.{suggestion.Extension}"));
        }

        await Bot.SendMessageAsync(Constants.SUGGESTION_ARCHIVE_CHANNEL_ID, toSend.WithEmbeds(embed));
        return Response($"You successfully approved suggestion #{suggestion.Id}.");
    }

    [SlashCommand("deny")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> DenySuggestionAsync([Description("The suggestion to deny")] int suggestionChoice, [Description("The deny reason")] string reason)
    {
        var suggestion = await _db.Suggestions.FindAsync(suggestionChoice);
        var author = await Context.Bot.FetchUserAsync(suggestion.AuthorId);

        try
        {
            var message = await Bot.FetchMessageAsync(Constants.SUGGESTIONS_CHANNEL_ID, suggestion.MessageId);
            Logger.LogInformation($"Suggestion Message ID: {message.Id}");
            await message.DeleteAsync();

            var dm = await author.CreateDirectChannelAsync();

            await dm.SendMessageAsync(
                new LocalMessage().WithContent($"Your suggestion '{suggestion.Content.Truncate(50)}' has been denied. Check the archive channel for details."));
        }
        catch
        {
            Logger.LogWarning($"Could not delete message for suggestion {suggestionChoice}.");
        }

        suggestion.IsCompleted = true;
        await _db.SaveChangesAsync();
        var toSend = new LocalMessage();
        var embed = EmbedUtilities.ErrorBuilder.WithTitle(
                $"Suggestion #{suggestionChoice} by {author.Tag} was denied with {suggestion.UpvoteUsers.Count} upvote(s) and {suggestion.DownvoteUsers.Count} downvote(s).")
            .WithDescription(suggestion.Content)
            .WithImageUrl($"attachment://suggestion_{suggestion.Id}.{suggestion.Extension}")
            .AddField("Suggestion created at:", Markdown.Timestamp(suggestion.CreatedAt))
            .AddField("Reason:", reason)
            .WithFooter($"Denied by {Context.Author.Tag}", Context.Author.GetAvatarUrl());

        if (suggestion.Data is not null)
        {
            toSend.AddAttachment(new LocalAttachment(suggestion.Data, $"suggestion_{suggestion.Id}.{suggestion.Extension}"));
        }

        await Bot.SendMessageAsync(Constants.SUGGESTION_ARCHIVE_CHANNEL_ID, toSend.WithEmbeds(embed));
        return Response($"You successfully denied suggestion #{suggestion.Id}.");
    }

    [SlashCommand("status")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> ViewSuggestionStatusAsync([Description("The suggestion to view")] int suggestionChoice)
    {
        var suggestion = await _db.Suggestions.FindAsync(suggestionChoice);
        var author = await Bot.FetchUserAsync(suggestion.AuthorId);
        var embed = EmbedUtilities.SuccessBuilder.WithTitle($"Suggestion {suggestion.Id} by {author.Tag} ({author.Id})")
            .WithDescription(suggestion.Content).AddField("Upvotes:", suggestion.UpvoteUsers.Count, true)
            .AddField("Downvotes:", suggestion.DownvoteUsers.Count, true);

        return Response(embeds: embed);
    }

    [AutoComplete("remove")]
    [AutoComplete("approve")]
    [AutoComplete("deny")]
    [AutoComplete("status")]
    public async Task AutoCompleteSuggestionAsync(
        [Name("suggestion-choice")] AutoComplete<int> suggestionChoice)
    {
        List<Suggestion> suggestions;

        if (Context.Author.RoleIds.Contains(Constants.MODERATOR_ROLE_ID))
            suggestions = await _db.Suggestions.Where(x => !x.IsCompleted).ToListAsync();
        else
            suggestions = await _db.Suggestions.Where(x => x.AuthorId == Context.AuthorId.RawValue && !x.IsCompleted).ToListAsync();

        suggestionChoice.Choices.AddRange(suggestions.Take(25).ToDictionary(x => $"#{x.Id} - {x.Content.TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength - 25)}", x => x.Id));
    }
}