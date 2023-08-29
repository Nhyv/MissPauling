using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Checks;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using Qmmands;

namespace MissPaulingBot.Modules.Premium;

[SlashGroup("premium")]
[RequireChannel(Constants.PREMIUM_CHANNEL_ID)]
public class PremiumCommands : DiscordApplicationGuildModuleBase
{
    private readonly PaulingDbContext _db;
    private readonly HttpClient _http;

    public PremiumCommands(PaulingDbContext db, HttpClient http)
    {
        _db = db;
        _http = http;
    }

    [SlashCommand("role")]
    [Description("Create/Update Saxton's Own custom role")]
    [RequireAuthorRole(Constants.SAXTON_OWN_ROLE_ID)]
    public async Task<IResult> ManageRoleAsync([Description("Role name")]string? name = null, [Description("Role color")] Color? color = null, [Description("Role icon")][SupportedFileExtensions("png", "jpg", "jpeg", "webm")]IAttachment iconImage = null)
    {
        await Deferral();

        if (name is null && color is null & iconImage is null)
            return Response("You cannot provide nothing.").AsEphemeral();

        IRole role = null;
        var deathMerchantRole = Bot.GetRole(Constants.TF2_GUILD_ID, Constants.DEATH_MERCHANT_ROLE_ID);

        // If the user already has a Saxton's Own Custom Role.
        if (await _db.SaxtonOwnRoles.FindAsync(Context.AuthorId.RawValue) is { } existingRole)
        {
            role = Bot.GetRole(Constants.TF2_GUILD_ID, existingRole.Id);
            // If they provided an image.
            if (iconImage != null)
            {
                var data = await _http.GetMemoryStreamAsync(iconImage.Url);

                existingRole.Data = data;
                existingRole.Extension = Path.GetExtension(new Uri(iconImage.Url).AbsolutePath)[1..].ToLower();
                existingRole.Color = color ?? existingRole.Color;
            }

            existingRole.Name = name ?? existingRole.Name;
            existingRole.Color = color ?? existingRole.Color;

            await Bot.ModifyRoleAsync(Context.GuildId, existingRole.Id, x =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    x.Name = name;
                if (iconImage != null)
                    x.Icon = existingRole.Data;
                if (color != null)
                    x.Color = color;
            });
        }
        else
        {
            // If they provided an image
            if (iconImage != null)
            {
                // Get the image 
                var data = await _http.GetMemoryStreamAsync(iconImage.Url);

                role = await Bot.CreateRoleAsync(Context.GuildId, x =>
                {
                    x.Name = name ?? Context.Author.Tag;
                    x.Color = color;
                    x.Icon = data;
                });
                await role.ModifyAsync(x => x.Position = deathMerchantRole.Position + 2);

                _db.SaxtonOwnRoles.Add(new SaxtonOwnRole
                {
                    Id = role.Id.RawValue,
                    OwnerId = Context.AuthorId.RawValue,
                    Name = name,
                    Color = color,
                    Data = data,
                    Extension = Path.GetExtension(new Uri(iconImage.Url).AbsolutePath)[1..].ToLower()
                });
            }
            else
            {
                role = await Bot.CreateRoleAsync(Context.GuildId, x =>
                {
                    x.Name = name ?? Context.Author.Tag;
                    x.Color = color;
                });

                await role.ModifyAsync(x => x.Position = deathMerchantRole.Position + 1);

                _db.SaxtonOwnRoles.Add(new SaxtonOwnRole
                {
                    Id = role.Id.RawValue,
                    Name = name,
                    Color = color,
                    OwnerId = Context.AuthorId.RawValue
                });

            }
            await Bot.GrantRoleAsync(Context.GuildId, Context.AuthorId, role.Id);
        }

        await _db.SaveChangesAsync();
        return Response($"You have created or updated your role {role.Name}");
    }
}