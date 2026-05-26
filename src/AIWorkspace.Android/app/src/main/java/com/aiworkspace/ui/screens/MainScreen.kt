package com.aiworkspace.ui.screens

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.DrawerValue
import androidx.compose.material3.ModalNavigationDrawer
import androidx.compose.material3.Scaffold
import androidx.compose.material3.rememberDrawerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import com.aiworkspace.ui.components.ChatTopBar
import com.aiworkspace.ui.components.Sidebar
import com.aiworkspace.ui.navigation.AppScreen
import com.aiworkspace.viewmodel.ChatViewModel
import com.aiworkspace.viewmodel.ProviderViewModel
import com.aiworkspace.viewmodel.SettingsViewModel
import kotlinx.coroutines.launch

@Composable
fun MainScreen(
    chatViewModel: ChatViewModel = viewModel(factory = ChatViewModelFactory()),
    providerViewModel: ProviderViewModel = viewModel(factory = ProviderViewModelFactory()),
    settingsViewModel: SettingsViewModel = viewModel(factory = SettingsViewModelFactory())
) {
    val drawerState = rememberDrawerState(DrawerValue.Closed)
    val scope = rememberCoroutineScope()
    var currentScreen by remember { mutableStateOf(AppScreen.Chat) }

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
                    currentScreen = AppScreen.Chat
                    scope.launch { drawerState.close() }
                },
                onNewChat = {
                    chatViewModel.createConversation()
                    currentScreen = AppScreen.Chat
                    scope.launch { drawerState.close() }
                },
                onDeleteConversation = { id ->
                    chatViewModel.deleteConversation(id)
                },
                onNavigateToProviders = {
                    currentScreen = AppScreen.Providers
                    scope.launch { drawerState.close() }
                },
                onNavigateToSettings = {
                    currentScreen = AppScreen.Settings
                    scope.launch { drawerState.close() }
                }
            )
        }
    ) {
        when (currentScreen) {
            AppScreen.Chat -> {
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

            AppScreen.Providers -> {
                ProviderManagementScreen(
                    providerViewModel = providerViewModel,
                    onBack = { currentScreen = AppScreen.Chat }
                )
            }

            AppScreen.Settings -> {
                SettingsScreen(
                    settingsViewModel = settingsViewModel,
                    onBack = { currentScreen = AppScreen.Chat }
                )
            }
        }
    }
}
