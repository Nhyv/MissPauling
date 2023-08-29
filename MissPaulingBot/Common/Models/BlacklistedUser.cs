using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models
{
    public class BlacklistedUser : IEntityTypeConfiguration<BlacklistedUser>
    {
        public ulong UserId { get; set; }

        public string Username { get; set; }

        public ulong ModeratorId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public void Configure(EntityTypeBuilder<BlacklistedUser> builder)
        {
            builder.HasKey(x => x.UserId);
        }
    }
}