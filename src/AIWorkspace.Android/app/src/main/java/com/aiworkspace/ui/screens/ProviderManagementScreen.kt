package com.aiworkspace.ui.screens

import androidx.compose.foundation.layout.Arrangement
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
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.aiworkspace.data.entity.ProviderEntity
import com.aiworkspace.viewmodel.ProviderViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProviderManagementScreen(
    providerViewModel: ProviderViewModel,
    onBack: () -> Unit
) {
    val providers by providerViewModel.providers.collectAsState()
    val isLoading by providerViewModel.isLoading.collectAsState()
    val error by providerViewModel.error.collectAsState()

    var showAddDialog by remember { mutableStateOf(false) }
    var editingProvider by remember { mutableStateOf<ProviderEntity?>(null) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Providers") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { showAddDialog = true }) {
                Icon(Icons.Default.Add, contentDescription = "Add Provider")
            }
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier
                        .align(Alignment.CenterHorizontally)
                        .padding(16.dp)
                )
            }

            if (providers.isEmpty() && !isLoading) {
                Text(
                    text = "No providers configured. Tap + to add one.",
                    style = MaterialTheme.typography.bodyLarge,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(32.dp)
                )
            } else {
                LazyColumn(
                    modifier = Modifier.fillMaxSize(),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(providers) { provider ->
                        ProviderCard(
                            provider = provider,
                            onEdit = { editingProvider = provider },
                            onDelete = { providerViewModel.deleteProvider(provider) },
                            onToggleEnabled = {
                                providerViewModel.updateProvider(
                                    provider.copy(isEnabled = !provider.isEnabled)
                                )
                            }
                        )
                    }
                }
            }
        }
    }

    // Add/Edit dialog
    if (showAddDialog || editingProvider != null) {
        ProviderDialog(
            provider = editingProvider,
            onDismiss = {
                showAddDialog = false
                editingProvider = null
            },
            onConfirm = { name, apiBaseUrl, apiKey, proxyUrl, supportsVision ->
                if (editingProvider != null) {
                    providerViewModel.updateProvider(
                        editingProvider!!.copy(
                            name = name,
                            apiBaseUrl = apiBaseUrl,
                            proxyUrl = proxyUrl.ifBlank { null },
                            supportsVision = supportsVision
                        ),
                        apiKey = apiKey.ifBlank { null }
                    )
                } else {
                    providerViewModel.addProvider(
                        ProviderEntity(
                            name = name,
                            apiBaseUrl = apiBaseUrl,
                            apiKey = ByteArray(0),
                            proxyUrl = proxyUrl.ifBlank { null },
                            supportsVision = supportsVision
                        ),
                        apiKey = apiKey
                    )
                }
                showAddDialog = false
                editingProvider = null
            },
            onFetchModels = { provider ->
                providerViewModel.fetchModelsFromApi(provider)
            }
        )
    }

    // Error dialog
    if (error != null) {
        AlertDialog(
            onDismissRequest = { providerViewModel.clearError() },
            title = { Text("Error") },
            text = { Text(error!!) },
            confirmButton = {
                TextButton(onClick = { providerViewModel.clearError() }) {
                    Text("OK")
                }
            }
        )
    }
}

@Composable
private fun ProviderCard(
    provider: ProviderEntity,
    onEdit: () -> Unit,
    onDelete: () -> Unit,
    onToggleEnabled: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.fillMaxWidth()
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = provider.name,
                        style = MaterialTheme.typography.titleMedium,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                    Text(
                        text = provider.apiBaseUrl,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                }

                Switch(
                    checked = provider.isEnabled,
                    onCheckedChange = { onToggleEnabled() }
                )
            }

            Spacer(modifier = Modifier.height(8.dp))

            Row {
                if (provider.supportsVision) {
                    Text(
                        text = "Vision",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.padding(end = 8.dp)
                    )
                }
                Text(
                    text = if (provider.apiKey.isNotEmpty()) "API Key set" else "No API Key",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            Spacer(modifier = Modifier.height(8.dp))

            Row(
                horizontalArrangement = Arrangement.End,
                modifier = Modifier.fillMaxWidth()
            ) {
                IconButton(onClick = onEdit) {
                    Icon(Icons.Default.Edit, contentDescription = "Edit")
                }
                IconButton(onClick = onDelete) {
                    Icon(
                        Icons.Default.Delete,
                        contentDescription = "Delete",
                        tint = MaterialTheme.colorScheme.error
                    )
                }
            }
        }
    }
}

@Composable
private fun ProviderDialog(
    provider: ProviderEntity?,
    onDismiss: () -> Unit,
    onConfirm: (String, String, String, String, Boolean) -> Unit,
    onFetchModels: (ProviderEntity) -> Unit
) {
    var name by remember { mutableStateOf(provider?.name ?: "") }
    var apiBaseUrl by remember { mutableStateOf(provider?.apiBaseUrl ?: "") }
    var apiKey by remember { mutableStateOf("") }
    var proxyUrl by remember { mutableStateOf(provider?.proxyUrl ?: "") }
    var supportsVision by remember { mutableStateOf(provider?.supportsVision ?: false) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(if (provider != null) "Edit Provider" else "Add Provider") },
        text = {
            Column {
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Name") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))
                OutlinedTextField(
                    value = apiBaseUrl,
                    onValueChange = { apiBaseUrl = it },
                    label = { Text("API Base URL") },
                    placeholder = { Text("https://api.openai.com/v1") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))
                OutlinedTextField(
                    value = apiKey,
                    onValueChange = { apiKey = it },
                    label = { Text("API Key") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))
                OutlinedTextField(
                    value = proxyUrl,
                    onValueChange = { proxyUrl = it },
                    label = { Text("Proxy URL (optional)") },
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Switch(
                        checked = supportsVision,
                        onCheckedChange = { supportsVision = it }
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Supports Vision")
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = {
                    if (name.isNotBlank() && apiBaseUrl.isNotBlank()) {
                        onConfirm(name, apiBaseUrl, apiKey, proxyUrl, supportsVision)
                    }
                },
                enabled = name.isNotBlank() && apiBaseUrl.isNotBlank()
            ) {
                Text("Save")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}
