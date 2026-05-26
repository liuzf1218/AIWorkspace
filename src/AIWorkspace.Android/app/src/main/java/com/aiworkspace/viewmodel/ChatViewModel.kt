package com.aiworkspace.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.aiworkspace.data.entity.ConversationEntity
import com.aiworkspace.data.entity.MessageEntity
import com.aiworkspace.data.entity.ProviderEntity
import com.aiworkspace.data.repository.ChatRepository
import com.aiworkspace.data.repository.ProviderRepository
import com.aiworkspace.data.repository.SettingsRepository
import com.aiworkspace.network.SseEventSource
import com.aiworkspace.network.model.ChatImageDto
import com.aiworkspace.network.model.ChatMessageDto
import com.aiworkspace.network.model.ChatRequest
import com.aiworkspace.network.model.ImageUrlDto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.util.UUID

class ChatViewModel(
    private val chatRepository: ChatRepository,
    private val providerRepository: ProviderRepository,
    private val settingsRepository: SettingsRepository,
    private val sseEventSource: SseEventSource,
    private val decryptKey: (ByteArray) -> String
) : ViewModel() {

    private val _conversations = MutableStateFlow<List<ConversationEntity>>(emptyList())
    val conversations: StateFlow<List<ConversationEntity>> = _conversations.asStateFlow()

    private val _messages = MutableStateFlow<List<MessageEntity>>(emptyList())
    val messages: StateFlow<List<MessageEntity>> = _messages.asStateFlow()

    private val _currentConversationId = MutableStateFlow<String?>(null)
    val currentConversationId: StateFlow<String?> = _currentConversationId.asStateFlow()

    private val _isStreaming = MutableStateFlow(false)
    val isStreaming: StateFlow<Boolean> = _isStreaming.asStateFlow()

    private val _isThinking = MutableStateFlow(false)
    val isThinking: StateFlow<Boolean> = _isThinking.asStateFlow()

    private val _streamContent = MutableStateFlow("")
    val streamContent: StateFlow<String> = _streamContent.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    private val _selectedProvider = MutableStateFlow<ProviderEntity?>(null)
    val selectedProvider: StateFlow<ProviderEntity?> = _selectedProvider.asStateFlow()

    private val _selectedModel = MutableStateFlow<String?>(null)
    val selectedModel: StateFlow<String?> = _selectedModel.asStateFlow()

    init {
        loadConversations()
        loadDefaultProvider()
    }

    private fun loadDefaultProvider() {
        viewModelScope.launch {
            val providerId = settingsRepository.getDefaultProviderId()
            val modelId = settingsRepository.getDefaultModelId()
            providerId?.let { id ->
                _selectedProvider.value = providerRepository.getProviderById(id)
            }
            _selectedModel.value = modelId
        }
    }

    fun loadConversations() {
        viewModelScope.launch {
            chatRepository.getConversations().collect {
                _conversations.value = it
            }
        }
    }

    fun createConversation(title: String = "New Chat") {
        viewModelScope.launch {
            val id = chatRepository.createConversation(title)
            _currentConversationId.value = id
            _messages.value = emptyList()
        }
    }

    fun selectConversation(id: String) {
        viewModelScope.launch {
            _currentConversationId.value = id
            chatRepository.getMessages(id).collect {
                _messages.value = it
            }
        }
    }

    fun deleteConversation(id: String) {
        viewModelScope.launch {
            chatRepository.deleteConversation(id)
            if (_currentConversationId.value == id) {
                _currentConversationId.value = null
                _messages.value = emptyList()
            }
        }
    }

    fun setProvider(provider: ProviderEntity?) {
        _selectedProvider.value = provider
        viewModelScope.launch {
            provider?.id?.let { settingsRepository.setDefaultProviderId(it) }
        }
    }

    fun setModel(modelId: String?) {
        _selectedModel.value = modelId
        viewModelScope.launch {
            settingsRepository.setDefaultModelId(modelId)
        }
    }

    fun sendMessage(content: String, imageBase64: String? = null) {
        val provider = _selectedProvider.value ?: return
        val modelId = _selectedModel.value ?: return
        val convId = _currentConversationId.value ?: run {
            viewModelScope.launch {
                val newId = chatRepository.createConversation()
                _currentConversationId.value = newId
                doSendMessage(newId, content, imageBase64, provider, modelId)
            }
            return
        }
        doSendMessage(convId, content, imageBase64, provider, modelId)
    }

    private fun doSendMessage(
        convId: String,
        content: String,
        imageBase64: String?,
        provider: ProviderEntity,
        modelId: String
    ) {
        viewModelScope.launch {
            // Save user message
            val userMsg = MessageEntity(
                id = UUID.randomUUID().toString(),
                conversationId = convId,
                role = "user",
                content = content,
                imageData = imageBase64
            )
            chatRepository.addMessage(userMsg)
            chatRepository.updateConversationTimestamp(convId)

            // Prepare context messages
            val history = chatRepository.getRecentMessages(convId, 20)
            val apiMessages = history.map { msg ->
                val images = msg.imageData?.let { base64 ->
                    listOf(ChatImageDto(imageUrl = ImageUrlDto(url = base64)))
                }
                ChatMessageDto(role = msg.role, content = msg.content, images = images)
            }

            _isStreaming.value = true
            _isThinking.value = true
            _streamContent.value = ""
            _error.value = null

            val apiKey = decryptKey(provider.apiKey)
            val request = ChatRequest(
                model = modelId,
                messages = apiMessages
            )

            sseEventSource.streamChat(
                request = request,
                apiBaseUrl = provider.apiBaseUrl,
                apiKey = apiKey,
                onChunk = { delta ->
                    _isThinking.value = false
                    _streamContent.value += delta
                },
                onError = { e ->
                    _isStreaming.value = false
                    _isThinking.value = false
                    _error.value = e.message
                },
                onDone = {
                    viewModelScope.launch {
                        val assistantMsg = MessageEntity(
                            id = UUID.randomUUID().toString(),
                            conversationId = convId,
                            role = "assistant",
                            content = _streamContent.value
                        )
                        chatRepository.addMessage(assistantMsg)
                        chatRepository.updateConversationTimestamp(convId)
                        _isStreaming.value = false
                        _isThinking.value = false
                        _streamContent.value = ""
                    }
                }
            )
        }
    }

    fun abortChat() {
        _isStreaming.value = false
        _isThinking.value = false
    }

    fun clearError() {
        _error.value = null
    }

    class Factory(
        private val chatRepository: ChatRepository,
        private val providerRepository: ProviderRepository,
        private val settingsRepository: SettingsRepository,
        private val sseEventSource: SseEventSource,
        private val decryptKey: (ByteArray) -> String
    ) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            return ChatViewModel(
                chatRepository,
                providerRepository,
                settingsRepository,
                sseEventSource,
                decryptKey
            ) as T
        }
    }
}
