package com.aiworkspace.data.repository

import com.aiworkspace.data.dao.SettingsDao
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class SettingsRepository(private val settingsDao: SettingsDao) {

    suspend fun get(key: String): String? = withContext(Dispatchers.IO) {
        settingsDao.getValue(key)
    }

    suspend fun set(key: String, value: String) = withContext(Dispatchers.IO) {
        settingsDao.setValue(key, value)
    }

    suspend fun getString(key: String, default: String = ""): String {
        return get(key) ?: default
    }

    suspend fun setString(key: String, value: String) {
        set(key, value)
    }

    suspend fun getInt(key: String, default: Int = 0): Int {
        return get(key)?.toIntOrNull() ?: default
    }

    suspend fun setInt(key: String, value: Int) {
        set(key, value.toString())
    }

    suspend fun getTheme(): String {
        return get("theme") ?: "dark"
    }

    suspend fun setTheme(theme: String) {
        set("theme", theme)
    }

    suspend fun getDefaultProviderId(): Int? {
        return get("default_provider_id")?.toIntOrNull()
    }

    suspend fun setDefaultProviderId(id: Int?) {
        set("default_provider_id", id?.toString() ?: "")
    }

    suspend fun getDefaultModelId(): String? {
        return get("default_model_id")
    }

    suspend fun setDefaultModelId(modelId: String?) {
        set("default_model_id", modelId ?: "")
    }
}
