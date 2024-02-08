using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace MissPaulingBot.Common.Menus.Views.Applications;

public class AgeResponseView : ModAppViewBase
{
    [Selection(Row = 1)]
    [SelectionOption("13-17")]
    [SelectionOption("18-21")]
    [SelectionOption("21-25")]
    [SelectionOption("26+")]
    public ValueTask SelectAgeRangeAsync(SelectionEventArgs e)
    {
        App.AgeResponse = e.SelectedOptions[0].Value.Value;
        ReportChanges();
        return ValueTask.CompletedTask;
    }

    protected override void FormatLocalEmbed(LocalEmbed embed)
    {
        embed.WithTitle("What is your age?")
            .WithDescription(App.AgeResponse);
    }
}