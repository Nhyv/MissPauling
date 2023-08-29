using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

namespace MissPaulingBot.Common.Menus.Views;

public class ForumModView : ViewBase
{
    private static readonly List<string> PremadeReasons = new()
    {
        "Answered",
        "Author found a solution",
        "Off-topic",
        "Shitpost",
        "Author is inactive in post",
        "Please use the trading channels",
        "Your solution is on the wiki",
        "Other"
    };

    public ForumModView() : base(x => (x as LocalInteractionMessageResponse).WithContent("Select a closing reason").WithIsEphemeral())
    {
        AddComponent(new SelectionViewComponent(CloseForumAsync)
        {
            Placeholder = "Choose a premade reason...",
            Options = PremadeReasons.Select(x => new LocalSelectionComponentOption
            {
                Label = x,
                Value = x
            }).ToList()
        });
    }

    private async ValueTask CloseForumAsync(SelectionEventArgs e)
    {
        var reason = e.SelectedOptions[0].Value;
        var thread = await Menu.Client.FetchChannelAsync(e.ChannelId) as IThreadChannel;

        if (reason.Equals("Other"))
        {

        }

    } 
}