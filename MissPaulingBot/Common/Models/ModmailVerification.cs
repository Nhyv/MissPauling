using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ModmailVerification : IEntityTypeConfiguration<ModmailVerification>
{
    public ulong UserId { get; set; }

    public ulong MessageId { get; set; }

    public void Configure(EntityTypeBuilder<ModmailVerification> builder)
    {
        builder.HasKey(x => x.UserId);
    }
}