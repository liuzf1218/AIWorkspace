using AIWorkspace.Core.Models;
using Microsoft.Data.Sqlite;

namespace AIWorkspace.Core.Services;

public class ChatService
{
    private readonly DatabaseService _db;

    public ChatService(DatabaseService db)
    {
        _db = db;
    }

    public List<Conversation> GetConversations()
    {
        var conversations = new List<Conversation>();
        using var cmd = new SqliteCommand(
            "SELECT id, title, model_id, provider_id, created_at, updated_at FROM conversations ORDER BY updated_at DESC",
            _db.GetConnection());
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            conversations.Add(new Conversation
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                ModelId = reader.IsDBNull(2) ? null : reader.GetString(2),
                ProviderId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                UpdatedAt = reader.GetDateTime(5)
            });
        }
        return conversations;
    }

    public Conversation CreateConversation(string title, string? modelId = null, int? providerId = null)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            ModelId = modelId,
            ProviderId = providerId
        };
        using var cmd = new SqliteCommand(@"
            INSERT INTO conversations (id, title, model_id, provider_id)
            VALUES (@id, @title, @model_id, @provider_id)",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", conversation.Id);
        cmd.Parameters.AddWithValue("@title", conversation.Title);
        cmd.Parameters.AddWithValue("@model_id", (object?)conversation.ModelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@provider_id", (object?)conversation.ProviderId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
        return conversation;
    }

    public void DeleteConversation(string id)
    {
        using var cmd = new SqliteCommand("DELETE FROM conversations WHERE id = @id", _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void UpdateConversationTitle(string id, string title)
    {
        using var cmd = new SqliteCommand(
            "UPDATE conversations SET title = @title, updated_at = CURRENT_TIMESTAMP WHERE id = @id",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.ExecuteNonQuery();
    }

    public List<Message> GetMessages(string conversationId)
    {
        var messages = new List<Message>();
        using var cmd = new SqliteCommand(
            "SELECT id, conversation_id, role, content, image_data, tokens_used, created_at FROM messages WHERE conversation_id = @conversation_id ORDER BY created_at",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@conversation_id", conversationId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            messages.Add(new Message
            {
                Id = reader.GetString(0),
                ConversationId = reader.GetString(1),
                Role = reader.GetString(2),
                Content = reader.GetString(3),
                ImageData = reader.IsDBNull(4) ? null : reader.GetString(4),
                TokensUsed = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreatedAt = reader.GetDateTime(6)
            });
        }
        return messages;
    }

    public Message AddMessage(string conversationId, string role, string content, string? imageData = null, int? tokensUsed = null)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            ImageData = imageData,
            TokensUsed = tokensUsed
        };
        using var cmd = new SqliteCommand(@"
            INSERT INTO messages (id, conversation_id, role, content, image_data, tokens_used)
            VALUES (@id, @conversation_id, @role, @content, @image_data, @tokens_used)",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", message.Id);
        cmd.Parameters.AddWithValue("@conversation_id", message.ConversationId);
        cmd.Parameters.AddWithValue("@role", message.Role);
        cmd.Parameters.AddWithValue("@content", message.Content);
        cmd.Parameters.AddWithValue("@image_data", (object?)message.ImageData ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tokens_used", (object?)message.TokensUsed ?? DBNull.Value);
        cmd.ExecuteNonQuery();

        // Update conversation updated_at
        using var updateCmd = new SqliteCommand(
            "UPDATE conversations SET updated_at = CURRENT_TIMESTAMP WHERE id = @id",
            _db.GetConnection());
        updateCmd.Parameters.AddWithValue("@id", conversationId);
        updateCmd.ExecuteNonQuery();

        return message;
    }
}
