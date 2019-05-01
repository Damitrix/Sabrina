using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<SabrinaVersion> SabrinaVersion { get; set; }
        public virtual DbSet<SankakuImage> SankakuImage { get; set; }
        public virtual DbSet<SankakuImageTag> SankakuImageTag { get; set; }
        public virtual DbSet<SankakuImageVote> SankakuImageVote { get; set; }
        public virtual DbSet<SankakuPost> SankakuPost { get; set; }
        public virtual DbSet<SankakuTag> SankakuTag { get; set; }
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
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UserSetting> UserSetting { get; set; }
        public virtual DbSet<WaifuJoiAlbum> WaifuJoiAlbum { get; set; }
        public virtual DbSet<WaifuJoiContentPost> WaifuJoiContentPost { get; set; }
        public virtual DbSet<WheelChances> WheelChances { get; set; }
        public virtual DbSet<WheelOutcome> WheelOutcome { get; set; }
        public virtual DbSet<WheelUserItem> WheelUserItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.Config.DatabaseConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "3.0.0-preview3.19153.1");

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

                entity.Property(e => e.MessageId)
                    .HasColumnName("MessageID")
                    .ValueGeneratedNever();

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

            modelBuilder.Entity<SabrinaVersion>(entity =>
            {
                entity.HasKey(e => e.VersionNumber);

                entity.ToTable("Sabrina.Version");

                entity.Property(e => e.VersionNumber).ValueGeneratedNever();

                entity.Property(e => e.Description).IsRequired();
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

                entity.HasIndex(e => e.ImageId)
                    .HasName("IX_Sankaku.ImageTag");

                entity.HasIndex(e => e.TagId)
                    .HasName("IX_Sankaku.ImageTag_1");

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

                entity.Property(e => e.Date).HasColumnType("datetime2(1)");

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

            modelBuilder.Entity<SankakuTagWhiteList>(entity =>
            {
                entity.ToTable("Sankaku.TagWhiteList");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChannelId).HasColumnName("ChannelID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.SankakuTagWhiteList)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sankaku.TagWhiteList_Sankaku.Tag");
            });

            modelBuilder.Entity<ScenarioLocation>(entity =>
            {
                entity.ToTable("Scenario.Location");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ScenarioLocationModifier>(entity =>
            {
                entity.ToTable("Scenario.LocationModifier");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ScenarioLocationModifierLink>(entity =>
            {
                entity.ToTable("Scenario.LocationModifierLink");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.LocationModifierId).HasColumnName("LocationModifierID");

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
                entity.ToTable("Scenario.Name");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Text).IsRequired();
            });

            modelBuilder.Entity<ScenarioRace>(entity =>
            {
                entity.ToTable("Scenario.Race");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ScenarioRaceLink>(entity =>
            {
                entity.ToTable("Scenario.RaceLink");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.RaceId).HasColumnName("RaceID");

                entity.Property(e => e.RaceModifierId).HasColumnName("RaceModifierID");

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

            modelBuilder.Entity<ScenarioRaceModifier>(entity =>
            {
                entity.ToTable("Scenario.RaceModifier");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ScenarioRaceNameLink>(entity =>
            {
                entity.ToTable("Scenario.RaceNameLink");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.NameId).HasColumnName("NameID");

                entity.Property(e => e.RaceId).HasColumnName("RaceID");

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

            modelBuilder.Entity<ScenarioSave>(entity =>
            {
                entity.ToTable("Scenario.Save");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.LocationModifierId).HasColumnName("LocationModifierID");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ScenarioSavePlayer>(entity =>
            {
                entity.ToTable("Scenario.SavePlayer");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.RaceId).HasColumnName("RaceID");

                entity.Property(e => e.RaceModifierId).HasColumnName("RaceModifierID");

                entity.Property(e => e.SaveId).HasColumnName("SaveID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

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
                entity.ToTable("Scenario.SavePlayerNameLink");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.NameId).HasColumnName("NameID");

                entity.Property(e => e.SavePlayerId).HasColumnName("SavePlayerID");

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

            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.SettingId).HasColumnName("SettingID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSetting)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSetting_Users");
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

            modelBuilder.Entity<WaifuJoiAlbum>(entity =>
            {
                entity.ToTable("WaifuJOI.Album");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ContentId)
                    .IsRequired()
                    .HasMaxLength(24)
                    .IsFixedLength();
            });

            modelBuilder.Entity<WaifuJoiContentPost>(entity =>
            {
                entity.ToTable("WaifuJOI.ContentPost");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ContentId)
                    .IsRequired()
                    .HasMaxLength(24)
                    .IsFixedLength();

                entity.Property(e => e.Time).HasColumnType("datetime");
            });

            modelBuilder.Entity<WheelChances>(entity =>
            {
                entity.HasKey(e => e.Difficulty)
                    .HasName("PK_WheelChances_1");

                entity.Property(e => e.Difficulty).ValueGeneratedNever();
            });

            modelBuilder.Entity<WheelOutcome>(entity =>
            {
                entity.ToTable("Wheel.Outcome");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<WheelUserItem>(entity =>
            {
                entity.ToTable("Wheel.UserItem");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ItemId).HasColumnName("ItemID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.WheelUserItem)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wheel.UserItem_Users");
            });
        }
    }
}