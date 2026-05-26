package com.aiworkspace.data.entity

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.ForeignKey
import androidx.room.Index
import androidx.room.PrimaryKey

@Entity(
    tableName = "models",
    foreignKeys = [
        ForeignKey(
            entity = ProviderEntity::class,
            parentColumns = ["id"],
            childColumns = ["provider_id"],
            onDelete = ForeignKey.CASCADE
        )
    ],
    indices = [Index(value = ["provider_id", "model_id"], unique = true)]
)
data class ModelEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Int = 0,

    @ColumnInfo(name = "provider_id")
    val providerId: Int,

    @ColumnInfo(name = "model_id")
    val modelId: String,

    @ColumnInfo(name = "display_name")
    val displayName: String? = null,

    @ColumnInfo(name = "supports_vision")
    val supportsVision: Boolean = false,

    @ColumnInfo(name = "is_default")
    val isDefault: Boolean = false
)
