using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Rest;

namespace MissPaulingBot.Common.Menus.Views;

public class DmPromptView : PromptView
{
    public DmPromptView(Action<LocalMessageBase> messageTemplate, string content) : base(messageTemplate)
    {
        AddComponent(new ButtonViewComponent(EditDmAsync)
        {
            Label = "Edit"
        });
        
        Content = content;
    }
    
    public string Content { get; set; }

    protected override async ValueTask OnConfirmButton(ButtonEventArgs e)
    {
        await e.Interaction.Message.ModifyAsync(x => x.Components = new List<LocalRowComponent>());
        await CompleteAsync(true, e);
    }

    protected override async ValueTask OnDenyButton(ButtonEventArgs e)
    {
        await e.Interaction.Message.ModifyAsync(x => x.Components = new List<LocalRowComponent>());
        await CompleteAsync(false, e);
    }
    
    private async ValueTask EditDmAsync(ButtonEventArgs e)
    {
        var guid = Guid.NewGuid();
        var modal = new LocalInteractionModalResponse().WithTitle("Edit").WithCustomId($"Dm:Edit:{guid}").WithComponents(
            new LocalRowComponent().WithComponents(new LocalTextInputComponent().WithCustomId("content").WithLabel("Content").WithIsRequired()
                .WithStyle(TextInputComponentStyle.Paragraph)
                .WithPrefilledValue(e.Interaction.Message.Embeds[0].Description)));

        await e.Interaction.Response().SendModalAsync(modal);

        _ = Task.Run(async () =>
        {
            var interaction =
                await Menu.Client.WaitForInteractionAsync<IModalSubmitInteraction>(Menu.ChannelId, $"Dm:Edit:{guid}",
                    timeout: TimeSpan.FromHours(1));

            if (interaction is null)
                return;

            await interaction.Response().SendMessageAsync(new LocalInteractionMessageResponse().WithContent("Message updated.").WithIsEphemeral());

            var editContent = ((IRowComponent)interaction.Components[0]).Components.OfType<ITextInputComponent>()
                .Single(x => x.CustomId == "content").Value;
            var message = e.Interaction.Message;
            var embed = e.Interaction.Message.Embeds[0];

            Content = editContent;

            await message.ModifyAsync(x => x.Embeds = new List<LocalEmbed>()
                { LocalEmbed.CreateFrom(embed).WithDescription(editContent) });
        });
    }
}