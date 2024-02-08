using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public enum ContestState
{
    Idle,
    Submissions,
    Voting,
    Results,
    Completed,
}

public class Contest : IEntityTypeConfiguration<Contest>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong ChannelId { get; set; }

    public DateTimeOffset AllowSubmissionsAfter { get; set; }

    public DateTimeOffset AllowSubmissionsUntil { get; set; }

    public DateTimeOffset AllowVotingAfter { get; set; }

    public DateTimeOffset AllowVotingUntil { get; set; }

    public DateTimeOffset AllowResultsViewingAfter { get; set; }

    public DateTimeOffset AllowResultsViewingUntil { get; set; }

    public List<string> AcceptedFileTypes { get; set; } = null!;

    public ContestState State
    {
        get
        {
            var now = DateTimeOffset.UtcNow;

            if (now < AllowSubmissionsAfter)
                return ContestState.Idle;

            if (now < AllowSubmissionsUntil)
                return ContestState.Submissions;

            if (now < AllowVotingAfter)
                return ContestState.Idle;

            if (now < AllowVotingUntil)
                return ContestState.Voting;

            if (now < AllowResultsViewingAfter)
                return ContestState.Idle;

            return now < AllowResultsViewingUntil ? ContestState.Results : ContestState.Completed;
        }
    }

    public void Configure(EntityTypeBuilder<Contest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}