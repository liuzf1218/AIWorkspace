package com.aiworkspace.ui.components

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AttachFile
import androidx.compose.material.icons.filled.CameraAlt
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
import androidx.compose.ui.unit.dp

@Composable
fun ChatInput(
    onSendMessage: (String, String?) -> Unit,
    isEnabled: Boolean = true,
    modifier: Modifier = Modifier
) {
    var text by remember { mutableStateOf("") }
    var imageBase64 by remember { mutableStateOf<String?>(null) }

    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier = modifier
            .fillMaxWidth()
            .padding(8.dp)
    ) {
        IconButton(
            onClick = { /* TODO: Open camera */ },
            enabled = isEnabled
        ) {
            Icon(Icons.Default.CameraAlt, contentDescription = "Camera")
        }

        IconButton(
            onClick = { /* TODO: Open file picker */ },
            enabled = isEnabled
        ) {
            Icon(Icons.Default.AttachFile, contentDescription = "Attach file")
        }

        OutlinedTextField(
            value = text,
            onValueChange = { text = it },
            placeholder = { Text("Type your message...") },
            keyboardOptions = KeyboardOptions(
                capitalization = KeyboardCapitalization.Sentences,
                imeAction = ImeAction.Send
            ),
            keyboardActions = KeyboardActions(
                onSend = {
                    if (text.isNotBlank()) {
                        onSendMessage(text.trim(), imageBase64)
                        text = ""
                        imageBase64 = null
                    }
                }
            ),
            modifier = Modifier.weight(1f),
            enabled = isEnabled,
            maxLines = 5
        )

        IconButton(
            onClick = {
                if (text.isNotBlank()) {
                    onSendMessage(text.trim(), imageBase64)
                    text = ""
                    imageBase64 = null
                }
            },
            enabled = isEnabled && text.isNotBlank()
        ) {
            Icon(Icons.Default.Send, contentDescription = "Send")
        }
    }
}
