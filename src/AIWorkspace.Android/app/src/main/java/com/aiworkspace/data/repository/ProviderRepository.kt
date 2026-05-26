package com.aiworkspace.data.repository

import com.aiworkspace.data.db.ProviderDao
import com.aiworkspace.data.entity.ModelEntity
import com.aiworkspace.data.entity.ProviderEntity
import com.aiworkspace.data.security.KeystoreEncryption
import kotlinx.coroutines.flow.Flow

class ProviderRepository(
    private val providerDao: ProviderDao,
    private val keystoreEncryption: KeystoreEncryption
) {
    fun getAllProviders(): Flow<List<ProviderEntity>> = providerDao.getAll()

    fun getAllEnabledProviders(): Flow<List<ProviderEntity>> = providerDao.getAllEnabled()

    suspend fun getProviderById(id: Int): ProviderEntity? = providerDao.getById(id)

    suspend fun addProvider(provider: ProviderEntity, plainApiKey: String): Int {
        val encryptedKey = keystoreEncryption.encrypt(plainApiKey)
        val encryptedProvider = provider.copy(apiKey = encryptedKey)
        return providerDao.insert(encryptedProvider).toInt()
    }

    suspend fun updateProvider(provider: ProviderEntity, plainApiKey: String? = null) {
        val updated = if (plainApiKey != null) {
            provider.copy(apiKey = keystoreEncryption.encrypt(plainApiKey))
        } else provider
        providerDao.update(updated)
    }

    suspend fun deleteProvider(provider: ProviderEntity) {
        providerDao.delete(provider)
    }

    fun getModels(providerId: Int): Flow<List<ModelEntity>> =
        providerDao.getModelsByProviderId(providerId)

    suspend fun saveModels(providerId: Int, models: List<ModelEntity>) {
        providerDao.deleteModelsByProviderId(providerId)
        providerDao.insertModels(models.map { it.copy(providerId = providerId) })
    }

    fun decryptApiKey(encryptedKey: ByteArray): String =
        keystoreEncryption.decrypt(encryptedKey)
}
