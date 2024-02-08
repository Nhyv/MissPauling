using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace MissPaulingBot.Common.Menus.Views.Applications;

public class ChannelsResponseView : ModAppViewBase
{
    [Selection(Row = 1, Type = SelectionComponentType.Channel, ChannelTypes = new[] { ChannelType.Text }, MinimumSelectedOptions = 1, MaximumSelectedOptions = 10)]
    public ValueTask SelectAgeRangeAsync(SelectionEventArgs e)
    {
        App.ChannelsResponse = string.Join("\n", e.SelectedEntities.Select(x => Mention.Channel(x.Id)));
        ReportChanges();
        return ValueTask.CompletedTask;
    }

    protected override void FormatLocalEmbed(LocalEmbed embed)
    {
        embed.WithTitle("Which channels are you active in?")
            .WithDescription(App.ChannelsResponse);
    }
}