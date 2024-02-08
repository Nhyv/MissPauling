using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Gateway;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation;

[SlashGroup("notes")]
[Description("Notes commands.")]
[RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
public class NotesCommands : DiscordApplicationModuleBase
{
    private readonly PaulingDbContext _db;

    public NotesCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("list")]
    [Description("View a user's notes.")]
    public IResult ViewUserNotes([Description("The user you wish to view notes for.")] IUser user)
    {
        var notes = _db.UserNotes.Where(x => x.UserId == user.Id.RawValue).ToList();

        if (!notes.Any())
        {
            return Response("Could not find notes for this user.");
        }

        var embed = EmbedUtilities.SuccessBuilder.WithTitle($"Notes for {Context.Bot.GetOrFetchUserAsync(user.Id).Result!.Tag} (`{user.Id}`)");

        foreach (var note in notes)
        {
            embed.AddField($"#{note.Id}", $"*{note.Note}*\n**Moderator:** <@{note.ModeratorId}> (`{note.ModeratorId}`)\n{note.GivenAt:g} UTC");
        }

        return Response(embed);
    }

    [SlashCommand("add")]
    [Description("Add a note to a user.")]
    public async Task<IResult> AddNoteAsync([Description("The user to add a note to.")] IUser user, [Description("The note to add.")] string note)
    {
        _db.Add(new UserNote()
        {
            UserId = user.Id.RawValue,
            ModeratorId = Context.Author.Id.RawValue,
            Note = note,
            Username = user.Tag
        });

        await _db.SaveChangesAsync();
        return Response("Note created.");
    }

    [SlashCommand("remove")]
    [Description("Remove a note from a user.")]
    public async Task<IResult> RemoveNoteAsync([Description("The note ID.")] int id)
    {
        if (_db.UserNotes.Find(id) is not { } note)
        {
            return Response("Note not found.");
        }

        var embed = EmbedUtilities.SuccessBuilder
            .WithTitle($"Note {note.Id} for {note.Username} by {Context.Bot.GetGuild(Constants.TF2_GUILD_ID)!.GetMember(note.ModeratorId)}")
            .WithDescription($"{note.Note}");

        var view = new PromptView(x => x.WithEmbeds(embed));
        await View(view);

        if (view.Result)
        {
            _db.Remove(note);
            await _db.SaveChangesAsync();
            return Response("Note successfully removed.");
        }

        return Response("Operation cancelled. Thanks for using Airbnb.");
    }
}