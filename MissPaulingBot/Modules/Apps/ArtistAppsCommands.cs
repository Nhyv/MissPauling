using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Checks;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using Qmmands;

namespace MissPaulingBot.Modules.Apps
{
    [SlashGroup("artist")]
    [Description("Artist Apps Commands.")]
    public class ArtistAppsCommands : DiscordApplicationModuleBase
    {
        private readonly PaulingDbContext _db;

        public ArtistAppsCommands(PaulingDbContext db)
        {
            _db = db;
        }

        [SlashCommand("apply")]
        [Description("Apply for the artist role in the TF2 Community Discord.")]
        public async Task<IResult> Apply([Description("Your artwork.")] [SupportedFileExtensions("png", "jpg", "jpeg", "gif", "gifv", "webm", "mp4", "wav", "mp3", "ogg")] IAttachment attachment)
        {
            if (_db.ArtistApplications.FirstOrDefault(x => x.UserId == Context.Author.Id.RawValue) is { } app || (Context.Author is IMember member && member.RoleIds.Contains(Constants.ARTIST_ROLE_ID)))
                return Response("You've already applied.").AsEphemeral();

            _db.ArtistApplications.Add(new ArtistApplication
            {
                UserId = Context.Author.Id.RawValue,
                UploadUrl = attachment.Url
            });

            await _db.SaveChangesAsync();

            await Context.Bot.SendMessageAsync(Constants.APPLICATION_CHANNEL_ID,
                new LocalMessage().WithContent(
                        $"**Artist Application**\n**{Context.Author.Tag}** - (`{Context.Author.Id}`)\n{attachment.Url}")
                    .WithComponents(new LocalRowComponent()
                        .WithComponents(
                        new LocalButtonComponent()
                            .WithStyle(LocalButtonComponentStyle.Success)
                            .WithLabel("Approve")
                            .WithCustomId($"ArtistApplication:Approve:{Context.AuthorId}"),
                        new LocalButtonComponent()
                            .WithStyle(LocalButtonComponentStyle.Danger)
                            .WithLabel("Deny")
                            .WithCustomId($"ArtistApplication:Deny:{Context.AuthorId}"))));

            return Response("Application sent!").AsEphemeral();

        }
    }
}