using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class Suggestion : IEntityTypeConfiguration<Suggestion>
{
    public int Id { get; set; }

    public ulong AuthorId { get; set; }

    public ulong MessageId { get; set; }

    public string Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Extension { get; set; }

    public MemoryStream Data { get; set; }

    public List<ulong> UpvoteUsers { get; set; } = new();

    public List<ulong> DownvoteUsers { get; set; } = new();

    public ulong ThreadId { get; set; }

    public bool IsCompleted { get; set; }
    
    public void Configure(EntityTypeBuilder<Suggestion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Data).HasConversion(x => x.ToArray(), x => new MemoryStream(x));
    }
}