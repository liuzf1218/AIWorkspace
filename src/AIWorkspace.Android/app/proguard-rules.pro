# ProGuard rules for AI Workspace
# Add any project specific keep options here

# Keep serialized classes
-keepclassmembers class * {
    @kotlinx.serialization.SerialName <fields>;
}
-keep class com.aiworkspace.network.model.** { *; }
-keep class com.aiworkspace.data.entity.** { *; }

# Keep Room entities
-keep class * extends androidx.room.RoomDatabase
-keep @androidx.room.Entity class *
-keepclassmembers @androidx.room.Entity class * { <init>(...); }

# OkHttp
-dontwarn okhttp3.**
-dontwarn okio.**
