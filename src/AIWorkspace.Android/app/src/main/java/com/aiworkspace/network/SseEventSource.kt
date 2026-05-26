package com.aiworkspace.network

import com.aiworkspace.network.model.ChatRequest
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.io.IOException
import java.util.concurrent.TimeUnit

class SseEventSource(
    private val okHttpClient: OkHttpClient = OkHttpClient.Builder()
        .connectTimeout(30, TimeUnit.SECONDS)
        .readTimeout(300, TimeUnit.SECONDS)
        .writeTimeout(30, TimeUnit.SECONDS)
        .build()
) {
    private val json = Json { ignoreUnknownKeys = true }

    suspend fun streamChat(
        request: ChatRequest,
        apiBaseUrl: String,
        apiKey: String,
        onChunk: (String) -> Unit,
        onError: (Throwable) -> Unit,
        onDone: () -> Unit
    ) {
        val jsonBody = json.encodeToString(ChatRequest.serializer(), request)

        val httpRequest = Request.Builder()
            .url("$apiBaseUrl/v1/chat/completions")
            .header("Authorization", "Bearer $apiKey")
            .header("Content-Type", "application/json")
            .header("Accept", "text/event-stream")
            .post(jsonBody.toRequestBody("application/json".toMediaType()))
            .build()

        withContext(Dispatchers.IO) {
            try {
                okHttpClient.newCall(httpRequest).execute().use { response ->
                    if (!response.isSuccessful) {
                        onError(IOException("HTTP ${response.code}: ${response.message}"))
                        return@use
                    }

                    val source = response.body?.source()
                        ?: run { onError(IOException("Empty response body")); return@use }

                    while (!source.exhausted()) {
                        val line = source.readUtf8Line() ?: continue
                        if (!line.startsWith("data: ")) continue

                        val data = line.substring(6)
                        if (data == "[DONE]") {
                            onDone()
                            break
                        }

                        try {
                            val chunk = json.decodeFromString(
                                com.aiworkspace.network.model.ChatChunkDto.serializer(),
                                data
                            )
                            val delta = chunk.choices?.firstOrNull()?.delta
                            delta?.content?.let(onChunk)
                            if (chunk.choices?.firstOrNull()?.finishReason != null) {
                                onDone()
                            }
                        } catch (_: Exception) {
                            // Skip malformed chunks
                        }
                    }
                }
            } catch (e: Exception) {
                onError(e)
            }
        }
    }

    suspend fun fetchModels(apiBaseUrl: String, apiKey: String): List<String> {
        val request = Request.Builder()
            .url("$apiBaseUrl/v1/models")
            .header("Authorization", "Bearer $apiKey")
            .build()

        return withContext(Dispatchers.IO) {
            okHttpClient.newCall(request).execute().use { response ->
                if (!response.isSuccessful) {
                    throw IOException("HTTP ${response.code}")
                }
                val body = response.body?.string()
                    ?: throw IOException("Empty response")
                val modelsResponse = json.decodeFromString(
                    com.aiworkspace.network.model.ModelsResponseDto.serializer(),
                    body
                )
                modelsResponse.data.map { it.id }
            }
        }
    }
}
