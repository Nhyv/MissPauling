
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ReportedMessage : IEntityTypeConfiguration<ReportedMessage>
{
    public ulong MessageId { get; set; }
    
    public void Configure(EntityTypeBuilder<ReportedMessage> builder)
    {
        builder.HasKey(x => x.MessageId);
    }
}