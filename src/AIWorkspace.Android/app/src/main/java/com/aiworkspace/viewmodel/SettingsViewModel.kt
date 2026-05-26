package com.aiworkspace.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.aiworkspace.data.repository.SettingsRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class SettingsViewModel(private val settingsRepository: SettingsRepository) : ViewModel() {

    private val _theme = MutableStateFlow("dark")
    val theme: StateFlow<String> = _theme.asStateFlow()

    private val _maxFileSizeMb = MutableStateFlow(10)
    val maxFileSizeMb: StateFlow<Int> = _maxFileSizeMb.asStateFlow()

    private val _maxImageLongEdge = MutableStateFlow(1536)
    val maxImageLongEdge: StateFlow<Int> = _maxImageLongEdge.asStateFlow()

    init {
        loadSettings()
    }

    fun loadSettings() {
        viewModelScope.launch {
            _theme.value = settingsRepository.getString("theme", "dark")
            _maxFileSizeMb.value = settingsRepository.getInt("max_file_size_mb", 10)
            _maxImageLongEdge.value = settingsRepository.getInt("max_image_long_edge", 1536)
        }
    }

    fun setTheme(value: String) {
        viewModelScope.launch {
            settingsRepository.setString("theme", value)
            _theme.value = value
        }
    }

    fun setMaxFileSizeMb(value: Int) {
        viewModelScope.launch {
            settingsRepository.setInt("max_file_size_mb", value)
            _maxFileSizeMb.value = value
        }
    }

    fun setMaxImageLongEdge(value: Int) {
        viewModelScope.launch {
            settingsRepository.setInt("max_image_long_edge", value)
            _maxImageLongEdge.value = value
        }
    }

    class Factory(private val settingsRepository: SettingsRepository) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            return SettingsViewModel(settingsRepository) as T
        }
    }
}
