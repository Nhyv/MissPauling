using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Gateway;
using Disqord.Rest;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Menus.Views;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Services;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Utility;

public class ButtonCommands : DiscordComponentGuildModuleBase
{
    private readonly PaulingDbContext _db;
    private readonly ModmailService _modmailService;
    private readonly ForumService _forumService;

    public ButtonCommands(PaulingDbContext db, ModmailService modmailService, ForumService forumService)
    {
        _db = db;
        _modmailService = modmailService;
        _forumService = forumService;
    }

    [ButtonCommand("VerificationButton_758829640885862431")]
    public async Task<IResult> VerifyAsync()
    {
        if (_db.ModmailVerifications.Find(Context.AuthorId.RawValue) is { } existingVerification)
            return Response("Your verification is still pending. Please wait.").AsEphemeral();

        if (DateTimeOffset.UtcNow - Context.Author.CreatedAt() > TimeSpan.FromDays(14) && !Context.Author.RoleIds.Contains(Constants.CO_OWNER_ROLE_ID))
        {
            await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, Context.AuthorId, Constants.CONTRACT_KILLER_ROLE_ID);
            return Response(
                    "You have agreed to the rules, welcome! Please do note agreeing to the rules completely invalidates using 'I did not know' as an excuse.")
                .AsEphemeral();
        }

        return View(new FailedVerificationView(Context.Interaction as IComponentInteraction));
    }

    [ButtonCommand(TradingGuidelinesService.TRADING_GUIDELINES_BUTTON_CUSTOM_ID)]
    public async Task<IResult> AcceptGuidelinesAsync()
    {
        if (Context.Author.RoleIds.Contains(Constants.TRADING_GUIDELINES_ROLE_ID))
        {
            return Response("You have already agreed to the trading guidelines!").AsEphemeral();
        }

        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, Context.AuthorId, Constants.TRADING_GUIDELINES_ROLE_ID);
        return Response("You have agreed to the trading guidelines.").AsEphemeral();
    }

    [ButtonCommand(ModmailService.CONTACT_BUTTON_CUSTOM_ID)]
    public async Task<IResult> ContactTheModsAsync()
    {
        if (await _db.BlacklistedUsers.FindAsync(Context.AuthorId.RawValue) is not null)
            return Response("You could not contact the modteam because you were blacklisted.").AsEphemeral();

        if (_modmailService.ActiveThreads.ContainsKey(Context.AuthorId))
            return Response("You cannot have 2 modmail threads open at once.").AsEphemeral();

        var modal = new LocalInteractionModalResponse().WithTitle("Modmail").WithCustomId("Modmail:Contact")
            .WithComponents(new LocalRowComponent().WithComponents(new LocalTextInputComponent().WithLabel("What are you contacting us about?")
                .WithStyle(TextInputComponentStyle.Paragraph).WithIsRequired().WithCustomId("details")
                .WithPlaceholder("Enter your answer here... REMINDERS: WE ARE NOT VALVE. DO NOT USE THIS TO GET TF2 HELP.")
                .WithMinimumInputLength(50)
                .WithMaximumInputLength(1500)));
        await Context.Interaction.Response().SendModalAsync(modal);
        return default;
    }

    [ModalCommand("Modmail:Contact")]
    public async Task<IResult> ContactModmailAsync(string details)
    {
        var contactChannel = (CachedTextChannel)Bot.GetChannel(Context.GuildId, Context.ChannelId);
        var createdThread = await Bot.CreatePrivateThreadAsync(contactChannel.Id, Guid.NewGuid().ToString("N"), x => x.AllowsInvitation = false);

        _modmailService.ActiveThreads.Add(Context.AuthorId, createdThread);

        await createdThread.SendMessageAsync(new LocalMessage()
            .WithContent(
                $"{Mention.Role(Constants.MODERATOR_ROLE_ID)}, {Mention.User(Context.Author)} has opened a modmail. Please let the modteam know what your question or your report is and they will respond" +
                $" to you as soon as possible! Use the 🔒 button below once your issue is resolved. Abuse of this system will result in a blacklist from it.")
            .WithAllowedMentions(new LocalAllowedMentions().WithRoleIds(Constants.MODERATOR_ROLE_ID)
                .WithUserIds(Context.AuthorId))
            .WithComponents(new LocalRowComponent()
                .AddComponent(new LocalButtonComponent()
                    .WithLabel("🔒")
                    .WithStyle(LocalButtonComponentStyle.Primary)
                    .WithCustomId("Modmail:Close"))));

        await createdThread.SendMessageAsync(
            new LocalMessage().WithContent($"User's contact reason: {details}"));

        return Response(
                $"{createdThread.Mention} was created for your request. Use it to contact the moderation team.")
            .AsEphemeral();
    }

    [ButtonCommand("Modmail:Close")]
    public async Task<IResult> CloseModmail()
    {
        if (DateTimeOffset.UtcNow - Context.ChannelId.CreatedAt < TimeSpan.FromMinutes(3) && !Context.Author.RoleIds.Contains(Constants.MODERATOR_ROLE_ID))
        {
            return Response("You cannot delete newly created threads.").AsEphemeral();
        }

        var thread = (IThreadChannel) Bot.GetChannel(Constants.TF2_GUILD_ID, Context.ChannelId);

        if (thread is null)
        {
            thread = await Bot.FetchChannelAsync(Context.ChannelId) as IThreadChannel;
        }

        await thread.SendMessageAsync(new LocalMessage().WithContent($"Closing per {Context.Author.Tag}'s request."));

        try
        {
            _modmailService.ActiveThreads.Remove(_modmailService.ActiveThreads.First(x => x.Value.Id == thread.Id).Key);
        }
        catch (InvalidOperationException ex)
        {
            await _modmailService.ResetThreadDictionaryAsync();
            _modmailService.ActiveThreads.Remove(_modmailService.ActiveThreads.First(x => x.Value.Id == thread.Id).Key);
        }

        var interaction = (IComponentInteraction)Context.Interaction;
        await interaction.Message.ModifyAsync(x => x.Components = new List<LocalRowComponent>());

        await thread.ModifyAsync(x =>
        {
            x.IsArchived = true;
            x.IsLocked = true;
        });

        return default;
    }

    [ButtonCommand("ArtistApplication:Approve:*")]
    public async Task<IResult> ApproveArtistApplication(Snowflake userId)
    {
        var member = await Bot.FetchMemberAsync(Constants.TF2_GUILD_ID, userId);
        var interaction = (IComponentInteraction)Context.Interaction;
        var application = _db.ArtistApplications.Find(userId.RawValue);

        if (member == null)
        {
            if (application != null)
            {
                _db.Remove(application);
                await _db.SaveChangesAsync();
            }

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = interaction.Message.Content +
                            $"\nUser left the server";
                x.Components = new List<LocalRowComponent>();
            });

            return default;
        }

        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, userId, Constants.ARTIST_ROLE_ID);

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nApproved by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        _db.Remove(application);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent("You were granted the artist role."));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user, but I granted the role.").AsEphemeral();
        }
    }

    [ButtonCommand("ArtistApplication:Deny:*")]
    public async Task<IResult> DenyArtistApplication(Snowflake userId) // TODO: add denied user to database
    {
        var member = await Bot.FetchMemberAsync(Constants.TF2_GUILD_ID, userId);
        var interaction = (IComponentInteraction)Context.Interaction;
        var application = _db.ArtistApplications.Find(userId.RawValue);

        if (member == null)
        {
            if (application != null)
            {
                _db.Remove(application);
                await _db.SaveChangesAsync();
            }

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = interaction.Message.Content +
                            $"\nUser left the server";
                x.Components = new List<LocalRowComponent>();
            });

            return default;
        }

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nDenied by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent("Hello, your artist application has been denied for one or many of the following reasons:\n\n" +
            "- Shitposts application\n- Low quality\n- Stolen content"));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user.").AsEphemeral();
        }
    }

    [ButtonCommand("StreamerApplication:Approve:*")]
    public async Task<IResult> ApproveStreamerApplication(Snowflake userId)
    {
        var member = await Bot.FetchMemberAsync(Constants.TF2_GUILD_ID, userId);
        var interaction = (IComponentInteraction)Context.Interaction;
        var application = _db.StreamerApplications.Find(userId.RawValue);

        if (member == null)
        {
            if (application != null)
            {
                _db.Remove(application);
                await _db.SaveChangesAsync();
            }

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = interaction.Message.Content +
                            $"\nUser left the server";
                x.Components = new List<LocalRowComponent>();
            });

            return default;
        }

        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, userId, Constants.STREAMER_ROLE_ID);


        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nApproved by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });
        _db.Remove(application);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "Hey there, you have been given the role Trusted Streamer. Please make sure to respect this small" +
                " set of rules:\n- During IRL streams, for the safety of all users involved, you are not allowed to" +
                " point your camera at yourself at any point. You may stream bread baking, pasta making or whatever" +
                " makes your heart happy that does not break our rules. We are aware you most likely don't have" +
                " any ill intents. However, in a 100k+ members server, you don't know what kind of people will be" +
                " looking at you and what they will be doing with what you are sharing from your webcam.\n- If" +
                " someone earrapes or is being a nuisance on your stream, you have permission to mute them. However," +
                " please make sure to tell us why you muted them.\n- If someone broke the rules in your channel and" +
                " made sure to leave, please let us know via this DM channel. We have logs and can catch them if you" +
                " don't remember their name.\n- If another streamer is already streaming in a channel, request for" +
                " permission to start yours, if you wish to be in the same channel as them.They might not want to share" +
                " the channel and potentially hold multiple conversations at once. There is a second channel available" +
                " just for that.They are rarely full but if it ever gets to a point where both channels are constantly" +
                " being used while you wish to stream, we will work something out.\n- No real life or realistic gore / NSFW content."));

            return default;
        }
        catch
        {
            return Response("I could not contact this user, but I granted the role.").AsEphemeral();
        }
    }

    [ButtonCommand("PollOption:*:*")]
    public async Task<IResult> VoteForPollOption(int pollId, int optionId)
    {
        if (_db.PollVotes.FirstOrDefault(x => x.PollId == pollId && x.VoterId == Context.AuthorId.RawValue) is
            { } existingVote)
            return Response("You already voted.").AsEphemeral();

        var option = _db.PollOptions.FindAsync(optionId).Result!;

        var vote = _db.PollVotes.Add(new PollVote
        {
            Option = option,
            OptionId = optionId,
            PollId = pollId,
            VoterId = Context.AuthorId.RawValue
        }).Entity;

        await _db.SaveChangesAsync();

        return Response("Thank you for voting!").AsEphemeral();
    }

    [ButtonCommand("StreamerApplication:Deny:*")]
    public async Task<IResult> DenyStreamerApplication(Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;
        var member = await Bot.FetchMemberAsync(Constants.TF2_GUILD_ID, userId);
        var application = _db.StreamerApplications.Find(userId.RawValue);

        if (member == null)
        {
            if (application != null)
            {
                _db.Remove(application);
                await _db.SaveChangesAsync();
            }

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = interaction.Message.Content +
                            $"\nUser left the server";
                x.Components = new List<LocalRowComponent>();
            });
            return default;
        }

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nDenied by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        _db.StreamerApplications.Remove(application);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "Hello, your streamer application has been denied for one or many of the following reasons:\n\nWe do not" +
                " accept applications for users with very little activity in the server. Without a proper history of" +
                " activity to look through to make sure an account is in good standing or behaves themselves, we don't" +
                " have any basis to go off of to allow them to stream.\n\nWe do not accept applications for users with" +
                " a recent punishment history.We need to be able to trust our streamers with the amount of moderation" +
                " permissions they are granted.\n\nWe hope you understand."));

            return default;
        }
        catch
        {
            return Response("I could not contact this user.").AsEphemeral();
        }
    }

    [ButtonCommand("Verification:Verify:*")]
    public async Task<IResult> VerifyUserAsync(Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nVerified by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, userId, Constants.CONTRACT_KILLER_ROLE_ID);

        var verification = await _db.ModmailVerifications.FindAsync(userId.RawValue);
        _db.ModmailVerifications.Remove(verification);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "You have been verified, welcome to the TF2 Community Discord server!"));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user, but I granted the role.").AsEphemeral();
        }
    }

    [ButtonCommand("Verification:Spectator:*")]
    public async Task<IResult> SpectatorUserAsync(Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nSpectator role granted by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        await Bot.GrantRoleAsync(Constants.TF2_GUILD_ID, userId, Constants.SPECTATOR_ROLE_ID);

        var verification = await _db.ModmailVerifications.FindAsync(userId.RawValue);
        _db.ModmailVerifications.Remove(verification);
        await _db.SaveChangesAsync();

        return default;
    }

    [ButtonCommand("Verification:NotLinked:*")]
    public async Task<IResult> SendNotLinkedMessageAsync(Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nMissing Steam link message sent by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        var verification = await _db.ModmailVerifications.FindAsync(userId.RawValue);
        _db.ModmailVerifications.Remove(verification);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "You have not followed the verification instructions, please link your Steam account to Discord to proceed with the verification process." +
                " Once you have done so please make sure to press the verification button again to send the verification request to the modteam." +
                " Below is a video tutorial on how to link your account: https://youtu.be/6qm6NoQeIvg"));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user.").AsEphemeral();
        }
    }

    [ButtonCommand("Verification:PrivateSteam:*")]
    public async Task<IResult> SendSteamPrivateMessageAsync(Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nPrivate Steam Message sent by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        var verification = await _db.ModmailVerifications.FindAsync(userId.RawValue);
        _db.ModmailVerifications.Remove(verification);
        await _db.SaveChangesAsync();

        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "You could not be verified because your Steam account is private. " +
                "Do note that Steam accounts are mostly private by default." +
                " Make sure to set Friends, Inventory, and Game Details to public. Need help on how to do this? " +
                "Return to the verification channel and follow the guides. Press the verify button again once those details are public."));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user.").AsEphemeral();
        }
    }
    // NEW SYSTEM
    [ButtonCommand("SteamSpy:Ban:*:*")]
    public async Task<IResult> BanUser(Snowflake steamId, Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        var modal = new LocalInteractionModalResponse().WithTitle("Extra Information").WithCustomId("SteamSpy:Comment")
            .WithComponents(new LocalRowComponent().WithComponents(new LocalTextInputComponent().WithLabel("Information about the case?")
                .WithStyle(TextInputComponentStyle.Paragraph).WithCustomId("details")
                .WithPlaceholder("Enter here why they were banned.")
                .WithMinimumInputLength(10)
                .WithMaximumInputLength(1500)));
        await Context.Interaction.Response().SendModalAsync(modal);
        
        _ = Task.Run(async () =>
        {
            var modalInteraction = await Bot.WaitForInteractionAsync<IModalSubmitInteraction>(Context.ChannelId, "SteamSpy:Comment");
            
            if (modalInteraction is null)
                return;

            var details = ((IRowComponent)modalInteraction.Components[1]).Components
                .OfType<ITextInputComponent>().Single(x => x.CustomId == "details").Value;
            
            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = interaction.Message.Content +
                            $"\nBanned by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
                x.Components = new List<LocalRowComponent>();
            });
        
            _db.VerificationEntries.Add(new VerificationEntry
            {
                SteamId = steamId,
                UserId = userId,
                EntryType = EntryType.Blacklisted,
                AdditionalComment = details,
                ModeratorId = Context.AuthorId
            });

            await _db.SaveChangesAsync();
            await Context.Bot.CreateBanAsync(Context.GuildId, userId, $"Banned during verification: {details}");
        });
        return default;
    }
    
    [ButtonCommand("SteamSpy:Verify:*:*")]
    public async Task<IResult> VerifyUser(Snowflake steamId, Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nVerified by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });
        
        _db.VerificationEntries.Add(new VerificationEntry
        {
            SteamId = steamId,
            UserId = userId,
            EntryType = EntryType.Accepted,
            ModeratorId = Context.AuthorId
        });

        await _db.SaveChangesAsync();
        await Bot.GrantRoleAsync(Context.GuildId, userId, Constants.CONTRACT_KILLER_ROLE_ID);
        
        try
        {
            var dmChannel = await Bot.CreateDirectChannelAsync(userId);
            await dmChannel.SendMessageAsync(new LocalMessage().WithContent(
                "You have been verified, welcome to the TF2 Community Discord server!"));

            return default;
        }
        catch 
        {
            return Response("I could not contact this user, but I granted the role.").AsEphemeral();
        }
    }
    
    [ButtonCommand("SteamSpy:Reject:*:*")]
    public async Task<IResult> RejectUser(Snowflake steamId, Snowflake userId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content +
                        $"\nRejected by {interaction.Author.Tag} (`{interaction.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });
        
        _db.VerificationEntries.Add(new VerificationEntry
        {
            SteamId = steamId,
            UserId = userId,
            EntryType = EntryType.Blacklisted,
            ModeratorId = Context.AuthorId,
            AdditionalComment = "Rejected"
        });
        await _db.SaveChangesAsync();
        
        return default;
    }

    [ButtonCommand("Forum:Close:*")]
    public async Task<IResult> CloseForumThreadAsync(Snowflake threadId)
    {
        var thread = Bot.GetChannel(Constants.TF2_GUILD_ID, threadId) as IThreadChannel;

        if (thread is null)
        {
            thread = await Bot.FetchChannelAsync(threadId) as IThreadChannel;
        }

        if (!Context.Author.RoleIds.Contains(Constants.MODERATOR_ROLE_ID) && Context.AuthorId != thread.CreatorId)
            return Response("This button may only be used by moderators or the OP.").AsEphemeral();

        var updatedTags = thread.TagIds.Append(thread.ChannelId == Constants.SUGGESTION_FEEDBACK_FORUM_ID ? (Snowflake)Constants.RESOLVED_SUGGESTION_TAG_ID : Constants.RESOLVED_HELP_TAG_ID).Distinct();

        if (Context.AuthorId == thread.CreatorId)
        {
            var view = new PromptView(x =>
                (x as LocalInteractionMessageResponse)!
                .WithContent(
                    "Are you sure you really wish to close this thread? You will no longer be able to receive replies in it.")
                .WithIsEphemeral());

            if (thread.Metadata.IsArchived)
            {
                await thread.ModifyAsync(x => x.IsArchived = false);
            }

            await View(view);

            if (!view.Result) 
                return default;

            await thread.SendMessageAsync(new LocalMessage().WithContent("Closed by the OP."));
            await thread.ModifyAsync(x => x.TagIds = updatedTags.ToList());
            await _forumService.CloseThreadAsync(thread);
            return default;
        }

        var modal = new LocalInteractionModalResponse().WithTitle("Closing Forum Post").WithCustomId("Forum:ModClose")
            .WithComponents(new LocalRowComponent().WithComponents(new LocalTextInputComponent().WithLabel("Why are you closing this post?")
                .WithStyle(TextInputComponentStyle.Paragraph).WithIsRequired().WithCustomId("reason")
                .WithPlaceholder("Reason for closing the forum post...")
                .WithMaximumInputLength(1500)));
        await Context.Interaction.Response().SendModalAsync(modal);
        return default;
    }

    [ModalCommand("Forum:ModClose")]
    public async Task<IResult> ModCloseForumAsync(string reason)
    {
        var thread = Bot.GetChannel(Constants.TF2_GUILD_ID, Context.ChannelId) as IThreadChannel;

        if (thread is null)
        {
            thread = await Bot.FetchChannelAsync(Context.ChannelId) as IThreadChannel;
        }

        var jumpLink = Discord.MessageJumpLink(Constants.TF2_GUILD_ID, thread.Id, thread.LastMessageId.Value);

        var updatedTags = thread.TagIds.Append(thread.ChannelId == Constants.SUGGESTION_FEEDBACK_FORUM_ID ? (Snowflake)Constants.RESOLVED_SUGGESTION_TAG_ID : Constants.RESOLVED_HELP_TAG_ID).Distinct();

        await thread.SendMessageAsync(new LocalMessage().WithContent($"This thread has been closed by a moderator.\nReason: {reason}"));
        await thread.ModifyAsync(x => x.TagIds = updatedTags.ToList());
        var wasSent = true;

        try
        {
            var dm = await Bot.CreateDirectChannelAsync(thread.CreatorId);
            await dm.SendMessageAsync(new LocalMessage().WithContent(
                $"Your thread **{thread.Name}** has been closed by a moderator.\nReason: {reason}\nYou may still view it by clicking on this link:\n{jumpLink}"));
        }
        catch
        {
            wasSent = false;
        }

        await Response(wasSent ? "You have closed this thread." : "You have closed this thread but I could not DM the user the reason.").AsEphemeral();
        await _forumService.CloseThreadAsync(thread);

        return default;
    }

    [ButtonCommand("Submission:Vote:*:*")]
    public async Task<IResult> VoteForSubmissionAsync(int contestId, int submissionId)
    {
        var submission = await _db.ContestSubmissions.FindAsync(submissionId);

        if (submission.CreatorId == Context.AuthorId.RawValue)
            return Response($"You cannot vote for your own submission!").AsEphemeral();

        if (_db.ContestVotes.FirstOrDefault(x => x.ContestId == contestId && x.UserId == Context.AuthorId.RawValue) is { } existingVote)
        {
            existingVote.SubmissionId = submissionId;
        }
        else
        {
            _db.ContestVotes.Add(new ContestVote
            {
                UserId = Context.AuthorId.RawValue,
                ContestId = contestId,
                SubmissionId = submissionId
            });
        }

        await _db.SaveChangesAsync();

        return Response($"You've successfully voted for #{submissionId}").AsEphemeral();
    }

    [ButtonCommand("Report:Handled:*")]
    public async Task<IResult> CompleteReportAsync(Snowflake reporterId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content + $"\nHandled by {Context.Author.Tag} (`{Context.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        return Response("Case handled!").AsEphemeral();
    }

    [ButtonCommand("Report:Invalid:*")]
    public async Task<IResult> MarkAsInvalidAsync(Snowflake reporterId)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = interaction.Message.Content + $"\nMarked as invalid by {Context.Author.Tag} (`{Context.AuthorId}`)";
            x.Components = new List<LocalRowComponent>();
        });

        try
        {
            var dm = await Bot.CreateDirectChannelAsync(reporterId);
            await dm.SendMessageAsync(new LocalMessage().WithContent("We either could not find anything wrong in your report or what you reported is simply not against our rules. If you believe that is wrong, you may open a modmail in #contact-the-mods."));
        }
        catch
        {
            return Response("Could not contact this user.").AsEphemeral();
        }

        return Response("I let the user know their report was invalid!").AsEphemeral();
    }

    [ModalCommand("Report:Message:*:*")]
    public async Task<IResult> HandleReportMessageAsync(Snowflake messageId, Snowflake channelId, string reason)
    {
        var message = await Bot.FetchMessageAsync(channelId, messageId) as IUserMessage;
        var imageUrls = string.Empty;

        if (message is null)
            return Response("I could not access this message data on Discord's end. This happens if the message is old or was deleted during your report.").AsEphemeral();

        if (message.Attachments.Count > 0)
        {
            foreach (var attachment in message.Attachments)
            {
                imageUrls = attachment.Url + "\n";
            }
        }

        var embed = EmbedUtilities.SuccessBuilder
            .WithTitle($"Message Report For {message.Author.Tag} ({message.Author.Id})").WithDescription(
                $"**Reported Content**\n\n**{message.Author.Tag} at {message.CreatedAt():g}**\n{message.Content}\n{imageUrls}\n**Report Reason:**\n{reason}\n\n[Jump To Message]({Discord.MessageJumpLink(Constants.TF2_GUILD_ID, channelId, messageId)})")
            .WithThumbnailUrl(message.Author.GetAvatarUrl())
            .WithFooter($"Reported by {Context.Author.Tag} ({Context.AuthorId})", Context.Author.GetAvatarUrl());

        await Bot.SendMessageAsync(Constants.MESSAGE_REPORT_CHANNEL_ID, new LocalMessage().WithEmbeds(embed).WithComponents(new LocalRowComponent().WithComponents(new LocalButtonComponent()
            .WithLabel("Handled").WithStyle(LocalButtonComponentStyle.Primary).WithCustomId($"Report:Handled:{Context.AuthorId}"), new LocalButtonComponent().WithLabel("Invalid Report").WithStyle(LocalButtonComponentStyle.Danger).WithCustomId($"Report:Invalid:{Context.AuthorId}"))));
        return Response("Report Sent!").AsEphemeral();
    }


    [ModalCommand("Report:User:*:*")]
    public async Task<IResult> HandleReportUserAsync(Snowflake userId, bool isMember, string reason)
    {
        var user = await Bot.FetchUserAsync(userId);

        var embed = EmbedUtilities.SuccessBuilder
            .WithTitle($"User Report For {user.Tag} ({user.Id})").WithDescription(
                $"**Report Reason:**\n{reason}").WithThumbnailUrl(user.GetAvatarUrl()).WithFooter($"Reported by {Context.Author.Tag} ({Context.AuthorId})", Context.Author.GetAvatarUrl());

        await Bot.SendMessageAsync(Constants.MESSAGE_REPORT_CHANNEL_ID, new LocalMessage().WithEmbeds(embed).WithComponents(new LocalRowComponent().WithComponents(new LocalButtonComponent().WithLabel("Handled").WithStyle(LocalButtonComponentStyle.Primary).WithCustomId($"Report:Handled:{Context.AuthorId}"), new LocalButtonComponent().WithLabel("Invalid Report").WithStyle(LocalButtonComponentStyle.Danger).WithCustomId($"Report:Invalid:{Context.AuthorId}"))));
        return Response("Report Sent!").AsEphemeral();
    }

    [ButtonCommand("Suggestion:Vote:*:*")]
    public async Task<IResult> SuggestionVoteAsync(int suggestionId, bool isUpvote)
    {
        var suggestion = await _db.Suggestions.FindAsync(suggestionId);
        var thanks = "Thanks for voting";

        if (suggestion.UpvoteUsers.Contains(Context.AuthorId.RawValue))
        {
            if (isUpvote)
                return Response("You already upvoted this suggestion!").AsEphemeral();

            suggestion.UpvoteUsers.Remove(Context.AuthorId.RawValue);
            suggestion.DownvoteUsers.Add(Context.AuthorId.RawValue);
            await _db.SaveChangesAsync();

            return Response(thanks).AsEphemeral();
        }

        if (suggestion.DownvoteUsers.Contains(Context.AuthorId.RawValue))
        {
            if (!isUpvote)
                return Response("You already downvoted this suggestion!").AsEphemeral();

            suggestion.DownvoteUsers.Remove(Context.AuthorId.RawValue);
            suggestion.UpvoteUsers.Add(Context.AuthorId.RawValue);
            await _db.SaveChangesAsync();

            return Response(thanks).AsEphemeral();
        }

        if (isUpvote)
            suggestion.UpvoteUsers.Add(Context.AuthorId.RawValue);
        else
            suggestion.DownvoteUsers.Add(Context.AuthorId.RawValue);

        await _db.SaveChangesAsync();
        return Response(thanks).AsEphemeral();
    }
}