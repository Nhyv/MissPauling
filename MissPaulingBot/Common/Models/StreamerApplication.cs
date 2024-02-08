using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class StreamerApplication : IEntityTypeConfiguration<StreamerApplication>
{
    public ulong UserId { get; set; }

    public string ContentAnswer { get; set; } = null!;

    public string ReasonAnswer { get; set; } = null!;
    
    public string PlatformAnswer { get; set; } = null!;

    public void Configure(EntityTypeBuilder<StreamerApplication> builder)
    {
        builder.HasKey(x => x.UserId);
    }
}