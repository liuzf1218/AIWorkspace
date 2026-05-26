package com.aiworkspace.utils

import android.content.Context
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.net.Uri
import android.util.Base64
import androidx.core.content.FileProvider
import java.io.ByteArrayOutputStream
import java.io.File
import java.io.FileOutputStream

object ImageUtils {

    private const val MAX_IMAGE_LONG_EDGE = 1536
    private const val JPEG_QUALITY = 85

    /**
     * Create a temporary image file and return its FileProvider URI for camera capture.
     */
    fun createImageUri(context: Context): Uri? {
        return try {
            val file = File.createTempFile("camera_", ".jpg", context.cacheDir)
            FileProvider.getUriForFile(
                context,
                "${context.packageName}.fileprovider",
                file
            )
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }

    /**
     * Convert an image URI to a base64 data URL string.
     * Resizes if the long edge exceeds MAX_IMAGE_LONG_EDGE.
     */
    fun uriToBase64(context: Context, uri: Uri): String? {
        return try {
            val bitmap = loadBitmap(context, uri) ?: return null
            val resized = resizeBitmap(bitmap, MAX_IMAGE_LONG_EDGE)
            val output = ByteArrayOutputStream()
            resized.compress(Bitmap.CompressFormat.JPEG, JPEG_QUALITY, output)
            val bytes = output.toByteArray()
            val base64 = Base64.encodeToString(bytes, Base64.NO_WRAP)
            "data:image/jpeg;base64,$base64"
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }

    /**
     * Load a Bitmap from a URI, handling rotation if needed.
     */
    private fun loadBitmap(context: Context, uri: Uri): Bitmap? {
        return context.contentResolver.openInputStream(uri)?.use { stream ->
            BitmapFactory.decodeStream(stream)
        }
    }

    /**
     * Resize bitmap so its longest edge is at most maxEdge pixels.
     */
    private fun resizeBitmap(bitmap: Bitmap, maxEdge: Int): Bitmap {
        val width = bitmap.width
        val height = bitmap.height
        val maxDim = maxOf(width, height)
        if (maxDim <= maxEdge) return bitmap

        val scale = maxEdge.toFloat() / maxDim
        val newWidth = (width * scale).toInt()
        val newHeight = (height * scale).toInt()
        return Bitmap.createScaledBitmap(bitmap, newWidth, newHeight, true)
    }

    /**
     * Clean up temporary camera image files from cache.
     */
    fun cleanupTempImages(context: Context) {
        context.cacheDir.listFiles { _, name -> name.startsWith("camera_") && name.endsWith(".jpg") }
            ?.forEach { it.delete() }
    }
}
