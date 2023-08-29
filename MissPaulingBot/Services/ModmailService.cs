using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;

namespace MissPaulingBot.Services;

public class ModmailService : DiscordBotService
{
    public const string CONTACT_BUTTON_CUSTOM_ID = "ContactModmailButton_1022177111886286920";
    public Dictionary<Snowflake, IThreadChannel> ActiveThreads { get; private set; } = new ();
    private bool _firstRun = true;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_firstRun)
        {
            await ResetThreadDictionaryAsync();
        }
        else
        {
            _firstRun = false;
        }

        Logger.LogInformation($"Discovered {ActiveThreads.Count} threads. User Id is {String.Join(", ", ActiveThreads.Keys)}");

        var messages = await Bot.FetchMessagesAsync(Constants.CONTACT_THE_MODS_CHANNEL_ID);

        var contactMessage = messages.OfType<IUserMessage>().FirstOrDefault(x =>
        {
            foreach (var row in x.Components)

            foreach (var component in row.Components.OfType<IButtonComponent>())
            {
                if (component.CustomId == CONTACT_BUTTON_CUSTOM_ID)
                {
                    return true;
                }
            }

            return false;
        });

        if (contactMessage is null)
        {
            await Bot.SendMessageAsync(Constants.CONTACT_THE_MODS_CHANNEL_ID,
                new LocalMessage().WithContent(
                        "To message our moderators regarding a question or a report related to this server, please press the button below.")
                    .WithComponents(new LocalRowComponent().AddComponent(new LocalButtonComponent()
                        .WithCustomId(CONTACT_BUTTON_CUSTOM_ID).WithLabel("Open a modmail")
                        .WithStyle(LocalButtonComponentStyle.Primary))));
        }
    }

    public async Task ResetThreadDictionaryAsync()
    {
        var threads = await Bot.FetchActiveThreadsAsync(Constants.TF2_GUILD_ID);
        var modThreads = threads.Where(x => x.ChannelId == Constants.CONTACT_THE_MODS_CHANNEL_ID);

        ActiveThreads = new Dictionary<Snowflake, IThreadChannel>();

        foreach (var modThread in modThreads)
        {
            var threadMessages = await modThread.FetchMessagesAsync(direction: FetchDirection.After, startFromId: modThread.Id);
            var orderedMessages = threadMessages.OrderBy(x => x.Id).ToList();
            var users = Mention.ParseUsers(orderedMessages[0].Content);

            ActiveThreads.Add(users.ElementAt(0).RawValue, modThread);
        }
    }
}