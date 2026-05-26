package com.aiworkspace.data.repository

import com.aiworkspace.data.db.ConversationDao
import com.aiworkspace.data.db.MessageDao
import com.aiworkspace.data.entity.ConversationEntity
import com.aiworkspace.data.entity.MessageEntity
import kotlinx.coroutines.flow.Flow
import java.util.UUID

class ChatRepository(
    private val conversationDao: ConversationDao,
    private val messageDao: MessageDao
) {
    fun getConversations(): Flow<List<ConversationEntity>> = conversationDao.getAll()

    suspend fun getConversationById(id: String): ConversationEntity? = conversationDao.getById(id)

    suspend fun createConversation(title: String = "New Chat"): String {
        val id = UUID.randomUUID().toString()
        val conversation = ConversationEntity(
            id = id,
            title = title
        )
        conversationDao.insert(conversation)
        return id
    }

    suspend fun deleteConversation(id: String) {
        conversationDao.deleteById(id)
        messageDao.deleteByConversationId(id)
    }

    suspend fun updateConversationTitle(id: String, title: String) {
        conversationDao.getById(id)?.let {
            conversationDao.update(it.copy(title = title))
        }
    }

    suspend fun updateConversationTimestamp(id: String) {
        conversationDao.updateTimestamp(id)
    }

    fun getMessages(conversationId: String): Flow<List<MessageEntity>> =
        messageDao.getByConversationId(conversationId)

    suspend fun addMessage(message: MessageEntity) {
        messageDao.insert(message)
    }

    suspend fun getRecentMessages(conversationId: String, limit: Int = 20): List<MessageEntity> =
        messageDao.getRecentMessages(conversationId, limit)
}
