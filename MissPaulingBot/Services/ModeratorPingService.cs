using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class ModeratorPingService : DiscordBotService
{
    protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.Member is null || e.Member.RoleIds.Contains(Constants.MODERATOR_ROLE_ID) || e.Member.IsBot)
        {
            return;
        }

        if (!e.Message.Content.Contains(Mention.Role(Constants.MODERATOR_ROLE_ID)))
            return;
        
        await Bot.SendMessageAsync(e.ChannelId,
            new LocalMessage().WithContent("Awaiting moderator response...").WithReply(e.MessageId).WithComponents(
                new LocalRowComponent().WithComponents(new LocalButtonComponent
                {
                    CustomId = "ModPingAssign",
                    Label = "Assign this to me",
                    Style = LocalButtonComponentStyle.Primary
                })));
    }
}