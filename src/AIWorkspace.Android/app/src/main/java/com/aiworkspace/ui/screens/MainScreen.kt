package com.aiworkspace.ui.screens

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.DrawerValue
import androidx.compose.material3.ModalNavigationDrawer
import androidx.compose.material3.Scaffold
import androidx.compose.material3.rememberDrawerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import com.aiworkspace.ui.components.ChatTopBar
import com.aiworkspace.ui.components.Sidebar
import com.aiworkspace.viewmodel.ChatViewModel
import com.aiworkspace.viewmodel.ProviderViewModel
import kotlinx.coroutines.launch

@Composable
fun MainScreen(
    chatViewModel: ChatViewModel = viewModel(factory = ChatViewModelFactory()),
    providerViewModel: ProviderViewModel = viewModel(factory = ProviderViewModelFactory())
) {
    val drawerState = rememberDrawerState(DrawerValue.Closed)
    val scope = rememberCoroutineScope()

    val conversations = chatViewModel.conversations.collectAsState().value
    val currentConversationId = chatViewModel.currentConversationId.collectAsState().value
    val messages = chatViewModel.messages.collectAsState().value
    val isStreaming = chatViewModel.isStreaming.collectAsState().value
    val streamContent = chatViewModel.streamContent.collectAsState().value
    val error = chatViewModel.error.collectAsState().value
    val selectedProvider = chatViewModel.selectedProvider.collectAsState().value
    val selectedModel = chatViewModel.selectedModel.collectAsState().value

    val providers = providerViewModel.providers.collectAsState().value
    val models = providerViewModel.models.collectAsState().value

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            Sidebar(
                conversations = conversations,
                currentConversationId = currentConversationId,
                onConversationClick = { id ->
                    chatViewModel.selectConversation(id)
                    scope.launch { drawerState.close() }
                },
                onNewChat = {
                    chatViewModel.createConversation()
                    scope.launch { drawerState.close() }
                },
                onDeleteConversation = { id ->
                    chatViewModel.deleteConversation(id)
                }
            )
        }
    ) {
        Scaffold(
            topBar = {
                ChatTopBar(
                    providers = providers,
                    models = models,
                    selectedProvider = selectedProvider,
                    selectedModel = selectedModel,
                    onProviderSelected = { provider ->
                        chatViewModel.setProvider(provider)
                        providerViewModel.selectProvider(provider?.id)
                    },
                    onModelSelected = { modelId ->
                        chatViewModel.setModel(modelId)
                        providerViewModel.selectModel(modelId)
                    },
                    onMenuClick = {
                        scope.launch { drawerState.open() }
                    }
                )
            }
        ) { padding ->
            ChatScreen(
                messages = messages,
                isStreaming = isStreaming,
                streamContent = streamContent,
                error = error,
                onSendMessage = { content, image ->
                    chatViewModel.sendMessage(content, image)
                },
                onDismissError = { chatViewModel.clearError() },
                modifier = Modifier.padding(padding)
            )
        }
    }
}
