package com.aiworkspace.ui.screens

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.aiworkspace.AIWorkspaceApplication
import com.aiworkspace.data.repository.SettingsRepository
import com.aiworkspace.viewmodel.SettingsViewModel

@Suppress("UNCHECKED_CAST")
class SettingsViewModelFactory : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        val app = AIWorkspaceApplication.instance
        val settingsRepo = SettingsRepository(app.database.settingsDao())
        return SettingsViewModel(settingsRepo) as T
    }
}
