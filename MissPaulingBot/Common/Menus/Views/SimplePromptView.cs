using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Rest;

namespace MissPaulingBot.Common.Menus.Views;

public class SimplePromptView : PromptView
{
    public SimplePromptView(Action<LocalMessageBase> messageTemplate) : base(messageTemplate)
    {
        AbortMessage = "Operation cancelled. Thanks for using Airbnb.";
    }

    

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
}