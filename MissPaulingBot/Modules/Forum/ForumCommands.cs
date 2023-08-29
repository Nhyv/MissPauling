using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Extensions;
using MissPaulingBot.Services;
using Qmmands;

namespace MissPaulingBot.Modules.Forum;

public class ForumCommands : DiscordApplicationGuildModuleBase
{
    private readonly ForumService _forumService;

    public ForumCommands(ForumService forumService)
    {
        _forumService = forumService;
    }

    [SlashCommand("close")]
    [Description("Closes a forum post")]
    public async Task<IResult> CloseForumPostAsync()
    {
        var thread = Bot.GetChannel(Constants.TF2_GUILD_ID, Context.ChannelId) as IThreadChannel;

        if (thread is null)
        {
            thread = await Bot.FetchChannelAsync(Context.ChannelId) as IThreadChannel;
        }

        var updatedTags = thread.TagIds.Append(thread.ChannelId == Constants.SUGGESTION_FEEDBACK_FORUM_ID ? (Snowflake)Constants.RESOLVED_SUGGESTION_TAG_ID : Constants.RESOLVED_HELP_TAG_ID).Distinct();

        if (thread.CreatorId == Context.AuthorId)
        {
            var view = new PromptView(x =>
                (x as LocalInteractionMessageResponse)
                .WithContent(
                    "Are you sure you really wish to close this thread? You will no longer be able to receive replies in it.")
                .WithIsEphemeral());

            await View(view);

            if (!view.Result)
                return default;

            await thread.SendMessageAsync(new LocalMessage().WithContent("Closed by the OP."));
            await thread.ModifyAsync(x => x.TagIds = updatedTags.ToList());
            await _forumService.CloseThreadAsync(thread);
            return default;
        }

        if (Context.Author.RoleIds.Contains(Constants.MODERATOR_ROLE_ID))
        {
            var modal = new LocalInteractionModalResponse().WithTitle("Closing Forum Post").WithCustomId("Forum:ModClose")
                .WithComponents(new LocalRowComponent().WithComponents(new LocalTextInputComponent().WithLabel("Why are you closing this post?")
                    .WithStyle(TextInputComponentStyle.Paragraph).WithIsRequired().WithCustomId("reason")
                    .WithPlaceholder("Reason for closing the forum post...")
                    .WithMaximumInputLength(1500)));
            await Context.Interaction.Response().SendModalAsync(modal);
            return default;
        }

        return Response("This command may only be used by moderators or the OP.").AsEphemeral();
    }

    [MessageCommand("Mark As Solution")]
    public async Task<IResult> MarkAsSolutionAsync(IMessage message)
    {
        if (message.Author.Id == 527693799531741205)
            return Response("You cannot mark Pauling as the solution!").AsEphemeral();

        if (!(Bot.GetChannel(Constants.TF2_GUILD_ID, message.ChannelId) is IThreadChannel channel))
            return Response("This can only be used in a forum post.").AsEphemeral();

        if (Context.AuthorId != channel.CreatorId && !Context.Author.RoleIds.Contains(Constants.MODERATOR_ROLE_ID))
            return Response("Only moderators or the OP can mark an answer as the solution.");

        await channel.SendMessageAsync(new LocalMessage().WithContent($"{Context.Author.Mention} has marked this message as the solution/answer.").WithAllowedMentions(LocalAllowedMentions.None).WithReply(message.Id, message.ChannelId, Context.GuildId));
        return Response(
            $"Make sure to close this post using the close button or {Mention.SlashCommand(1025851488624455730, "close")} command once you are no longer in need of assistance.").AsEphemeral();
    }
}