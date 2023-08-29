using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Apps;

[SlashGroup("streamer")]
[Description("Streamer Apps Commands.")]
public class StreamerAppsCommands : DiscordApplicationModuleBase
{
    private PaulingDbContext _db;

    public StreamerAppsCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("apply")]
    [Description("Apply for the streamer role in the TF2 Community Discord.")]
    public async Task<IResult> Apply()
    {
        if (_db.StreamerApplications.FirstOrDefault(x => x.UserId == Context.Author.Id.RawValue) is { } app)
            return Response("You've already applied.").AsEphemeral();

        var modal = new LocalInteractionModalResponse().WithTitle("Streamer Application")
            .WithCustomId($"StreamerApplication:Apply").WithComponents(new LocalRowComponent().WithComponents(new LocalTextInputComponent()
                .WithLabel("What type of content will you stream?")
                .WithStyle(TextInputComponentStyle.Short).WithCustomId("content")
                .WithIsRequired()
                .WithPlaceholder("Enter your answer here...")), new LocalRowComponent().WithComponents(new LocalTextInputComponent()
                .WithLabel("Why do you want to stream?").WithCustomId("reason")
                .WithStyle(TextInputComponentStyle.Paragraph)
                .WithIsRequired()
                .WithMaximumInputLength(Discord.Limits.Message.Embed.Field.MaxValueLength)
                .WithPlaceholder("Enter your answer here...")));
        await Context.Interaction.Response().SendModalAsync(modal);

        return default;

        
    }
}

public class StreamerAppsModalCommands : DiscordComponentModuleBase
{
    private readonly PaulingDbContext _db;

    public StreamerAppsModalCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [ModalCommand("StreamerApplication:Apply")]
    public async Task<IResult> ApplyAsync(string content, string reason)
    {
        _db.StreamerApplications.Add(new StreamerApplication
        {
            UserId = Context.Author.Id.RawValue,
            ContentAnswer = content,
            ReasonAnswer = reason
        });

        await _db.SaveChangesAsync();

        var embed = EmbedUtilities.SuccessBuilder.WithAuthor($"{Context.Author.Tag} ({Context.Author.Id})", Context.Author.GetAvatarUrl())
            .WithFields(new LocalEmbedField().WithName("What type of content will you stream?")
                .WithValue(content), new LocalEmbedField()
                .WithName("Why do you want to stream?").WithValue(reason));
        await Context.Bot.SendMessageAsync(Constants.APPLICATION_CHANNEL_ID,
            new LocalMessage().WithEmbeds(embed).WithComponents(new LocalRowComponent().WithComponents(
                new LocalButtonComponent().WithStyle(LocalButtonComponentStyle.Success).WithLabel("Approve")
                    .WithCustomId($"StreamerApplication:Approve:{Context.AuthorId}"),
                new LocalButtonComponent().WithStyle(LocalButtonComponentStyle.Danger).WithLabel("Deny")
                    .WithCustomId($"StreamerApplication:Deny:{Context.AuthorId}"))));

        return Response("Thank you for applying!").AsEphemeral();
    }
}