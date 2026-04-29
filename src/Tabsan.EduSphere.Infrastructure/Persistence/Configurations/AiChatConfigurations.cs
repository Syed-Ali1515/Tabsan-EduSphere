using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.AiChat;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ChatConversation"/>.</summary>
internal sealed class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
{
    /// <summary>Configures table name, column constraints, and indexes.</summary>
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.ToTable("chat_conversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserRole).IsRequired().HasMaxLength(50);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.UserId, c.StartedAt });
    }
}

/// <summary>EF Core configuration for <see cref="ChatMessage"/>.</summary>
internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    /// <summary>Configures table name, column constraints, FK, and indexes.</summary>
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasMaxLength(16000);

        builder.HasOne(m => m.Conversation)
               .WithMany(c => c.Messages)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });
    }
}
