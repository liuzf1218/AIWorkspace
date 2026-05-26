package com.aiworkspace.ui.components

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AttachFile
import androidx.compose.material.icons.filled.CameraAlt
import androidx.compose.material.icons.filled.Close
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
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage
import com.aiworkspace.utils.ImageUtils

@Composable
fun ChatInput(
    onSendMessage: (String, String?) -> Unit,
    isEnabled: Boolean = true,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current
    var text by remember { mutableStateOf("") }
    var imageBase64 by remember { mutableStateOf<String?>(null) }
    var cameraUri by remember { mutableStateOf<Uri?>(null) }

    val cameraLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.TakePicture()
    ) { success ->
        if (success) {
            cameraUri?.let { uri ->
                imageBase64 = ImageUtils.uriToBase64(context, uri)
            }
        }
        cameraUri = null
    }

    val galleryLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.GetContent()
    ) { uri ->
        uri?.let {
            imageBase64 = ImageUtils.uriToBase64(context, it)
        }
    }

    Column(modifier = modifier.fillMaxWidth()) {
        // Image preview
        if (imageBase64 != null) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 4.dp)
            ) {
                AsyncImage(
                    model = imageBase64,
                    contentDescription = "Selected image",
                    contentScale = ContentScale.Crop,
                    modifier = Modifier.size(48.dp)
                )
                Spacer(modifier = Modifier.width(8.dp))
                Text(
                    text = "Image attached",
                    style = MaterialTheme.typography.labelMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.weight(1f)
                )
                IconButton(
                    onClick = { imageBase64 = null },
                    modifier = Modifier.size(32.dp)
                ) {
                    Icon(
                        Icons.Default.Close,
                        contentDescription = "Remove image",
                        modifier = Modifier.size(18.dp)
                    )
                }
            }
        }

        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .padding(8.dp)
        ) {
            IconButton(
                onClick = {
                    ImageUtils.createImageUri(context)?.let { uri ->
                        cameraUri = uri
                        cameraLauncher.launch(uri)
                    }
                },
                enabled = isEnabled
            ) {
                Icon(Icons.Default.CameraAlt, contentDescription = "Camera")
            }

            IconButton(
                onClick = { galleryLauncher.launch("image/*") },
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
                        if (text.isNotBlank() || imageBase64 != null) {
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
                    if (text.isNotBlank() || imageBase64 != null) {
                        onSendMessage(text.trim(), imageBase64)
                        text = ""
                        imageBase64 = null
                    }
                },
                enabled = isEnabled && (text.isNotBlank() || imageBase64 != null)
            ) {
                Icon(Icons.Default.Send, contentDescription = "Send")
            }
        }
    }
}
