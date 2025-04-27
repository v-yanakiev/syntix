using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Enums;

namespace Models;

public partial class PostgresContext(DbContextOptions<PostgresContext> options)
    : IdentityDbContext<UserInfo>(options), IDataProtectionKeyContext
{
    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<ExecutionMachine> ExecutionMachine { get; set; }
    public virtual DbSet<ExecutionMachineTemplate> FlyMachineTemplates { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");
            entity.ToTable("log");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.Content).HasColumnName("content");
        });
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chats_pkey");
            entity.ToTable("chat");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.CreatorId).HasDefaultValueSql("gen_random_uuid()").HasColumnName("creator_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.HasOne(d => d.Creator).WithMany(p => p.Chats).HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ExecutionMachine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("execution_machine_pkey");
            entity.ToTable("execution_machine");
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LastActive)
                .HasColumnName("last_active");
            entity.Property(e => e.ChatId)
                .HasColumnName("chat_id");
            entity.Property(e => e.ExecutionMachineTemplateId)
                .HasColumnName("execution_machine_template_id");

            entity.HasOne(d => d.User)
                .WithMany(u => u.ExecutionMachines)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Chat)
                .WithMany(u => u.ExecutionMachines)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.ExecutionMachineTemplate)
                .WithMany(emt => emt.ExecutionMachines)
                .HasForeignKey(d => d.ExecutionMachineTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(d => d.AppAddress).HasColumnName("app_address");
        });
        modelBuilder.Entity<ExecutionMachineTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("execution_machine_templates_pkey");
            entity.ToTable("execution_machine_template");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e=>e.Type).HasColumnName("type").IsRequired();
            entity.HasIndex(e => e.Type);
        });
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");
            entity.ToTable("message");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.HasOne(d => d.Chat).WithMany(p => p.Messages).HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
            entity.ToTable("user_info");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        });
        modelBuilder.HasSequence<int>("seq_schema_version", "graphql").IsCyclic();
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}