using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using MissPaulingBot.Common.Models;

namespace MissPaulingBot.Common.Menus.Views
{
    public class FailedVerificationView : ViewBase
    {
        private readonly IComponentInteraction _interaction;

        public FailedVerificationView(IComponentInteraction interaction)
            : base(BuildTemplateMessage())
        {
            _interaction = interaction;
        }

        [Button(Label = "My Steam Account Is Linked", Style = LocalButtonComponentStyle.Success)]
        public async ValueTask LinkedSteamAsync(ButtonEventArgs b)
        {
            var bot = (DiscordBotBase)Menu.Client;
            using var scope = bot.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<PaulingDbContext>();

            var verification = db.ModmailVerifications.Add(new ModmailVerification
            {
                UserId = b.AuthorId.RawValue
            }).Entity;

            await db.SaveChangesAsync();

            var message = await Menu.Client.SendMessageAsync(Constants.MODMAIL_CHANNEL_ID, new LocalMessage()
                .WithContent(
                    $"{b.Interaction.Author.Mention} (`{b.Interaction.Author.Id}`) -" +
                    " I have linked my Steam account and am ready for verification.")
                .WithComponents(new LocalRowComponent()
                    .WithComponents(new LocalButtonComponent()
                        .WithStyle(LocalButtonComponentStyle.Success)
                        .WithLabel("Verify")
                        .WithCustomId($"Verification:Verify:{b.Interaction.AuthorId}"), 
                        new LocalButtonComponent()
                            .WithStyle(LocalButtonComponentStyle.Danger)
                            .WithLabel("Spectator")
                            .WithCustomId($"Verification:Spectator:{b.Interaction.AuthorId}"), 
                        new LocalButtonComponent()
                            .WithStyle(LocalButtonComponentStyle.Secondary)
                            .WithLabel("Steam Not Linked")
                            .WithCustomId($"Verification:NotLinked:{b.Interaction.AuthorId}"),
                        new LocalButtonComponent()
                            .WithStyle(LocalButtonComponentStyle.Secondary)
                            .WithLabel("Private Steam")
                            .WithCustomId($"Verification:PrivateSteam:{b.Interaction.AuthorId}"))));

            verification.MessageId = message.Id.RawValue;
            await db.SaveChangesAsync();

            MessageTemplate = m =>
                m.WithContent("The modteam has been contacted. Please do note that it might take some time " +
                              "for your account to be verified, please be patient.");
            ClearComponents();
        }
        
        private static Action<LocalMessageBase> BuildTemplateMessage()
        {
            return x => (x as LocalInteractionMessageResponse).WithContent(
                "Hello! Your account has been detected to be a new account by our automated systems.\n" +
                "As a counter-measure to deter raid bots, spam accounts, and punishment evasion, we have " +
                "prevented new accounts from accessing the server immediately.\n" +
                "If you believe this is an error, or you would like to request manual verification that you are " +
                "indeed *not* a bot, please link your steam account and set privacy settings to public for " +
                "Game Details, Friends List and Inventory then press the button below. (Need help switching to " +
                "public? See here: " +
                "<https://cdn.discordapp.com/attachments/416139786961813514/896423657197408276/OSZrmwqfXo.gif>)" +
                " (Why do we need that data?" + " We have had tons of people join to try and scam others over the" +
                " past months and an account with" +
                " no playtime, a private inventory to hide stolen items or no friends are usually indicators" +
                " something is off. Of course, once you are verified, you can change your settings back to private.)" +
                " The modteam will be happy to verify your account and allow you in as soon as " +
                "possible. Don't know how to link your Steam account? Check out this tutorial: " +
                "<https://youtu.be/6qm6NoQeIvg>.").WithIsEphemeral();
        }
        
    }
}