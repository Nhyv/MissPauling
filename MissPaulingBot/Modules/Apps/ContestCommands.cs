using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Checks;
using MissPaulingBot.Common.Models;
using MissPaulingBot.Extensions;
using MissPaulingBot.Utilities;
using Qmmands;

namespace MissPaulingBot.Modules.Moderation
{
    [SlashGroup("contest")]
    [Description("Contest commands.")]
    public class ContestCommands : DiscordApplicationModuleBase
    {
        private readonly PaulingDbContext _db;
        private readonly HttpClient _http;

        public ContestCommands(PaulingDbContext db, HttpClient http)
        {
            _db = db;
            _http = http;
        }

        [SlashCommand("submit")]
        [RequireAuthorRole(Constants.ARTIST_ROLE_ID)]
        [Description("Submits an icon or a banner during our contests.")]
        public async Task<IResult> SubmitAsync([Description("The contest you are submitting to.")] int contestChoice, [SupportedFileExtensions("png")][Description("Your artwork must be a PNG.")] IAttachment attachment)
        {
            var contest = await _db.Contests.FindAsync(contestChoice);
            if (contest is null || contest.State != ContestState.Submissions)
                return Response("Invalid contest, or the contest isn't ready for submissions.").AsEphemeral();

            if (_db.ContestSubmissions.FirstOrDefault(x => x.ContestId == contestChoice && x.CreatorId == Context.AuthorId.RawValue) is {} existingSubmission)
                return Response("You have already submitted.").AsEphemeral();

            var data = new MemoryStream(); 
            await using var stream = await _http.GetStreamAsync(attachment.Url); 
            await stream.CopyToAsync(data); 

            var submission = _db.Add(new ContestSubmission
            {
                CreatorId = Context.Author.Id,
                ContestId = contestChoice,
                Data = data,
                Extension = Path.GetExtension(new Uri(attachment.Url).AbsolutePath)[1..].ToLower() 

            }).Entity;

            await _db.SaveChangesAsync();

            data.Seek(0, SeekOrigin.Begin);
                
            var message = await Context.Bot.SendMessageAsync(contest.ChannelId,
               new LocalMessage().AddAttachment(new LocalAttachment(data,
                   $"submission_{submission.Id}.{submission.Extension}")).WithContent($"**Submission by {submission.CreatorId}**\nSpam or Troll submission? Use /contest deny {submission.Id}"));

            submission.MessageId = message.Id;

            await _db.SaveChangesAsync();

            return Response($"Your submission was successfully submitted. Look out for announcement posts as we will send a notification as soon as voting starts.").AsEphemeral();
        }

        [SlashCommand("deny")]
        [RequireAuthor(290273013511749634, Group = "Nyoom or Mod")] // Temporary setup for Nyoom's art project
        [RequireAuthorRole(Constants.MODERATOR_ROLE_ID, Group = "Nyoom or Mod")]
        [Description("Denies an icon or a banner by id.")]
        public async Task<IResult> ApproveSubmissionAsync([Description("The contest you are submitting to.")] int contestChoice, [Description("The submission ID.")] int submissionId)
        {
            var contest = await _db.Contests.FindAsync(contestChoice);

            if (contest is null)
                return Response("Invalid contest.").AsEphemeral();

            if (await _db.ContestSubmissions.FindAsync(submissionId) is not { } submission)
                return Response("No submission could be found with that ID.").AsEphemeral();

            var messageDeleted = false;

            try
            {
                await Context.Bot.DeleteMessageAsync(contest.ChannelId, submission.MessageId);
                messageDeleted = true;
            }
            catch { }
            
            _db.ContestSubmissions.Remove(submission);
            await _db.SaveChangesAsync();

            return Response(messageDeleted
                ? $"Submission #{submissionId} was successfully deleted."
                : $"Submission #{submissionId} was successfully deleted. However, the message attached to it was not able to be deleted.");
        }

        [SlashCommand("create")]
        [RequireGuild]
        [Description("Creates a contest")]
        [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
        public async Task<IResult> CreateContestAsync([Description("Contest name")] string name, [Description("The channel")][ChannelTypes(ChannelType.PublicThread)] IChannel channel,
            [Description("Time in Unix")] long allowSubmissionsAfter,
            [Description("Time in Unix")] long allowSubmissionsUntil,
            [Description("Time in Unix")] long allowVotingAfter,
            [Description("Time in Unix")] long allowVotingUntil,
            [Description("Time in Unix")] long allowResultsViewingAfter,
            [Description("Time in Unix")] long allowResultsViewingUntil)
        {
            var contests = _db.Contests;

            if (contests.Any(x => x.Name == name))
                return Response(
                    "There is a contest named exactly the same. For search and viewing purposes, be more original.");

            _db.Contests.Add(new Contest
            {
                Name = name,
                AllowSubmissionsAfter = DateTimeOffset.FromUnixTimeSeconds(allowSubmissionsAfter),
                AllowSubmissionsUntil = DateTimeOffset.FromUnixTimeSeconds(allowSubmissionsUntil),
                AllowVotingAfter = DateTimeOffset.FromUnixTimeSeconds(allowVotingAfter),
                AllowVotingUntil = DateTimeOffset.FromUnixTimeSeconds(allowVotingUntil),
                AllowResultsViewingAfter = DateTimeOffset.FromUnixTimeSeconds(allowResultsViewingAfter),
                AllowResultsViewingUntil = DateTimeOffset.FromUnixTimeSeconds(allowResultsViewingUntil),
                ChannelId = channel.Id.RawValue
            });

            await _db.SaveChangesAsync();

            return Response("Contest created.");
        }

        [SlashCommand("results")]
        [RequireGuild]
        [Description("Allows users to view the contest results.")]
        public async Task<IResult> ViewIconResultsAsync([Description("The contest you want to see")] int contestResultsChoice)
        {
            var contest = await _db.Contests.FindAsync(contestResultsChoice);

            if (contest is null || contest.State != ContestState.Results)
                return Response("Invalid contest, or the contest is already completed.").AsEphemeral();

            var allSubmissions = await _db.ContestSubmissions.Where(x => x.ContestId == contest.Id).ToListAsync();
            var allVotes = await _db.ContestVotes.Where(x => x.ContestId == contestResultsChoice).ToListAsync();
            var pages = new List<Page>();

            var results = new List<(int SubmissionId, int NumberOfVotes)>();
            foreach (var submission in allSubmissions)
            {
                results.Add((submission.Id, allVotes.Count(x => x.SubmissionId == submission.Id)));
            }

            foreach (var result in results.OrderByDescending(x => x.NumberOfVotes))
            {
                var submission = allSubmissions.First(x => x.Id == result.SubmissionId);
                var author = await Context.Bot.GetOrFetchUserAsync(submission.CreatorId);

                /*if ((Context.Author as IMember).RoleIds.Contains(Constants.MODERATOR_ROLE_ID))
                {
                    pages.Add(new Page().WithEmbeds(EmbedUtilities.SuccessBuilder.WithTitle($"Results")
                        .WithDescription($"{allVotes.Count(x => x.SubmissionId == submission.Id)} votes").WithAuthor($"Author: {author}", author.GetAvatarUrl()).WithImageUrl(submission.CreationUrl)));
                    // TODO: Add media
                    continue;
                }


                pages.Add(new Page().WithEmbeds(EmbedUtilities.SuccessBuilder.WithTitle($"Results")
                    .WithAuthor($"Author: {author}", author.GetAvatarUrl()).WithImageUrl(submission.CreationUrl)));*/
                // TODO: Add media
            }

            return Pages(pages);
        }

        [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
        [SlashCommand("status")]
        [Description("View a contest's status.")]
        public async Task<IResult> ViewContestSettings([Description("The contest")] int contestChoice)
        {
            var contest = await _db.Contests.FindAsync(contestChoice);

            if (contest is null)
                return Response("Invalid contest.").AsEphemeral();

            var embed = EmbedUtilities.SuccessBuilder.WithTitle($"{contest.Name} status")
                .AddField("**State**", contest.State)
                .AddField("**You can submit after**",
                    Markdown.Timestamp(contest.AllowSubmissionsAfter, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowSubmissionsAfter, Markdown.TimestampFormat.RelativeTime))
                .AddField("**Submissions will end on**",
                    Markdown.Timestamp(contest.AllowSubmissionsUntil, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowSubmissionsUntil, Markdown.TimestampFormat.RelativeTime))
                .AddField("**Voting will be available after**",
                    Markdown.Timestamp(contest.AllowVotingAfter, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowVotingAfter, Markdown.TimestampFormat.RelativeTime))
                .AddField("**Voting will end on**",
                    Markdown.Timestamp(contest.AllowVotingUntil, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowVotingUntil, Markdown.TimestampFormat.RelativeTime))
                .AddField("**You can view results after**",
                    Markdown.Timestamp(contest.AllowResultsViewingAfter, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowResultsViewingAfter, Markdown.TimestampFormat.RelativeTime))
                .AddField("**You can view results until**",
                    Markdown.Timestamp(contest.AllowResultsViewingUntil, Markdown.TimestampFormat.LongDateTime) + "\n" +
                    Markdown.Timestamp(contest.AllowResultsViewingUntil, Markdown.TimestampFormat.RelativeTime));

            return Response(embed);
        }

        [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
        [SlashCommand("obliterate")]
        [Description("OBLITERATES one contest. One.")]
        public async Task<IResult> ObliterateContest([Description("The contest")] int contestChoice)
        {
            var contest = await _db.Contests.FindAsync(contestChoice);

            if (contest is null)
                return Response("Invalid contest.").AsEphemeral();

            _db.Remove(contest);
            await _db.SaveChangesAsync();
            return Response("Obliterated. Nuked. Turned to dust. Dust is gone. Farewell.");
        }
        // TODO: Add a way to modify the datetimeoffsets

        [AutoComplete("submit")]
        [AutoComplete("deny")]
        public async Task AutoCompleteContestChoiceAsync([Name("contest-choice")]AutoComplete<int> contestChoice)
        {
            var contestChoices = await _db.Contests.ToListAsync();

            contestChoice.Choices.AddRange(contestChoices.Where(x => x.State == ContestState.Submissions).Take(25).ToDictionary(x => x.Name, x => x.Id));
        }

        [AutoComplete("status")]
        [AutoComplete("obliterate")]
        public async Task AutoCompleteContestSettingsAsync([Name("contest-choice")] AutoComplete<int> contestChoice)
        {
            var contestChoices = await _db.Contests.ToListAsync();

            contestChoice.Choices.AddRange(contestChoices.Take(25).ToDictionary(x => x.Name, x => x.Id));
        }

        [AutoComplete("results")]
        public async Task AutoCompleteContestResultsAsync([Name("contest-results-choice")] AutoComplete<int> contestResultsChoice)
        {
            var contestResultsChoices = await _db.Contests.ToListAsync();

            contestResultsChoice.Choices.AddRange(contestResultsChoices.Where(x => x.State == ContestState.Results)
                .Take(25).ToDictionary(x => x.Name, x => x.Id));
        }
    }
}