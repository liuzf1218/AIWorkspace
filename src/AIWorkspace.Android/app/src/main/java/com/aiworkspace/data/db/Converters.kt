package com.aiworkspace.data.db

import androidx.room.TypeConverter

class Converters {
    @TypeConverter
    fun fromByteArray(value: ByteArray?): String? {
        return value?.let { java.util.Base64.getEncoder().encodeToString(it) }
    }

    @TypeConverter
    fun toByteArray(value: String?): ByteArray? {
        return value?.let { java.util.Base64.getDecoder().decode(it) }
    }
}
