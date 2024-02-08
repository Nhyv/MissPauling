using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Qommon;

namespace MissPaulingBot.Common.Checks;

public class SupportedFileExtensions : DiscordParameterCheckAttribute
{
    public SupportedFileExtensions(params string[] allowedExtensions)
    {
        Guard.IsNotEmpty(allowedExtensions);

        AllowedExtensions = allowedExtensions;
    }

    private string[] AllowedExtensions { get; }

    public override bool CanCheck(IParameter parameter, object? value)
    {
        return value is IAttachment;
    }

    public override ValueTask<IResult> CheckAsync(IDiscordCommandContext context, IParameter parameter, object? argument)
    {
        var attachment = (IAttachment)argument!;

        var uri = new Uri(attachment.Url);

        var extension = Path.GetExtension(uri.AbsolutePath);

        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedExtensions.Contains(extension[1..], StringComparer.InvariantCultureIgnoreCase))
            return Results.Failure($"The supplied URL was not to a file of the following type(s): {string.Join(',', AllowedExtensions)}.");

        return Results.Success;
    }
}