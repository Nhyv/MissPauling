using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class TradingGuidelinesService : DiscordBotService
{
    public const string TRADING_GUIDELINES_BUTTON_CUSTOM_ID = "TradingGuidelinesButton_896142934112632882";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Bot.WaitUntilReadyAsync(stoppingToken);

        var messages = await Bot.FetchMessagesAsync(Constants.TRADING_GUIDELINES_CHANNEL_ID);
        var guidelineMessage = messages.OfType<IUserMessage>().FirstOrDefault(x =>
        {
            foreach (var row in x.Components)
            {
                foreach (var component in row.Components.OfType<IButtonComponent>())
                {
                    if (component.CustomId.Equals(TRADING_GUIDELINES_BUTTON_CUSTOM_ID))
                    {
                        return true;
                    }
                }
            }

            return false;
        });

        if (guidelineMessage is null)
        {
            await Bot.SendMessageAsync(Constants.TRADING_GUIDELINES_CHANNEL_ID,
                new LocalMessage().WithContent(
                    "**Guidelines**\nBelow are the guidelines you must follow at ALL TIMES in the trading " +
                    "channels.\n\n**Do not use #trading-discussions to send your ads.** This includes asking people to buy your items.\n\n**Steam trades only.** If your trade cannot be done entirely on Steam in **ONE** trade (no trust trade, brokering, etc.), you" +
                    " shouldn't be advertising it here.\n\n**All non image links must be wrapped not to take" +
                    " unnecessary space.** To wrap your links, you must put < > around your link, like this:" +
                    " `<link>`\n\n**Your image must" +
                    " feature your items and/or a description of what you are selling.** Even if you have" +
                    " the trade of the century underneath it, we don't allow off-topic images on trades." +
                    "\n\n**Your advertisement must take less than 20 lines.** We get it, you want your" +
                    " advertisement to be pretty and have some space to make it easier to read. However, you must" +
                    " remember other people want to share their trades and if you take the whole channel with" +
                    " one ad, it becomes a problem.\n\n**No begging.** This is already a part of our server rules and won't change even in our trading community. Asking to receive 'unwanted' items counts as begging.\n\nIf you understand the above guidelines and want to get" +
                    " access to the trading channels, press the button down below. However, do remember once you" +
                    " accept said guidelines, we will no longer take 'I didn't know' as an excuse.").WithComponents(
                    new LocalRowComponent().AddComponent(new LocalButtonComponent()
                        .WithLabel("I agree to the guidelines.").WithStyle(LocalButtonComponentStyle.Success)
                        .WithCustomId(TRADING_GUIDELINES_BUTTON_CUSTOM_ID))));
        }
    }
}