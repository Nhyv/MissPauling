using System;
using System.Threading.Tasks;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common.Models;
using Qmmands;

namespace MissPaulingBot.Common.Parsers;

public class DmMessageTemplateTypeParser : DiscordGuildTypeParser<DmMessageTemplate>
{
    public override async ValueTask<ITypeParserResult<DmMessageTemplate>> ParseAsync(IDiscordGuildCommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
    {
        var str = value.ToString().ToLower();

        using var scope = context.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        if (await db.DmMessageTemplates.FindAsync(str) is { } template)
        {
            return Success(template);
        }

        return Failure("No template could be found with that name.");
    }
}