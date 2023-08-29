using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation
{
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    [Description("Owner only commands")]
    public class OwnerCommands : DiscordApplicationModuleBase
    {
        private readonly PaulingDbContext _db;

        public OwnerCommands(PaulingDbContext db)
        {
            _db = db;
        }

        [SlashCommand("createwebhook")]
        [Description("Creates a webhook.")]
        public async Task<IResult> CreateWebhookAsync([Description("The channel")][ChannelTypes(ChannelType.Text)] IChannel publicationChannel, [Description("Webhook name")] string name)
        {
            var webhook = await Bot.CreateWebhookAsync(publicationChannel.Id, name);

            return Response($"Webhook created with name {name} for channel #{publicationChannel.Name}. Token: {webhook.Token} Id: {webhook.Id}").AsEphemeral();
        }

        [SlashCommand("state")]
        [Description("Changes Pauling's status")]
        public async Task<IResult> SetStateAsync([Description("The status.")] UserStatus status)
        {
            if (status == UserStatus.Offline)
                status = UserStatus.Invisible;

            await Context.Bot.SetPresenceAsync(status);
            return Response("<a:kuMusic:831728441412812821>");
        }
    }
}