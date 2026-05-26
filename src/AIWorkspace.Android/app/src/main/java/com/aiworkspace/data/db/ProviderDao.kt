package com.aiworkspace.data.db

import androidx.room.Dao
import androidx.room.Delete
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import androidx.room.Update
import com.aiworkspace.data.entity.ModelEntity
import com.aiworkspace.data.entity.ProviderEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface ProviderDao {
    @Query("SELECT * FROM providers WHERE is_enabled = 1 ORDER BY created_at DESC")
    fun getAllEnabled(): Flow<List<ProviderEntity>>

    @Query("SELECT * FROM providers ORDER BY created_at DESC")
    fun getAll(): Flow<List<ProviderEntity>>

    @Query("SELECT * FROM providers WHERE id = :id")
    suspend fun getById(id: Int): ProviderEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(provider: ProviderEntity): Long

    @Update
    suspend fun update(provider: ProviderEntity)

    @Delete
    suspend fun delete(provider: ProviderEntity)

    @Query("DELETE FROM providers WHERE id = :id")
    suspend fun deleteById(id: Int)

    // Models
    @Query("SELECT * FROM models WHERE provider_id = :providerId")
    fun getModelsByProviderId(providerId: Int): Flow<List<ModelEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertModels(models: List<ModelEntity>)

    @Query("DELETE FROM models WHERE provider_id = :providerId")
    suspend fun deleteModelsByProviderId(providerId: Int)
}
