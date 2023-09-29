using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MissPaulingBot.Common.Models;

namespace MissPaulingBot.Common
{
    public class PaulingDbContext : DbContext
    {
        public PaulingDbContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables("PAULING_")
                    .Build();

                optionsBuilder.UseNpgsql(configuration["DB_CONNECTION_STRING"]);
            }

            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }

        public DbSet<ServerEmoji> ServerEmojis { get; set; }
        public DbSet<BlacklistedUser> BlacklistedUsers { get; set; }
        public DbSet<UserNote> UserNotes { get; set; }
        public DbSet<StickyRole> StickyRoles { get; set; }
        public DbSet<StickyUser> StickyUsers { get; set; }
        public DbSet<ContestVote> ContestVotes { get; set; }
        public DbSet<ContestSubmission> ContestSubmissions { get; set; }
        public DbSet<ModApplication> ModApplications { get; set; }
        public DbSet<ArtistApplication> ArtistApplications { get; set; }
        public DbSet<StreamerApplication> StreamerApplications { get; set; }
        public DbSet<ModmailVerification> ModmailVerifications { get; set; }
        public DbSet<DmMessageTemplate> DmMessageTemplates { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<SaxtonOwnRole> SaxtonOwnRoles { get; set; }
        
        public DbSet<VerificationEntry> VerificationEntries { get; set; }
    }
}