using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using MissPaulingBot.Common.Menus.Views.Applications;
using MissPaulingBot.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MissPaulingBot.Common.Menus;

public class ModAppMenu : DefaultInteractionMenu
{
    public ModAppMenu(ModApplication app, IUserInteraction interaction)
        : base(new AgeResponseView(), interaction)
    {
        App = app;
        Views = new List<ModAppViewBase>
        {
            new AgeResponseView(),
            new AvailabilitiesResponseView(),
            new ChannelsResponseView(),
            new QualificationResponseView(),
            new ReasonResponseView(),
            new PersonalResponseView(),
            new ButtonsResponseView(),
            new ButtingHeadsResponseView(),
            new AbuseResponseView(),
            new ChangeResponseView()
        };
    }

    public ModApplication App { get; }

    public List<ModAppViewBase> Views { get; }

    public int CurrentIndex { get; set; }

    public ValueTask SetViewAsync(int index)
        => SetViewAsync(Views[index]);


    public async ValueTask SaveChangesAsync()
    {
        var bot = (DiscordBotBase) Client;
        using var scope = bot.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

        if (db.ModApplications.AsNoTracking().FirstOrDefault(x => x.UserId == AuthorId.Value.RawValue) is { } existingThing)
            db.Update(App);
        else
            db.Add(App);

        await db.SaveChangesAsync();
    }
}