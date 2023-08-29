using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models
{
    public class ContestVote : IEntityTypeConfiguration<ContestVote>
    {
        public int Id { get; set; }

        public ulong UserId { get; set; }
        
        public int ContestId { get; set; }

        public int SubmissionId { get; set; }

        public void Configure(EntityTypeBuilder<ContestVote> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}