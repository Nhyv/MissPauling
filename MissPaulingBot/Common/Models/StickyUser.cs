using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class StickyUser : IEntityTypeConfiguration<StickyUser>
{
    public ulong UserId { get; set; }

    public List<StickyRole> StickyRoles { get; set; } = null!;

    public void Configure(EntityTypeBuilder<StickyUser> builder)
    {
        builder.HasKey(t => t.UserId);
        builder.HasMany(x => x.StickyRoles).WithMany(x => x.StickyUsers);
    }
}