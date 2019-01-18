using System;
using Configuration;
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
        public virtual DbSet<Finisher> Finisher { get; set; }
        public virtual DbSet<IndexedVideo> IndexedVideo { get; set; }
        public virtual DbSet<Joiplatform> Joiplatform { get; set; }
        public virtual DbSet<KinkHashes> KinkHashes { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<PornhubVideos> PornhubVideos { get; set; }
        public virtual DbSet<Puns> Puns { get; set; }
        public virtual DbSet<SabrinaSettings> SabrinaSettings { get; set; }
        public virtual DbSet<SankakuImage> SankakuImage { get; set; }
        public virtual DbSet<SankakuImageTag> SankakuImageTag { get; set; }
        public virtual DbSet<SankakuImageVote> SankakuImageVote { get; set; }
        public virtual DbSet<SankakuPost> SankakuPost { get; set; }
        public virtual DbSet<SankakuTag> SankakuTag { get; set; }
        public virtual DbSet<SankakuTagBlacklist> SankakuTagBlacklist { get; set; }
        public virtual DbSet<SankakuTagWhitelist> SankakuTagWhitelist { get; set; }
        public virtual DbSet<Slavereports> Slavereports { get; set; }
        public virtual DbSet<TumblrPosts> TumblrPosts { get; set; }
        public virtual DbSet<UserSettings> UserSettings { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<WheelChances> WheelChances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Config.DatabaseConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Boost>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<Creator>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DiscordUserId).HasColumnName("DiscordUserID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<CreatorPlatformLink>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatorId).HasColumnName("CreatorID");

                entity.Property(e => e.Identification).IsRequired();

                entity.Property(e => e.PlatformId).HasColumnName("PlatformID");

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

            modelBuilder.Entity<DungeonMob>(entity =>
            {
                entity.ToTable("Dungeon.Mob");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<DungeonRoomEnterMessage>(entity =>
            {
                entity.ToTable("Dungeon.RoomEnterMessage");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<DungeonSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);

                entity.ToTable("Dungeon.Session");

                entity.Property(e => e.SessionId)
                    .HasColumnName("SessionID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DungeonData).IsRequired();

                entity.Property(e => e.RoomGuid).IsRequired();

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DungeonSession)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dungeon.Session_Users");
            });

            modelBuilder.Entity<DungeonText>(entity =>
            {
                entity.ToTable("Dungeon.Text");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("ntext");
            });

            modelBuilder.Entity<DungeonVariable>(entity =>
            {
                entity.ToTable("Dungeon.Variable");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.TextId).HasColumnName("TextID");

                entity.HasOne(d => d.Text)
                    .WithMany(p => p.DungeonVariable)
                    .HasForeignKey(d => d.TextId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dungeon.Variables_Dungeon.Variables");
            });

            modelBuilder.Entity<Finisher>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatorId).HasColumnName("CreatorID");

                entity.Property(e => e.Link).IsRequired();

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.Finisher)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Finisher_Creator");
            });

            modelBuilder.Entity<IndexedVideo>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreatorId).HasColumnName("CreatorID");

                entity.Property(e => e.Link).IsRequired();

                entity.Property(e => e.PlatformId).HasColumnName("PlatformID");

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

            modelBuilder.Entity<Joiplatform>(entity =>
            {
                entity.ToTable("JOIPlatform");

                entity.Property(e => e.Id).HasColumnName("ID");
            });

            modelBuilder.Entity<KinkHashes>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Hash)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.KinkHashes)
                    .HasForeignKey<KinkHashes>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KinkHashes_Users");
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.Property(e => e.AuthorId).HasColumnName("AuthorID");

                entity.Property(e => e.ChannelId).HasColumnName("ChannelID");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.MessageText).IsRequired();

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Messages_Users");
            });

            modelBuilder.Entity<PornhubVideos>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Creator).IsRequired();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).IsRequired();

                entity.Property(e => e.Title).IsRequired();

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("URL");
            });

            modelBuilder.Entity<Puns>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LastUsed).HasColumnType("datetime");

                entity.Property(e => e.Text).IsRequired();
            });

            modelBuilder.Entity<SabrinaSettings>(entity =>
            {
                entity.HasKey(e => e.GuildId);

                entity.Property(e => e.GuildId)
                    .HasColumnName("GuildID")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastDeepLearningPost).HasColumnType("datetime");

                entity.Property(e => e.LastIntroductionPost).HasColumnType("datetime");

                entity.Property(e => e.LastTumblrPost).HasColumnType("datetime");

                entity.Property(e => e.LastTumblrUpdate).HasColumnType("datetime");

                entity.Property(e => e.LastWheelHelpPost).HasColumnType("datetime");
            });

            modelBuilder.Entity<SankakuImage>(entity =>
            {
                entity.ToTable("Sankaku.Image");

                entity.HasIndex(e => e.Id)
                    .HasName("UQ__Sankaku.__3214EC265A0AA101")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<SankakuImageTag>(entity =>
            {
                entity.ToTable("Sankaku.ImageTag");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.HasOne(d => d.Image)
                    .WithMany(p => p.SankakuImageTag)
                    .HasForeignKey(d => d.ImageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.ImageTag_Sankaku.ImageTag");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SankakuImageTag)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.ImageTag_Sankaku.Tag");
            });

            modelBuilder.Entity<SankakuImageVote>(entity =>
            {
                entity.ToTable("Sankaku.ImageVote");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Image)
                    .WithMany(p => p.SankakuImageVote)
                    .HasForeignKey(d => d.ImageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.ImageVote_Sankaku.Image");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SankakuImageVote)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.ImageVote_Users");
            });

            modelBuilder.Entity<SankakuPost>(entity =>
            {
                entity.ToTable("Sankaku.Post");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.HasOne(d => d.Image)
                    .WithMany(p => p.SankakuPost)
                    .HasForeignKey(d => d.ImageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.Post_Sankaku.Image");
            });

            modelBuilder.Entity<SankakuTag>(entity =>
            {
                entity.ToTable("Sankaku.Tag");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SankakuTagBlacklist>(entity =>
            {
                entity.ToTable("Sankaku.TagBlacklist");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChannelId).HasColumnName("ChannelID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SankakuTagBlacklist)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.TagBlacklist_Sankaku.Tag");
            });

            modelBuilder.Entity<SankakuTagWhitelist>(entity =>
            {
                entity.ToTable("Sankaku.TagWhitelist");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChannelId).HasColumnName("ChannelID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SankakuTagWhitelist)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.TagWhitelist_Sankaku.Tag");
            });

            modelBuilder.Entity<Slavereports>(entity =>
            {
                entity.HasKey(e => e.SlaveReportId);

                entity.Property(e => e.SlaveReportId).HasColumnName("SlaveReportID");

                entity.Property(e => e.SessionOutcome)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.TimeOfReport).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

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

                entity.Property(e => e.TumblrId)
                    .HasColumnName("TumblrID")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastPosted).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserSettings)
                    .HasForeignKey<UserSettings>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSettings_Users");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BanTime).HasColumnType("datetime");

                entity.Property(e => e.DenialTime).HasColumnType("datetime");

                entity.Property(e => e.LockTime).HasColumnType("datetime");

                entity.Property(e => e.MuteTime).HasColumnType("datetime");

                entity.Property(e => e.RuinTime).HasColumnType("datetime");

                entity.Property(e => e.SpecialTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<WheelChances>(entity =>
            {
                entity.HasKey(e => e.Difficulty)
                    .HasName("PK_WheelChances_1");

                entity.Property(e => e.Difficulty).ValueGeneratedNever();
            });
        }
    }
}
