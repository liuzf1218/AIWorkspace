package com.aiworkspace.network.model

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

@Serializable
data class ChatRequest(
    val model: String,
    val messages: List<ChatMessageDto>,
    val temperature: Double = 0.7,
    @SerialName("max_tokens")
    val maxTokens: Int? = null,
    val stream: Boolean = true
)

@Serializable
data class ChatMessageDto(
    val role: String,
    val content: String,
    val images: List<ChatImageDto>? = null
)

@Serializable
data class ChatImageDto(
    @SerialName("type")
    val type: String = "image_url",
    @SerialName("image_url")
    val imageUrl: ImageUrlDto
)

@Serializable
data class ImageUrlDto(
    val url: String // data:image/jpeg;base64,...
)

@Serializable
data class ChatChunkDto(
    val id: String? = null,
    val choices: List<ChatChoiceDto>? = null
)

@Serializable
data class ChatChoiceDto(
    val delta: ChatDeltaDto? = null,
    @SerialName("finish_reason")
    val finishReason: String? = null
)

@Serializable
data class ChatDeltaDto(
    val role: String? = null,
    val content: String? = null
)

// Models list
@Serializable
data class ModelsResponseDto(
    val data: List<ModelDto> = emptyList()
)

@Serializable
data class ModelDto(
    val id: String,
    @SerialName("object")
    val objectType: String? = null
)
