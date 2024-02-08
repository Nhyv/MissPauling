using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class PollVote : IEntityTypeConfiguration<PollVote>
{
    public int Id { get; set; }

    public int OptionId { get; set; }

    public int PollId { get; set; }

    public ulong VoterId { get; set; }

    public PollOption Option { get; set; } = null!;

    public void Configure(EntityTypeBuilder<PollVote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasOne(x => x.Option).WithMany(x => x.Votes).OnDelete(DeleteBehavior.Cascade);
    }
}