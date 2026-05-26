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

    LaunchedEffect(messages.size, isStreaming) {
        if (messages.isNotEmpty()) {
            listState.animateScrollToItem(messages.size - 1)
        }
    }

    Column(modifier = modifier.fillMaxSize()) {
        // Messages list
        LazyColumn(
            state = listState,
            modifier = Modifier.weight(1f)
        ) {
            items(messages) { message ->
                MessageBubble(message = message)
            }

            // Streaming indicator
            if (isStreaming && streamContent.isNotEmpty()) {
                item {
                    MessageBubble(
                        message = MessageEntity(
                            id = "streaming",
                            conversationId = "",
                            role = "assistant",
                            content = streamContent
                        )
                    )
                }
            }

            if (isStreaming && streamContent.isEmpty()) {
                item {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        CircularProgressIndicator()
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
