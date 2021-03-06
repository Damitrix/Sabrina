﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Sabrina.Models
{
    public partial class DiscordContext : DbContext
    {
        public DiscordContext()
        {
        }

        public DiscordContext(DbContextOptions<DiscordContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Boost> Boost { get; set; }
        public virtual DbSet<Creator> Creator { get; set; }
        public virtual DbSet<CreatorPlatformLink> CreatorPlatformLink { get; set; }
        public virtual DbSet<DungeonMob> DungeonMob { get; set; }
        public virtual DbSet<DungeonRoomEnterMessage> DungeonRoomEnterMessage { get; set; }
        public virtual DbSet<DungeonSession> DungeonSession { get; set; }
        public virtual DbSet<DungeonText> DungeonText { get; set; }
        public virtual DbSet<DungeonVariable> DungeonVariable { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<EventLocation> EventLocation { get; set; }
        public virtual DbSet<EventRun> EventRun { get; set; }
        public virtual DbSet<Finisher> Finisher { get; set; }
        public virtual DbSet<IndexedVideo> IndexedVideo { get; set; }
        public virtual DbSet<Joiplatform> Joiplatform { get; set; }
        public virtual DbSet<KinkHashes> KinkHashes { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<PornhubVideos> PornhubVideos { get; set; }
        public virtual DbSet<Puns> Puns { get; set; }
        public virtual DbSet<SabrinaSettings> SabrinaSettings { get; set; }
        public virtual DbSet<SabrinaVersion> SabrinaVersion { get; set; }
        public virtual DbSet<SankakuPost> SankakuPost { get; set; }
        public virtual DbSet<SankakuTagBlacklist> SankakuTagBlacklist { get; set; }
        public virtual DbSet<SankakuTagWhiteList> SankakuTagWhiteList { get; set; }
        public virtual DbSet<ScenarioLocation> ScenarioLocation { get; set; }
        public virtual DbSet<ScenarioLocationModifier> ScenarioLocationModifier { get; set; }
        public virtual DbSet<ScenarioLocationModifierLink> ScenarioLocationModifierLink { get; set; }
        public virtual DbSet<ScenarioName> ScenarioName { get; set; }
        public virtual DbSet<ScenarioRace> ScenarioRace { get; set; }
        public virtual DbSet<ScenarioRaceLink> ScenarioRaceLink { get; set; }
        public virtual DbSet<ScenarioRaceModifier> ScenarioRaceModifier { get; set; }
        public virtual DbSet<ScenarioRaceNameLink> ScenarioRaceNameLink { get; set; }
        public virtual DbSet<ScenarioSave> ScenarioSave { get; set; }
        public virtual DbSet<ScenarioSavePlayer> ScenarioSavePlayer { get; set; }
        public virtual DbSet<ScenarioSavePlayerNameLink> ScenarioSavePlayerNameLink { get; set; }
        public virtual DbSet<Slavereports> Slavereports { get; set; }
        public virtual DbSet<TumblrPosts> TumblrPosts { get; set; }
        public virtual DbSet<UserSetting> UserSetting { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<WaifuJoiAlbum> WaifuJoiAlbum { get; set; }
        public virtual DbSet<WaifuJoiContentPost> WaifuJoiContentPost { get; set; }
        public virtual DbSet<WheelChances> WheelChances { get; set; }
        public virtual DbSet<WheelOutcome> WheelOutcome { get; set; }
        public virtual DbSet<WheelSetting> WheelSetting { get; set; }
        public virtual DbSet<WheelUserItem> WheelUserItem { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CreatorPlatformLink>(entity =>
            {
                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.CreatorPlatformLink)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CreatorPlatformLink_Creator");

                entity.HasOne(d => d.Platform)
                    .WithMany(p => p.CreatorPlatformLink)
                    .HasForeignKey(d => d.PlatformId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CreatorPlatformLink_JOIPlatform");
            });

            modelBuilder.Entity<DungeonSession>(entity =>
            {
                entity.HasKey(e => e.SessionId)
                    .HasName("PK_Dungeon.Session");

                entity.Property(e => e.SessionId).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DungeonSession)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dungeon.Session_Users");
            });

            modelBuilder.Entity<DungeonVariable>(entity =>
            {
                entity.HasOne(d => d.Text)
                    .WithMany(p => p.DungeonVariable)
                    .HasForeignKey(d => d.TextId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dungeon.Variables_Dungeon.Variables");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.TriggerIfMissed).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<EventLocation>(entity =>
            {
                entity.Property(e => e.Enabled).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<EventRun>(entity =>
            {
                entity.HasOne(d => d.Event)
                    .WithMany(p => p.EventRun)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EventRun_Event");
            });

            modelBuilder.Entity<Finisher>(entity =>
            {
                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.Finisher)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Finisher_Creator");
            });

            modelBuilder.Entity<IndexedVideo>(entity =>
            {
                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.IndexedVideo)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IndexedVideo_Creator");

                entity.HasOne(d => d.Platform)
                    .WithMany(p => p.IndexedVideo)
                    .HasForeignKey(d => d.PlatformId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IndexedVideo_JOIPlatform");
            });

            modelBuilder.Entity<KinkHashes>(entity =>
            {
                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.Privacy).HasDefaultValueSql("((-1))");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.KinkHashes)
                    .HasForeignKey<KinkHashes>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KinkHashes_Users");
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.Property(e => e.MessageId).ValueGeneratedNever();

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Messages_Users");
            });

            modelBuilder.Entity<SabrinaSettings>(entity =>
            {
                entity.Property(e => e.GuildId).ValueGeneratedNever();

                entity.Property(e => e.LastWaifuJoiUpdate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<SabrinaVersion>(entity =>
            {
                entity.HasKey(e => e.VersionNumber)
                    .HasName("PK_Sabrina.Version");

                entity.Property(e => e.VersionNumber).ValueGeneratedNever();
            });

            modelBuilder.Entity<SankakuPost>(entity =>
            {
                entity.HasIndex(e => e.ImageId)
                    .HasName("IX_Post_ImageID");
            });

            modelBuilder.Entity<ScenarioLocationModifierLink>(entity =>
            {
                entity.HasOne(d => d.Location)
                    .WithMany(p => p.ScenarioLocationModifierLink)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.LocationModifierLink_Scenario.Location");

                entity.HasOne(d => d.LocationModifier)
                    .WithMany(p => p.ScenarioLocationModifierLink)
                    .HasForeignKey(d => d.LocationModifierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.LocationModifierLink_Scenario.LocationModifier");
            });

            modelBuilder.Entity<ScenarioName>(entity =>
            {
                entity.Property(e => e.Type).HasComment("Specifies, if FirstName 1, Lastname 2 or MiddleName 3");
            });

            modelBuilder.Entity<ScenarioRaceLink>(entity =>
            {
                entity.HasOne(d => d.Race)
                    .WithMany(p => p.ScenarioRaceLink)
                    .HasForeignKey(d => d.RaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.RaceLink_Scenario.Race");

                entity.HasOne(d => d.RaceModifier)
                    .WithMany(p => p.ScenarioRaceLink)
                    .HasForeignKey(d => d.RaceModifierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.RaceLink_Scenario.RaceModifier");
            });

            modelBuilder.Entity<ScenarioRaceNameLink>(entity =>
            {
                entity.HasOne(d => d.Name)
                    .WithMany(p => p.ScenarioRaceNameLink)
                    .HasForeignKey(d => d.NameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.RaceNameLink_Scenario.Name");

                entity.HasOne(d => d.Race)
                    .WithMany(p => p.ScenarioRaceNameLink)
                    .HasForeignKey(d => d.RaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.RaceNameLink_Scenario.Race");
            });

            modelBuilder.Entity<ScenarioSavePlayer>(entity =>
            {
                entity.HasOne(d => d.Race)
                    .WithMany(p => p.ScenarioSavePlayer)
                    .HasForeignKey(d => d.RaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayer_Scenario.Race");

                entity.HasOne(d => d.RaceModifier)
                    .WithMany(p => p.ScenarioSavePlayer)
                    .HasForeignKey(d => d.RaceModifierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayer_Scenario.RaceModifier");

                entity.HasOne(d => d.Save)
                    .WithMany(p => p.ScenarioSavePlayer)
                    .HasForeignKey(d => d.SaveId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayer_Scenario.SavePlayer");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ScenarioSavePlayer)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayer_Users");
            });

            modelBuilder.Entity<ScenarioSavePlayerNameLink>(entity =>
            {
                entity.HasOne(d => d.Name)
                    .WithMany(p => p.ScenarioSavePlayerNameLink)
                    .HasForeignKey(d => d.NameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayerNameLink_Scenario.Name");

                entity.HasOne(d => d.SavePlayer)
                    .WithMany(p => p.ScenarioSavePlayerNameLink)
                    .HasForeignKey(d => d.SavePlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Scenario.SavePlayerNameLink_Scenario.SavePlayer");
            });

            modelBuilder.Entity<Slavereports>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Slavereports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Slavereports_Users");
            });

            modelBuilder.Entity<TumblrPosts>(entity =>
            {
                entity.HasKey(e => e.TumblrId)
                    .HasName("PK_TumblrPosts_1");

                entity.Property(e => e.TumblrId).ValueGeneratedNever();

                entity.Property(e => e.IsLoli).HasDefaultValueSql("((-1))");
            });

            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSetting)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSetting_Users");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.TotalEdges).HasDefaultValueSql("((0))");

                entity.Property(e => e.WalletEdges).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<WaifuJoiAlbum>(entity =>
            {
                entity.Property(e => e.ContentId).IsFixedLength();
            });

            modelBuilder.Entity<WaifuJoiContentPost>(entity =>
            {
                entity.Property(e => e.ContentId).IsFixedLength();
            });

            modelBuilder.Entity<WheelChances>(entity =>
            {
                entity.HasKey(e => e.Difficulty)
                    .HasName("PK_WheelChances_1");

                entity.Property(e => e.Difficulty).ValueGeneratedNever();
            });

            modelBuilder.Entity<WheelSetting>(entity =>
            {
                entity.HasKey(e => e.GuildId)
                    .HasName("PK_Wheel.Setting");

                entity.Property(e => e.GuildId).ValueGeneratedNever();
            });

            modelBuilder.Entity<WheelUserItem>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.WheelUserItem)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wheel.UserItem_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}