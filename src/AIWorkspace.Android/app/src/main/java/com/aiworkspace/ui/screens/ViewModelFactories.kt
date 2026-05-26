package com.aiworkspace.ui.screens

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.aiworkspace.AIWorkspaceApplication
import com.aiworkspace.data.repository.ChatRepository
import com.aiworkspace.data.repository.ProviderRepository
import com.aiworkspace.data.repository.SettingsRepository
import com.aiworkspace.network.SseEventSource
import com.aiworkspace.viewmodel.ChatViewModel
import com.aiworkspace.viewmodel.ProviderViewModel

@Suppress("UNCHECKED_CAST")
class ChatViewModelFactory : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        val app = AIWorkspaceApplication.instance
        val chatRepo = ChatRepository(
            app.database.conversationDao(),
            app.database.messageDao()
        )
        val providerRepo = ProviderRepository(
            app.database.providerDao(),
            app.encryption
        )
        val settingsRepo = SettingsRepository(app.database.settingsDao())
        return ChatViewModel(
            chatRepo,
            providerRepo,
            settingsRepo,
            SseEventSource(),
            { encrypted -> app.encryption.decrypt(encrypted) }
        ) as T
    }
}

@Suppress("UNCHECKED_CAST")
class ProviderViewModelFactory : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        val app = AIWorkspaceApplication.instance
        val providerRepo = ProviderRepository(
            app.database.providerDao(),
            app.encryption
        )
        return ProviderViewModel(
            providerRepo,
            SseEventSource()
        ) as T
    }
}
