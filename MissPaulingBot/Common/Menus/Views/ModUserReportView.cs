using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Utilities;

namespace MissPaulingBot.Common.Menus.Views
{
    public class ModUserReportView : ViewBase
    {
        private readonly IUser _reported;
        private readonly IUser _puppyHater;
        private readonly string _reportCategory;
        private readonly IGuildChannel _channel;

        public ModUserReportView(IUser puppyHater, IUser reported, IGuildChannel channel,
            string reportCategory) : base(BuildTemplateMessage(reported, puppyHater, channel, reportCategory))
        {
            _puppyHater = puppyHater;
            _reported = reported;
            _reportCategory = reportCategory;
            _channel = channel;
        }

        public DiscordBotBase Bot => (DiscordBotBase)Menu.Client;
        
        [Button(Label = "Action taken", Style = LocalButtonComponentStyle.Success)]
        public async ValueTask ActionTakenAsync(ButtonEventArgs b)
        {
            using var scope = Bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();
            
            try
            {
                await _puppyHater.SendMessageAsync(new LocalMessage().WithContent(
                    $"Hello, this is a message from the TF2 Community modteam as a response to your latest report." +
                    $" Thank you for your report, action has been taken against the reported user."));
                
                MessageTemplate = m => m.Embeds.Value[0].WithColor(16711680).WithFooter("Action handled.");
                //TemplateMessage.Embeds[0].WithColor(16711680).WithFooter("Action handled.");
                ClearComponents(); 
            }
            catch
            {
                MessageTemplate = m => m.Embeds.Value[0].WithColor(16711680).WithFooter("Action handled.");
                //TemplateMessage.Embeds[0].WithColor(16711680).WithFooter("Reporter could not be contacted.");
                ClearComponents(); 
            }
        }
        
        [Button(Label = "Follow-Up", Style = LocalButtonComponentStyle.Primary)]
        public async ValueTask FollowUpAsync(ButtonEventArgs b)
        {
            try
            {
                await _puppyHater.SendMessageAsync(new LocalMessage().WithContent("Hello, you are being contacted by the TF2 Community" +
                    $" Moderator team because of your recent report for user {_reported.Tag} under category {_reportCategory}. We are asking" +
                    $" of you to provide a detailed explanation as to why you are reporting this user so we can process your report as effectively" +
                    $" as possible. You can send this explanation by replying to this DM channel. Thank you."));
                
                MessageTemplate = m => m.Embeds.Value[0].WithColor(16711680).WithFooter("Action handled.");
                //TemplateMessage.Embeds[0].WithColor(9498256).WithFooter("User has been contacted.");
                ClearComponents(); 
            }
            catch
            {
                await Bot.SendMessageAsync(b.ChannelId, new LocalMessage().WithContent(
                    "This user either blocked me, has their DMs set to Friends Only, or no longer shares a server with me."));

                MessageTemplate = m => m.Embeds.Value[0].WithColor(9498256).WithFooter("User could not contacted");
                ClearComponents(); 
            }
        }
        
        private static Action<LocalMessageBase> BuildTemplateMessage(IUser reported, IUser puppyHater, IGuildChannel channel, string reportCategory)
        {
            var embed = EmbedUtilities.LoggingBuilder.WithTitle($"New user report")
                .WithDescription($"{reported.Tag} (`{reported.Id}`)")
                .AddField("Report Category", reportCategory)
                .AddField("Channel", channel.Mention)
                .WithFooter($"Reported by {puppyHater.Tag} ({puppyHater.Id})", puppyHater.GetAvatarUrl());
            return x => x.WithEmbeds(embed);
        }
    }
}