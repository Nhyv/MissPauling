using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models;

public class StickyRole : IEntityTypeConfiguration<StickyRole>
{
    public ulong RoleId { get; set; }

    public string RoleName { get; set; }

    public List<StickyUser> StickyUsers { get; set; }

    public void Configure(EntityTypeBuilder<StickyRole> builder)
    {
        builder.HasKey(x => x.RoleId);
    }
}