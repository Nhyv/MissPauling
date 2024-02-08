using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ModerationChannel : IEntityTypeConfiguration<ModerationChannel>
{
    public ulong ChannelId { get; set; }
    
    public void Configure(EntityTypeBuilder<ModerationChannel> builder)
    {
        builder.HasKey(x => x.ChannelId);
    }
}