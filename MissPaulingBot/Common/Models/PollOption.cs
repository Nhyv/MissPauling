using System.Collections.Generic;
using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class PollOption : IEntityTypeConfiguration<PollOption>
{
    public int Id { get; set; }

    public int PollId { get; set; }

    public string Content { get; set; }

    public Poll Poll { get; set; }

    public List<PollVote> Votes { get; set; }

    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasOne(x => x.Poll).WithMany(x => x.Options).OnDelete(DeleteBehavior.Cascade);
    }

    public LocalComponent ToComponent()
    {
        return new LocalButtonComponent().WithLabel(Content).WithCustomId($"PollOption:{PollId}:{Id}");
    }
}