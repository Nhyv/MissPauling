using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class ToDo : IEntityTypeConfiguration<ToDo>
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public ulong ModeratorId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Configure(EntityTypeBuilder<ToDo> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}