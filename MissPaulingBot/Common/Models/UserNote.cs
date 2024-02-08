using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class UserNote : IEntityTypeConfiguration<UserNote>
{
    public int Id { get; set; }

    public ulong UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Note { get; set; } = null!;

    public ulong ModeratorId { get; set; }

    public DateTimeOffset GivenAt { get; set; } = DateTimeOffset.UtcNow;

    public void Configure(EntityTypeBuilder<UserNote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}