using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;

namespace MissPaulingBot.Common.Menus.Views.Applications;

public abstract class ModAppViewBase : ViewBase
{
    private readonly ButtonViewComponent _previousQuestionButton;
    private readonly ButtonViewComponent _nextQuestionButton;

    protected ModAppViewBase()
        : base(null)
    {
        _previousQuestionButton = new ButtonViewComponent(PreviousQuestionAsync)
        {
            Label = "Previous question",
            //IsDisabled = menu.CurrentIndex == 0,
            Row = 0,
            Position = 0
        };
        _nextQuestionButton = new ButtonViewComponent(NextQuestionAsync)
        {
            Label = "Next question",
            //IsDisabled = menu.CurrentIndex == menu.Views.Count - 1,
            Row = 0,
            Position = 3
        };

        AddComponent(_previousQuestionButton);
        AddComponent(new ButtonViewComponent(SaveAndExitAsync)
        {
            Label = "Save and exit",
            Style = LocalButtonComponentStyle.Success,
            Row = 0,
            Position = 1
        });
        AddComponent(_nextQuestionButton);
    }

    public new ModAppMenu Menu => (ModAppMenu)base.Menu;

    public ModApplication App => Menu.App;

    public abstract void FormatLocalEmbed(LocalEmbed embed);

    public ValueTask PreviousQuestionAsync(ButtonEventArgs e)
        => Menu.SetViewAsync(--Menu.CurrentIndex);

    public async ValueTask SaveAndExitAsync(ButtonEventArgs e)
    {
        ClearComponents();
        await Menu.ApplyChangesAsync(e);
        await Menu.SaveChangesAsync();
        Menu.Stop();

        await e.Interaction.Followup().SendAsync(new LocalInteractionMessageResponse()
            .WithContent("Your app has been saved.")
            .WithIsEphemeral());
    }

    [Button(Label = "Submit", Style = LocalButtonComponentStyle.Danger, Row = 0, Position = 2)]
    public async ValueTask SubmitAsync(ButtonEventArgs e)
    {
        await e.Interaction.Response().DeferAsync();

        if (!Menu.App.TryValidate(out var error))
        {
            await e.Interaction.Followup().SendAsync(
                new LocalInteractionMessageResponse()
                    .WithContent("One or more errors occurred validating your responses to the moderator application:\n" + error)
                    .WithIsEphemeral());
            return;
        }

        Menu.App.HasApplied = true;
        ClearComponents();
        await Menu.ApplyChangesAsync(e);
        await Menu.SaveChangesAsync();
        Menu.Stop();

        await Menu.Client.SendMessageAsync(Constants.MODAPPS_CHANNEL_ID, new LocalMessage().WithContent($"User **{e.Member.Tag}** (`{e.AuthorId}`) has applied! Please use {Mention.SlashCommand(1107741102796177418, "modapps view")}"));
        await e.Interaction.Followup().SendAsync(
            new LocalInteractionMessageResponse()
                .WithContent("Your moderator application has been submitted and is pending review!")
                .WithIsEphemeral());
    }

    public ValueTask NextQuestionAsync(ButtonEventArgs e)
        => Menu.SetViewAsync(++Menu.CurrentIndex);

    public override ValueTask UpdateAsync()
    {
        _previousQuestionButton.IsDisabled = Menu.CurrentIndex == 0;
        _nextQuestionButton.IsDisabled = Menu.CurrentIndex == Menu.Views.Count - 1;

        return base.UpdateAsync();
    }

    public sealed override void FormatLocalMessage(LocalMessageBase message)
    {
        base.FormatLocalMessage(message);

        if (message is LocalInteractionMessageResponse response)
            response.WithIsEphemeral();

        message.WithContent($"Question #{Menu.CurrentIndex + 1}");
        var embed = EmbedUtilities.SuccessBuilder;
        FormatLocalEmbed(embed);
        message.WithEmbeds(embed);
    }
}