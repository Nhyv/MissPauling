using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissPaulingBot.Common.Parsers;
using Qmmands.Default;

namespace MissPaulingBot
{
    public sealed class MissPaulingBot : DiscordBot
    {
        public MissPaulingBot(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IServiceProvider services, DiscordClient client) : base(options, logger, services, client)
        {

        }

        protected override ValueTask AddTypeParsers(DefaultTypeParserProvider typeParserProvider, CancellationToken cancellationToken)
        {
            typeParserProvider.AddParser(new DmMessageTemplateTypeParser());
            return base.AddTypeParsers(typeParserProvider, cancellationToken);
        }
    }
}