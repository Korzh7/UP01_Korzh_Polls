using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UP01_Korzh_Polls.Models;

public partial class PollsDbContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр контекста базы данных <see cref="PollsDbContext"/>.
    /// </summary>
    public PollsDbContext()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр контекста базы данных с заданными параметрами.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста базы данных.</param>
    public PollsDbContext(DbContextOptions<PollsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    /// <summary>
    /// Конфигурирует параметры подключения к базе данных MS SQL Server.
    /// </summary>
    /// <param name="optionsBuilder">Построитель параметров конфигурации контекста.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=DB_Korzh_Polls;Trusted_Connection=True;TrustServerCertificate=True;");

    /// <summary>
    /// Настраивает сопоставление сущностей и связей с таблицами базы данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__answer__3213E83F683CAB07");

            entity.ToTable("answer");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.TextAnswer).HasColumnName("text_answer");

            entity.HasOne(d => d.Option).WithMany(p => p.Answers)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK_Answer_Option");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Question");

            entity.HasOne(d => d.Response).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ResponseId)
                .HasConstraintName("FK_Answer_Response");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__category__3213E83F608AA238");

            entity.ToTable("category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__poll__3213E83F29A297ED");

            entity.ToTable("poll");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsAnonymous).HasColumnName("is_anonymous");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("РђРєС‚РёРІРµРЅ")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Polls)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Poll_Category");

            entity.HasOne(d => d.User).WithMany(p => p.Polls)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Poll_User");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__question__3213E83F230F9D73");

            entity.ToTable("question");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderNumber)
                .HasDefaultValue(1)
                .HasColumnName("order_number");
            entity.Property(e => e.PollId).HasColumnName("poll_id");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("РћРґРёРЅРѕС‡РЅС‹Р№ РІС‹Р±РѕСЂ")
                .HasColumnName("type");

            entity.HasOne(d => d.Poll).WithMany(p => p.Questions)
                .HasForeignKey(d => d.PollId)
                .HasConstraintName("FK_Question_Poll");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__question__3213E83FD15A7F51");

            entity.ToTable("question_option");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Text)
                .HasMaxLength(255)
                .HasColumnName("text");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_QuestionOption_Question");
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__response__3213E83F0696AFF0");

            entity.ToTable("response");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PollId).HasColumnName("poll_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Poll).WithMany(p => p.Responses)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Response_Poll");

            entity.HasOne(d => d.User).WithMany(p => p.Responses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Response_User");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_acc__3213E83FEB9EC9C4");

            entity.ToTable("user_account");

            entity.HasIndex(e => e.Login, "UQ__user_acc__7838F272FE12072B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Photo).HasColumnName("photo");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
