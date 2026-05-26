package com.aiworkspace.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Stop
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.aiworkspace.data.entity.MessageEntity
import com.aiworkspace.ui.components.ChatInput
import com.aiworkspace.ui.components.MessageBubble

@Composable
fun ChatScreen(
    messages: List<MessageEntity>,
    isStreaming: Boolean,
    streamContent: String,
    error: String?,
    searchQuery: String = "",
    searchResults: List<MessageEntity> = emptyList(),
    isSearching: Boolean = false,
    onSendMessage: (String, String?) -> Unit,
    onDismissError: () -> Unit,
    onRetry: () -> Unit = {},
    onAbort: () -> Unit = {},
    onSearch: (String) -> Unit = {},
    onClearSearch: () -> Unit = {},
    modifier: Modifier = Modifier
) {
    val listState = rememberLazyListState()
    val displayMessages = if (searchQuery.isNotBlank()) searchResults else messages

    // Auto-scroll when messages change or streaming content updates
    LaunchedEffect(displayMessages.size, streamContent, isStreaming) {
        val targetIndex = when {
            displayMessages.isEmpty() && isStreaming -> 0
            displayMessages.isNotEmpty() -> displayMessages.size - 1
            else -> return@LaunchedEffect
        }
        listState.animateScrollToItem(targetIndex)
    }

    Column(modifier = modifier.fillMaxSize()) {
        // Search bar
        if (searchQuery.isNotBlank() || isSearching) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 4.dp)
            ) {
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = onSearch,
                    placeholder = { Text("Search messages...") },
                    modifier = Modifier.weight(1f),
                    singleLine = true
                )
                Spacer(modifier = Modifier.width(8.dp))
                IconButton(onClick = onClearSearch) {
                    Icon(Icons.Default.Close, contentDescription = "Clear search")
                }
            }
        }

        // Messages list
        Box(modifier = Modifier.weight(1f)) {
            if (displayMessages.isEmpty() && !isStreaming) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = if (searchQuery.isNotBlank()) "No results found"
                        else "Start a new conversation",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(32.dp)
                    )
                }
            } else {
                LazyColumn(
                    state = listState,
                    modifier = Modifier.fillMaxSize()
                ) {
                    items(displayMessages, key = { it.id }) { message ->
                        MessageBubble(message = message)
                    }

                    // Streaming message
                    if (isStreaming) {
                        item(key = "streaming") {
                            MessageBubble(
                                message = MessageEntity(
                                    id = "streaming",
                                    conversationId = "",
                                    role = "assistant",
                                    content = streamContent.ifBlank { "Thinking..." }
                                )
                            )
                        }
                    }
                }
            }
        }

        // Error banner with retry
        if (error != null) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 4.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.errorContainer
                )
            ) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 12.dp, vertical = 8.dp)
                ) {
                    Text(
                        text = error,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onErrorContainer,
                        modifier = Modifier.weight(1f)
                    )
                    TextButton(
                        onClick = onRetry,
                        colors = ButtonDefaults.textButtonColors(
                            contentColor = MaterialTheme.colorScheme.onErrorContainer
                        )
                    ) {
                        Icon(Icons.Default.Refresh, contentDescription = null)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Retry")
                    }
                    IconButton(onClick = onDismissError) {
                        Icon(
                            Icons.Default.Close,
                            contentDescription = "Dismiss",
                            tint = MaterialTheme.colorScheme.onErrorContainer
                        )
                    }
                }
            }
        }

        // Abort button during streaming
        if (isStreaming) {
            Row(
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier.fillMaxWidth()
            ) {
                TextButton(onClick = onAbort) {
                    Icon(Icons.Default.Stop, contentDescription = null)
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("Stop generating")
                }
            }
        }

        // Input
        ChatInput(
            onSendMessage = onSendMessage,
            isEnabled = !isStreaming,
            modifier = Modifier.fillMaxWidth()
        )
    }
}
