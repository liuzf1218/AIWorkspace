package com.aiworkspace.data.entity

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "providers")
data class ProviderEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Int = 0,

    val name: String,

    @ColumnInfo(name = "api_base_url")
    val apiBaseUrl: String,

    @ColumnInfo(name = "api_key")
    val apiKey: ByteArray, // encrypted

    @ColumnInfo(name = "proxy_url")
    val proxyUrl: String? = null,

    @ColumnInfo(name = "is_enabled")
    val isEnabled: Boolean = true,

    @ColumnInfo(name = "supports_vision")
    val supportsVision: Boolean = false,

    @ColumnInfo(name = "created_at")
    val createdAt: Long = System.currentTimeMillis(),

    @ColumnInfo(name = "updated_at")
    val updatedAt: Long = System.currentTimeMillis()
) {
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ProviderEntity) return false
        return id == other.id && name == other.name && apiBaseUrl == other.apiBaseUrl
    }

    override fun hashCode(): Int {
        var result = id
        result = 31 * result + name.hashCode()
        result = 31 * result + apiBaseUrl.hashCode()
        return result
    }
}
