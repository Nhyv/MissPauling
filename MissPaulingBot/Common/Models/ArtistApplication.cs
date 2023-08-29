using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ArtistApplication : IEntityTypeConfiguration<ArtistApplication>
{
    public ulong UserId { get; set; }

    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;

    public string UploadUrl { get; set; }

    public void Configure(EntityTypeBuilder<ArtistApplication> builder)
    {
        builder.HasKey(x => x.UserId);
    }
}