using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class DmMessageTemplate : IEntityTypeConfiguration<DmMessageTemplate>
{
    public string Name { get; set; } = null!;

    public string Response { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ulong AuthorId { get; set; }

    public override string ToString()
    {
        return $"{Name} - {Response}";
    }

    public void Configure(EntityTypeBuilder<DmMessageTemplate> builder)
    {
        builder.HasKey(x => x.Name);
    }
}