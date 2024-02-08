using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class SaxtonOwnRole : IEntityTypeConfiguration<SaxtonOwnRole>
{
    public ulong OwnerId { get; set; }

    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Color { get; set; }

    public string Extension { get; set; } = null!;

    public MemoryStream Data { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Configure(EntityTypeBuilder<SaxtonOwnRole> builder)
    {
        builder.HasKey(x => x.OwnerId);
        builder.Property(x => x.Data).HasConversion(x => x.ToArray(), x => new MemoryStream(x));
    }
}