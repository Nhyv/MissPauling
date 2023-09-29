using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public enum EntryType
{
    Blacklisted,
    Accepted,
}

public class VerificationEntry : IEntityTypeConfiguration<VerificationEntry>
{
    public ulong SteamId { get; set; }

    public ulong UserId { get; set; }
    
    public EntryType? EntryType { get; set; }

    public string AdditionalComment { get; set; } = "No comments provided.";
    
    public ulong ModeratorId { get; set; }
    
    public void Configure(EntityTypeBuilder<VerificationEntry> builder)
    {
        builder.HasKey(x => x.SteamId);
    }
}

