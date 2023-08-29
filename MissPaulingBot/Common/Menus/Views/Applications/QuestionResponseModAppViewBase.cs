using Disqord.Extensions.Interactivity.Menus;
using Disqord;
using System.Threading.Tasks;
using System;
using System.Linq;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;

namespace MissPaulingBot.Common.Menus.Views.Applications;

public abstract class QuestionResponseModAppViewBase : ModAppViewBase
{
    public abstract bool ResponseIsRequired { get; }

    public abstract string Question { get; }

    public abstract string GetCurrentResponse();

    public abstract void ModifyApp(string response);

    [Button(Label = "Edit response", Row = 1, Style = LocalButtonComponentStyle.Secondary)]
    public async ValueTask EditResponseAsync(ButtonEventArgs e)
    {
        var modal = FormatModal(out var customId);
        await e.Interaction.Response().SendModalAsync(modal);

        _ = Task.Run(async () =>
        {
            var interaction = await Menu.Client.WaitForInteractionAsync<IModalSubmitInteraction>(Menu.ChannelId, customId);
            if (interaction is null)
                return;

            var test = ((IRowComponent)interaction.Components[1]).Components;
            var response = ((IRowComponent)interaction.Components[1]).Components
                .OfType<ITextInputComponent>().Single(x => x.CustomId == "response").Value;

            if (string.IsNullOrWhiteSpace(response) && ResponseIsRequired)
            {
                await interaction.Response().SendMessageAsync(new LocalInteractionMessageResponse()
                    .WithContent("You must provide a response to this question.")
                    .WithIsEphemeral());

                return;
            }

            ModifyApp(response);

            await interaction.Response().SendMessageAsync(new LocalInteractionMessageResponse()
                .WithContent("Your response has been modified. Be sure to save & exit when done if you are not submitting your app now!")
                .WithIsEphemeral());

            ReportChanges();
            await Menu.ApplyChangesAsync(e);
        });
    }

    public override void FormatLocalEmbed(LocalEmbed embed)
    {
        embed.WithTitle(ResponseIsRequired ? $"{Question}*" : Question)
            .WithDescription(GetCurrentResponse() ?? Markdown.Italics("No response."));

        if (ResponseIsRequired)
            embed.WithFooter("*Response required for this question.");
    }

    private LocalInteractionModalResponse FormatModal(out string customId)
    {
        customId = Guid.NewGuid().ToString();

        var questionInput = new LocalTextInputComponent()
            .WithLabel("Question")
            .WithCustomId("question")
            .WithStyle(TextInputComponentStyle.Paragraph)
            .WithPlaceholder("Question goes here. Close and re-open to view the question again.")
            .WithPrefilledValue(Question)
            .WithIsRequired(false);

        var responseInput = new LocalTextInputComponent()
            .WithLabel("Response")
            .WithCustomId("response")
            .WithStyle(TextInputComponentStyle.Paragraph)
            .WithPlaceholder("Type your response here...")
            .WithIsRequired(ResponseIsRequired);

        var currentResponse = GetCurrentResponse();
        if (!string.IsNullOrWhiteSpace(currentResponse))
            responseInput.WithPrefilledValue(currentResponse);

        return new LocalInteractionModalResponse()
            .WithTitle($"Question #{Menu.CurrentIndex}")
            .WithCustomId(customId)
            .WithComponents(LocalComponent.Row(questionInput), LocalComponent.Row(responseInput));
    }
}