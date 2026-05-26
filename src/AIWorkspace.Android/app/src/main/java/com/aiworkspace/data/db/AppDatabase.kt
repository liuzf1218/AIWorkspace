package com.aiworkspace.data.db

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import androidx.sqlite.db.SupportSQLiteDatabase
import com.aiworkspace.data.entity.AttachmentEntity
import com.aiworkspace.data.entity.ConversationEntity
import com.aiworkspace.data.entity.MessageEntity
import com.aiworkspace.data.entity.ModelEntity
import com.aiworkspace.data.entity.ProviderEntity
import com.aiworkspace.data.entity.SettingsEntity
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

@TypeConverters(Converters::class)
@Database(
    entities = [
        ConversationEntity::class,
        MessageEntity::class,
        ProviderEntity::class,
        ModelEntity::class,
        AttachmentEntity::class,
        SettingsEntity::class
    ],
    version = 1,
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {
    abstract fun conversationDao(): ConversationDao
    abstract fun messageDao(): MessageDao
    abstract fun providerDao(): ProviderDao
    abstract fun settingsDao(): SettingsDao

    companion object {
        @Volatile
        private var INSTANCE: AppDatabase? = null

        fun getDatabase(context: Context): AppDatabase {
            return INSTANCE ?: synchronized(this) {
                val instance = Room.databaseBuilder(
                    context.applicationContext,
                    AppDatabase::class.java,
                    "aiworkspace.db"
                )
                    .addCallback(DatabaseCallback())
                    .build()
                INSTANCE = instance
                instance
            }
        }
    }

    private class DatabaseCallback : RoomDatabase.Callback() {
        override fun onCreate(db: SupportSQLiteDatabase) {
            super.onCreate(db)
            // Pre-populate default settings
            CoroutineScope(Dispatchers.IO).launch {
                db.execSQL(
                    """
                    INSERT OR IGNORE INTO settings (`key`, value) VALUES
                    ('theme', 'dark'),
                    ('max_file_size_mb', '10'),
                    ('max_image_long_edge', '1536');
                    """.trimIndent()
                )
            }
        }
    }
}
