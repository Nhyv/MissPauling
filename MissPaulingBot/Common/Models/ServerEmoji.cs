using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ServerEmoji : IEntityTypeConfiguration<ServerEmoji>
{
    public ulong EmojiId { get; set; }

    public int Usage { get; set; } = 0;

    public void Configure(EntityTypeBuilder<ServerEmoji> builder)
    {
        builder.HasKey(x => x.EmojiId);
    }
}