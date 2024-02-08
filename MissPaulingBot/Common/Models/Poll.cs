using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public enum PollState
{
    Idle,
    Voting,
    Completed
}

public class Poll : IEntityTypeConfiguration<Poll>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Content { get; set; } = null!;

    public ulong ChannelId { get; set; }

    public ulong? MessageId { get; set; }

    public DateTimeOffset OpenPollAfter { get; set; }

    public DateTimeOffset RemoveVotingAfter { get; set; }

    public bool DisplayVotesPublicly { get; set; }

    public List<PollOption> Options { get; set; } = null!;

    public PollState State
    {
        get
        {
            var now = DateTimeOffset.UtcNow;

            if (now < OpenPollAfter)
                return PollState.Idle;

            if (now < RemoveVotingAfter)
                return PollState.Voting;

            return PollState.Completed;
        }
    }


    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}