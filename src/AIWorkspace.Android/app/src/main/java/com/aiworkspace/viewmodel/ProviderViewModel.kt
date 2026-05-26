package com.aiworkspace.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.aiworkspace.data.entity.ModelEntity
import com.aiworkspace.data.entity.ProviderEntity
import com.aiworkspace.data.repository.ProviderRepository
import com.aiworkspace.network.SseEventSource
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class ProviderViewModel(
    private val providerRepository: ProviderRepository,
    private val sseEventSource: SseEventSource
) : ViewModel() {

    private val _providers = MutableStateFlow<List<ProviderEntity>>(emptyList())
    val providers: StateFlow<List<ProviderEntity>> = _providers.asStateFlow()

    private val _models = MutableStateFlow<List<ModelEntity>>(emptyList())
    val models: StateFlow<List<ModelEntity>> = _models.asStateFlow()

    private val _selectedProviderId = MutableStateFlow<Int?>(null)
    val selectedProviderId: StateFlow<Int?> = _selectedProviderId.asStateFlow()

    private val _selectedModelId = MutableStateFlow<String?>(null)
    val selectedModelId: StateFlow<String?> = _selectedModelId.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    init {
        loadProviders()
    }

    fun loadProviders() {
        viewModelScope.launch {
            providerRepository.getAllEnabledProviders().collect {
                _providers.value = it
            }
        }
    }

    fun selectProvider(providerId: Int?) {
        _selectedProviderId.value = providerId
        providerId?.let { loadModels(it) }
    }

    fun selectModel(modelId: String?) {
        _selectedModelId.value = modelId
    }

    fun loadModels(providerId: Int) {
        viewModelScope.launch {
            providerRepository.getModels(providerId).collect {
                _models.value = it
            }
        }
    }

    fun fetchModelsFromApi(provider: ProviderEntity) {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            try {
                val apiKey = providerRepository.decryptApiKey(provider.apiKey)
                val modelIds = sseEventSource.fetchModels(provider.apiBaseUrl, apiKey)
                val modelEntities = modelIds.map { modelId ->
                    ModelEntity(
                        providerId = provider.id,
                        modelId = modelId,
                        displayName = modelId
                    )
                }
                providerRepository.saveModels(provider.id, modelEntities)
                _models.value = modelEntities
            } catch (e: Exception) {
                _error.value = e.message
            } finally {
                _isLoading.value = false
            }
        }
    }

    fun addProvider(provider: ProviderEntity, apiKey: String) {
        viewModelScope.launch {
            providerRepository.addProvider(provider, apiKey)
        }
    }

    fun updateProvider(provider: ProviderEntity, apiKey: String? = null) {
        viewModelScope.launch {
            providerRepository.updateProvider(provider, apiKey)
        }
    }

    fun deleteProvider(provider: ProviderEntity) {
        viewModelScope.launch {
            providerRepository.deleteProvider(provider)
        }
    }

    fun clearError() {
        _error.value = null
    }

    class Factory(
        private val providerRepository: ProviderRepository,
        private val sseEventSource: SseEventSource
    ) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            return ProviderViewModel(providerRepository, sseEventSource) as T
        }
    }
}
