package com.aiworkspace.data.entity

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.ForeignKey
import androidx.room.PrimaryKey

@Entity(
    tableName = "messages",
    foreignKeys = [
        ForeignKey(
            entity = ConversationEntity::class,
            parentColumns = ["id"],
            childColumns = ["conversation_id"],
            onDelete = ForeignKey.CASCADE
        )
    ]
)
data class MessageEntity(
    @PrimaryKey
    val id: String,

    @ColumnInfo(name = "conversation_id")
    val conversationId: String,

    val role: String, // "user" | "assistant" | "system"

    val content: String,

    @ColumnInfo(name = "image_data")
    val imageData: String? = null,

    @ColumnInfo(name = "tokens_used")
    val tokensUsed: Int? = null,

    @ColumnInfo(name = "created_at")
    val createdAt: Long = System.currentTimeMillis()
)
