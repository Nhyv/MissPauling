using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Extensions;

namespace MissPaulingBot.Common.Menus.Views
{
    public class UserReportView : ViewBase
    {
        private readonly Snowflake _userId;
        private readonly Snowflake _channelId;
        private readonly Snowflake _guildId;
        private readonly IUser _puppyHater;
        private readonly IContextMenuInteraction _interaction;
        public UserReportView(IContextMenuInteraction interaction, Action<LocalMessageBase> templateMessage) 
            : base(templateMessage)
        {
            _userId = interaction.TargetId;
            _channelId = interaction.ChannelId;
            _guildId = interaction.GuildId!.Value;
            _puppyHater = interaction.Author;
            _interaction = interaction;
        }
        
        [Selection(Placeholder = "Select a category...")]
        [SelectionOption("Foreign language")]
        [SelectionOption("DM harassment")]
        [SelectionOption("Impersonation")]
        [SelectionOption("Alt account")]
        [SelectionOption("Inappropriate avatar/nickname/status")]
        public async ValueTask ReportAsync(SelectionEventArgs e)
        {
            var bot = (DiscordBotBase)Menu.Client;
            var target = await bot.GetOrFetchUserAsync(_userId);
            var channel = bot.GetChannel(_guildId, _channelId) as IGuildChannel;
            using var scope = bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

            if (db.BlacklistedUsers.Find(_puppyHater.Id.RawValue) is null)
            {
                await bot.StartMenuAsync(Constants.REPORT_CHANNEL_ID,
                    new DefaultTextMenu(new ModUserReportView(_puppyHater, target, channel, e.SelectedOptions[0].Value.ToString())), TimeSpan.FromHours(24));
            }
            
            await _interaction.Followup().ModifyResponseAsync(x =>
            {
                x.Content = "Thank you for the report.";
                x.Components = Array.Empty<LocalRowComponent>();
            });
            Menu.Stop();
        }
    }
}