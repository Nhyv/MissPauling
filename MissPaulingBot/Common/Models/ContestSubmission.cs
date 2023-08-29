using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models
{
    public class ContestSubmission : IEntityTypeConfiguration<ContestSubmission>
    {
        public int Id { get; set; }
        
        public ulong CreatorId { get; set; }

        public int ContestId { get; set; }
        
        public ulong MessageId { get; set; }
        
        public string Extension { get; set; }

        public MemoryStream Data { get; set; }

        public void Configure(EntityTypeBuilder<ContestSubmission> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Data).HasConversion(x => x.ToArray(), x => new MemoryStream(x));
        }
    }
}