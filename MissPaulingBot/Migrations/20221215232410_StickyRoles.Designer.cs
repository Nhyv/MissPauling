﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MissPaulingBot.Common;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissPaulingBot.Migrations
{
    [DbContext(typeof(PaulingDbContext))]
    [Migration("20221215232410_StickyRoles")]
    partial class StickyRoles
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MissPaulingBot.Common.Models.ArtistApplication", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<DateTimeOffset>("AppliedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("applied_at");

                    b.Property<string>("UploadUrl")
                        .HasColumnType("text")
                        .HasColumnName("upload_url");

                    b.HasKey("UserId")
                        .HasName("pk_artist_applications");

                    b.ToTable("artist_applications", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.BlacklistedUser", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<decimal>("ModeratorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("moderator_id");

                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("UserId")
                        .HasName("pk_blacklisted_users");

                    b.ToTable("blacklisted_users", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.Contest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("AllowResultsViewingAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_results_viewing_after");

                    b.Property<DateTimeOffset>("AllowResultsViewingUntil")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_results_viewing_until");

                    b.Property<DateTimeOffset>("AllowSubmissionsAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_submissions_after");

                    b.Property<DateTimeOffset>("AllowSubmissionsUntil")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_submissions_until");

                    b.Property<DateTimeOffset>("AllowVotingAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_voting_after");

                    b.Property<DateTimeOffset>("AllowVotingUntil")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("allow_voting_until");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_contests");

                    b.ToTable("contests", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.ContestSubmission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ContestId")
                        .HasColumnType("integer")
                        .HasColumnName("contest_id");

                    b.Property<decimal>("CreatorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("creator_id");

                    b.Property<byte[]>("Data")
                        .HasColumnType("bytea")
                        .HasColumnName("data");

                    b.Property<string>("Extension")
                        .HasColumnType("text")
                        .HasColumnName("extension");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.HasKey("Id")
                        .HasName("pk_contest_submissions");

                    b.ToTable("contest_submissions", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.ContestVote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ContestId")
                        .HasColumnType("integer")
                        .HasColumnName("contest_id");

                    b.Property<int>("SubmissionId")
                        .HasColumnType("integer")
                        .HasColumnName("submission_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_contest_votes");

                    b.ToTable("contest_votes", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.DmMessageTemplate", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<decimal>("AuthorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("author_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Response")
                        .HasColumnType("text")
                        .HasColumnName("response");

                    b.HasKey("Name")
                        .HasName("pk_dm_message_templates");

                    b.ToTable("dm_message_templates", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.ModApplication", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<string>("AbuseResponse")
                        .HasColumnType("text")
                        .HasColumnName("abuse_response");

                    b.Property<string>("AgeResponse")
                        .HasColumnType("text")
                        .HasColumnName("age_response");

                    b.Property<string>("AvailabilitiesResponse")
                        .HasColumnType("text")
                        .HasColumnName("availabilities_response");

                    b.Property<string>("ButtingHeadsResponse")
                        .HasColumnType("text")
                        .HasColumnName("butting_heads_response");

                    b.Property<string>("ButtonsResponse")
                        .HasColumnType("text")
                        .HasColumnName("buttons_response");

                    b.Property<string>("ChangeResponse")
                        .HasColumnType("text")
                        .HasColumnName("change_response");

                    b.Property<string>("ChannelsResponse")
                        .HasColumnType("text")
                        .HasColumnName("channels_response");

                    b.Property<bool>("HasApplied")
                        .HasColumnType("boolean")
                        .HasColumnName("has_applied");

                    b.Property<string>("PersonalResponse")
                        .HasColumnType("text")
                        .HasColumnName("personal_response");

                    b.Property<string>("QualificationResponse")
                        .HasColumnType("text")
                        .HasColumnName("qualification_response");

                    b.Property<string>("ReasonResponse")
                        .HasColumnType("text")
                        .HasColumnName("reason_response");

                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("UserId")
                        .HasName("pk_mod_applications");

                    b.ToTable("mod_applications", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.ModmailVerification", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.HasKey("UserId")
                        .HasName("pk_modmail_verifications");

                    b.ToTable("modmail_verifications", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.Poll", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<bool>("DisplayVotesPublicly")
                        .HasColumnType("boolean")
                        .HasColumnName("display_votes_publicly");

                    b.Property<decimal?>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<DateTimeOffset>("OpenPollAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("open_poll_after");

                    b.Property<DateTimeOffset>("RemoveVotingAfter")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("remove_voting_after");

                    b.HasKey("Id")
                        .HasName("pk_polls");

                    b.ToTable("polls", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.PollOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<int>("PollId")
                        .HasColumnType("integer")
                        .HasColumnName("poll_id");

                    b.HasKey("Id")
                        .HasName("pk_poll_options");

                    b.HasIndex("PollId")
                        .HasDatabaseName("ix_poll_options_poll_id");

                    b.ToTable("poll_options", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.PollVote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("OptionId")
                        .HasColumnType("integer")
                        .HasColumnName("option_id");

                    b.Property<int>("PollId")
                        .HasColumnType("integer")
                        .HasColumnName("poll_id");

                    b.Property<decimal>("VoterId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("voter_id");

                    b.HasKey("Id")
                        .HasName("pk_poll_votes");

                    b.HasIndex("OptionId")
                        .HasDatabaseName("ix_poll_votes_option_id");

                    b.ToTable("poll_votes", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.ServerEmoji", b =>
                {
                    b.Property<decimal>("EmojiId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("emoji_id");

                    b.Property<int>("Usage")
                        .HasColumnType("integer")
                        .HasColumnName("usage");

                    b.HasKey("EmojiId")
                        .HasName("pk_server_emojis");

                    b.ToTable("server_emojis", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.StickyRole", b =>
                {
                    b.Property<decimal>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.Property<string>("RoleName")
                        .HasColumnType("text")
                        .HasColumnName("role_name");

                    b.HasKey("RoleId")
                        .HasName("pk_sticky_roles");

                    b.ToTable("sticky_roles", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.StickyUser", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.HasKey("UserId")
                        .HasName("pk_sticky_users");

                    b.ToTable("sticky_users", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.StreamerApplication", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<string>("ContentAnswer")
                        .HasColumnType("text")
                        .HasColumnName("content_answer");

                    b.Property<string>("ReasonAnswer")
                        .HasColumnType("text")
                        .HasColumnName("reason_answer");

                    b.HasKey("UserId")
                        .HasName("pk_streamer_applications");

                    b.ToTable("streamer_applications", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.UserNote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("GivenAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("given_at");

                    b.Property<decimal>("ModeratorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("moderator_id");

                    b.Property<string>("Note")
                        .HasColumnType("text")
                        .HasColumnName("note");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_user_notes");

                    b.ToTable("user_notes", (string)null);
                });

            modelBuilder.Entity("StickyRoleStickyUser", b =>
                {
                    b.Property<decimal>("StickyRolesRoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("sticky_roles_role_id");

                    b.Property<decimal>("StickyUsersUserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("sticky_users_user_id");

                    b.HasKey("StickyRolesRoleId", "StickyUsersUserId")
                        .HasName("pk_sticky_role_sticky_user");

                    b.HasIndex("StickyUsersUserId")
                        .HasDatabaseName("ix_sticky_role_sticky_user_sticky_users_user_id");

                    b.ToTable("sticky_role_sticky_user", (string)null);
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.PollOption", b =>
                {
                    b.HasOne("MissPaulingBot.Common.Models.Poll", "Poll")
                        .WithMany("Options")
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_poll_options_polls_poll_id");

                    b.Navigation("Poll");
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.PollVote", b =>
                {
                    b.HasOne("MissPaulingBot.Common.Models.PollOption", "Option")
                        .WithMany("Votes")
                        .HasForeignKey("OptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_poll_votes_poll_options_option_id");

                    b.Navigation("Option");
                });

            modelBuilder.Entity("StickyRoleStickyUser", b =>
                {
                    b.HasOne("MissPaulingBot.Common.Models.StickyRole", null)
                        .WithMany()
                        .HasForeignKey("StickyRolesRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_sticky_role_sticky_user_sticky_roles_sticky_roles_temp_id");

                    b.HasOne("MissPaulingBot.Common.Models.StickyUser", null)
                        .WithMany()
                        .HasForeignKey("StickyUsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_sticky_role_sticky_user_sticky_users_sticky_users_temp_id");
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.Poll", b =>
                {
                    b.Navigation("Options");
                });

            modelBuilder.Entity("MissPaulingBot.Common.Models.PollOption", b =>
                {
                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
