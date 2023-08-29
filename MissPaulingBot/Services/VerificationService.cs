using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class VerificationService : DiscordBotService
{
    protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
    {
        using var scope = Bot.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();
        var verifications = db.ModmailVerifications.ToList();

        if (verifications.FirstOrDefault(x => x.UserId == e.MemberId.RawValue) is { } verification)
        {
            await Client.DeleteMessageAsync(Constants.MODMAIL_CHANNEL_ID, verification.MessageId);

            db.ModmailVerifications.Remove(verification);
            await db.SaveChangesAsync();
        }
    }
}