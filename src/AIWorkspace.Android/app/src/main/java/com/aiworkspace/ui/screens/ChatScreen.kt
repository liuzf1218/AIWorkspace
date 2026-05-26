package com.aiworkspace.ui.screens

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
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
    onSendMessage: (String, String?) -> Unit,
    onDismissError: () -> Unit,
    modifier: Modifier = Modifier
) {
    val listState = rememberLazyListState()

    // Auto-scroll when messages change or streaming content updates
    LaunchedEffect(messages.size, streamContent, isStreaming) {
        val targetIndex = when {
            messages.isEmpty() && isStreaming -> 0
            messages.isNotEmpty() -> messages.size - 1
            else -> return@LaunchedEffect
        }
        listState.animateScrollToItem(targetIndex)
    }

    Column(modifier = modifier.fillMaxSize()) {
        // Messages list
        Box(modifier = Modifier.weight(1f)) {
            if (messages.isEmpty() && !isStreaming) {
                // Empty state
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = "Start a new conversation",
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
                    items(messages, key = { it.id }) { message ->
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

        // Error dialog
        if (error != null) {
            AlertDialog(
                onDismissRequest = onDismissError,
                title = { Text("Error") },
                text = { Text(error) },
                confirmButton = {
                    TextButton(onClick = onDismissError) {
                        Text("OK")
                    }
                }
            )
        }

        // Input
        ChatInput(
            onSendMessage = onSendMessage,
            isEnabled = !isStreaming,
            modifier = Modifier.fillMaxWidth()
        )
    }
}
