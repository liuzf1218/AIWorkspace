export interface Provider {
  id: number
  name: string
  apiBaseUrl: string
  apiKey: string
  proxyUrl?: string
  isEnabled: boolean
  supportsVision: boolean
  createdAt: string
  updatedAt: string
}

export interface ModelInfo {
  modelId: string
  displayName: string
  supportsVision: boolean
  isDefault: boolean
}

export interface Conversation {
  id: string
  title: string
  modelId?: string
  providerId?: number
  createdAt: string
  updatedAt: string
}

export interface Message {
  id: string
  conversationId: string
  role: 'user' | 'assistant' | 'system'
  content: string
  imageData?: string
  tokensUsed?: number
  createdAt: string
}

export interface Attachment {
  id: string
  messageId: string
  fileName: string
  fileSize: number
  contentText?: string
  createdAt: string
}

export interface IpcMessage {
  id: string
  channel: string
  payload?: any
  error?: string
}

export interface AppSettings {
  theme: string
  defaultProvider?: string
  defaultModel?: string
  windowWidth: number
  windowHeight: number
  shortcutScreenshot: string
  shortcutActivate: string
  maxFileSizeMb: number
  maxImageLongEdge: number
}
