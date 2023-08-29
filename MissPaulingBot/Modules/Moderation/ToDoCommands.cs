using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation;

[SlashGroup("todo")]
[Description("Nhyv's todo list")]
public class ToDoCommands : DiscordApplicationModuleBase
{
    private readonly PaulingDbContext _db;

    public ToDoCommands(PaulingDbContext db)
    {
        _db = db;
    }

    [SlashCommand("create")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> CreateTodoAsync([Description("Brief name of TODO.")]string title, [Description("What you want done.")] string description)
    {
        _db.ToDos.Add(new ToDo
        {
            Description = description,
            Title = title,
            ModeratorId = Context.AuthorId.RawValue
        });

        await _db.SaveChangesAsync();

        try
        {
            var dm = await Bot.CreateDirectChannelAsync(227578898521653249);
            await dm.SendMessageAsync(new LocalMessage().WithContent("New TODO entry available!"));
        }
        catch
        {
        }

        return Response("Your request was added to Nhyv's ToDo list!").AsEphemeral();
    }

    [SlashCommand("markasdone")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> MarkAsDoneAsync([Description("The todo entry ID.")] int id)
    {
        var todo = await _db.ToDos.FindAsync(id);

        try
        {
            var dm = await Bot.CreateDirectChannelAsync(todo.ModeratorId);
            await dm.SendMessageAsync(new LocalMessage().WithContent(
                $"Your suggestion '{todo.Title}' has been completed! An announcement post may be posted for more details."));
        }
        catch
        {
            var dm = await Bot.CreateDirectChannelAsync(227578898521653249);
            await dm.SendMessageAsync(new LocalMessage().WithContent($"Could not dm {todo.ModeratorId}!"));
        }

        _db.ToDos.Remove(todo);
        await _db.SaveChangesAsync();

        return Response("Todo successfully completed.").AsEphemeral();
    }

    [SlashCommand("status")]
    [RequireAuthorRole(Constants.MODERATOR_ROLE_ID)]
    public async Task<IResult> ViewTodoStatusAsync([Description("The todo entry ID.")] int id)
    {
        var todo = await _db.ToDos.FindAsync(id);
        var moderator = await Bot.GetOrFetchUserAsync(todo.ModeratorId);
        var embed = EmbedUtilities.SuccessBuilder.WithTitle(todo.Title).WithDescription(todo.Description)
            .AddField("Requested By", moderator.Tag).AddField("Created",
                Markdown.Timestamp(todo.CreatedAt));

        return Response(embed);
    }

    [SlashCommand("remove")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    public async Task<IResult> RemoveTodoAsync(int id)
    {
        var todo = await _db.ToDos.FindAsync(id);

        _db.ToDos.Remove(todo);
        await _db.SaveChangesAsync();

        return Response("Todo successfully removed.").AsEphemeral();
    }

    [AutoComplete("markasdone")]
    [AutoComplete("status")]
    [AutoComplete("remove")]
    public async Task AutoCompleteToDoAsync([Name("id")] AutoComplete<int> id)
    {
        var toDos = await _db.ToDos.ToListAsync();

        id.Choices.AddRange(toDos.Take(25).ToDictionary(
            x => $"{x.Id} - {x.Description.TrimTo(Discord.Limits.ApplicationCommand.Option.Choice.MaxNameLength - 25)}"
            , x => x.Id));
    }
}