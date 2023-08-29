using System.Collections.Generic;
using System.Text;
using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MissPaulingBot.Common.Models
{
    public class ModApplication : IEntityTypeConfiguration<ModApplication>
    {
        public ulong UserId { get; set; }
        
        public string Username { get; set; }
        
        public string AgeResponse { get; set; }
        
        public string AvailabilitiesResponse { get; set; }
        
        public string ChannelsResponse { get; set; }
        
        public string QualificationResponse { get; set; }
        
        public string ReasonResponse { get; set; }
        
        public string PersonalResponse { get; set; }
        
        public string ButtonsResponse { get; set; }
        
        public string ButtingHeadsResponse { get; set; }
        
        public string AbuseResponse { get; set; }
        
        public string ChangeResponse { get; set; }

        public bool HasApplied { get; set; }

        public bool TryValidate(out string error)
        {
            var errorBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(AgeResponse))
            {
                errorBuilder.AppendLine("Age is not filled.");
            }

            if (string.IsNullOrWhiteSpace(AvailabilitiesResponse))
            {
                errorBuilder.AppendLine("Availabilities are not filled.");
            }

            if (string.IsNullOrWhiteSpace(ChannelsResponse))
            {
                errorBuilder.AppendLine("Channel activity is not filled.");
            }

            if (string.IsNullOrWhiteSpace(QualificationResponse))
            {
                errorBuilder.AppendLine("Qualifications are not filled.");
            }

            if (string.IsNullOrWhiteSpace(ReasonResponse))
            {
                errorBuilder.AppendLine("Reasons for moderating are not filled.");
            }

            if (string.IsNullOrWhiteSpace(ButtonsResponse) || string.IsNullOrWhiteSpace(ButtingHeadsResponse) ||
                string.IsNullOrWhiteSpace(AbuseResponse))
            {
                errorBuilder.AppendLine("One or more hypotheticals were not responded to.");
            }

            if (errorBuilder.Length > 0)
            {
                error = errorBuilder.ToString();
                return false;
            }

            error = null;
            return true;
        }

        public List<LocalEmbedField> ToFields()
        {
            var fields = new List<LocalEmbedField>
            {
                new LocalEmbedField()
                    .WithName("Please state your age.")
                    .WithValue(AgeResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("Describe your availabilities and your timezone. " +
                              "What times and days are you normally available to moderate?")
                    .WithValue(AvailabilitiesResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("Which channels are you active in?")
                    .WithValue(ChannelsResponse ?? "No response" ),
                new LocalEmbedField()
                    .WithName("What do you think qualifies you to be a moderator here?")
                    .WithValue(QualificationResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("Why do you want to be a moderator here?")
                    .WithValue(ReasonResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("Tells us something about yourself. Feel free to humble brag, or tell us a fun fact" +
                              " about yourself or your life!")
                    .WithValue(PersonalResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("A user is pushing everyone's buttons but in a way that doesn't break any rules." +
                              " Nobody has directly come to you to report anything, but it's apparent that they" +
                              " are a rule-abiding nuisance. Will you talk to them about this, and if so," +
                              " what will you say?")
                    .WithValue(ButtonsResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("You cannot seem to get along with another moderator. You always tend to butt heads" +
                              " with them, and it's very hard to hold a professional conversation with them because" +
                              " you both just can't seem to agree on anything. What would you do in this situation?")
                    .WithValue(ButtingHeadsResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("You feel a moderator is abusing their powers or otherwise becoming unfit to moderate." +
                              " What would you do to prevent this from becoming a bigger problem?")
                    .WithValue(AbuseResponse ?? "No response"),
                new LocalEmbedField()
                    .WithName("As a moderator," +
                              " what would you like to bring to this server? Do you have any concerns or desires to improve" +
                              " the state of the server?")
                    .WithValue(ChangeResponse ?? "No response")
            };

            return fields;
        }

        public void Configure(EntityTypeBuilder<ModApplication> builder)
        {
            builder.HasKey(x => x.UserId);
        }
    }
}