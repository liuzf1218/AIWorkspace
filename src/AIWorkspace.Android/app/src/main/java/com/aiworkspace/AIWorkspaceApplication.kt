package com.aiworkspace

import android.app.Application
import androidx.room.Room
import com.aiworkspace.data.db.AppDatabase
import com.aiworkspace.data.security.KeystoreEncryption

class AIWorkspaceApplication : Application() {

    lateinit var database: AppDatabase
        private set

    lateinit var encryption: KeystoreEncryption
        private set

    override fun onCreate() {
        super.onCreate()

        instance = this

        database = Room.databaseBuilder(
            applicationContext,
            AppDatabase::class.java,
            "aiworkspace.db"
        )
            .fallbackToDestructiveMigration()
            .build()

        encryption = KeystoreEncryption()
    }

    companion object {
        lateinit var instance: AIWorkspaceApplication
            private set
    }
}
