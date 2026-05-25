import { create } from 'zustand'
import type { Conversation, Message } from '@/types'
import { bridge } from '@/services/bridge'

function generateUUID(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0
    const v = c === 'x' ? r : (r & 0x3) | 0x8
    return v.toString(16)
  })
}

interface ChatState {
  conversations: Conversation[]
  currentConversationId: string | null
  messages: Message[]
  isStreaming: boolean
  isThinking: boolean
  streamContent: string
  error: string | null
  screenshotData: string | null
  fileAttachment: { name: string; content: string; tokens: number } | null
  loadConversations: () => Promise<void>
  createConversation: (title?: string) => Promise<string>
  deleteConversation: (id: string) => Promise<void>
  selectConversation: (id: string) => Promise<void>
  sendMessage: (content: string, providerId?: number, modelId?: string) => Promise<void>
  abortChat: () => void
  setScreenshot: (data: string | null) => void
  setFileAttachment: (file: { name: string; content: string; tokens: number } | null) => void
  setIsThinking: (thinking: boolean) => void
  appendStream: (delta: string) => void
  finishStream: () => void
  exportConversation: (format?: 'markdown' | 'json') => Promise<{ success: boolean; path?: string; cancelled?: boolean }>
  setError: (error: string | null) => void
}

export const useChatStore = create<ChatState>((set, get) => ({
  conversations: [],
  currentConversationId: null,
  messages: [],
  isStreaming: false,
  isThinking: false,
  streamContent: '',
  error: null,
  screenshotData: null,
  fileAttachment: null,

  loadConversations: async () => {
    const conversations = await bridge.send('conversation:list')
    set({ conversations: conversations || [] })
  },

  createConversation: async (title = 'New Chat') => {
    const conv = await bridge.send('conversation:create', { title })
    const id = conv?.id
    if (id) {
      set((state) => ({
        conversations: [conv, ...state.conversations],
        currentConversationId: id,
        messages: [],
      }))
    }
    return id
  },

  deleteConversation: async (id) => {
    await bridge.send('conversation:delete', { id })
    set((state) => ({
      conversations: state.conversations.filter((c) => c.id !== id),
      currentConversationId: state.currentConversationId === id ? null : state.currentConversationId,
    }))
  },

  selectConversation: async (id) => {
    const messages = await bridge.send('conversation:load', { id })
    set({
      currentConversationId: id,
      messages: messages || [],
      streamContent: '',
    })
  },

  sendMessage: async (content: string, providerId?: number, modelId?: string) => {
    const { currentConversationId, screenshotData, fileAttachment } = get()
    if (!currentConversationId) {
      const newId = await get().createConversation()
      if (!newId) return
    }

    if (!providerId || !modelId) {
      alert('Please select a provider and model first')
      return
    }

    const convId = get().currentConversationId!

    // Optimistically add user message
    const userMsg: Message = {
      id: generateUUID(),
      conversationId: convId,
      role: 'user',
      content,
      imageData: screenshotData || undefined,
      createdAt: new Date().toISOString(),
    }
    set((state) => ({
      messages: [...state.messages, userMsg],
      isStreaming: true,
      isThinking: true,
      streamContent: '',
      error: null,
      screenshotData: null,
    }))

    try {
      await bridge.send('chat:send', {
        conversationId: convId,
        providerId,
        modelId,
        content,
        imageData: screenshotData,
        fileContent: fileAttachment?.content,
      })
    } catch (error) {
      console.error('Chat error:', error)
      set({ isStreaming: false, error: (error as Error)?.message || 'Chat failed' })
    } finally {
      set({ fileAttachment: null })
    }
  },

  abortChat: () => {
    bridge.send('chat:abort')
    set({ isStreaming: false, isThinking: false })
  },

  setIsThinking: (thinking) => set({ isThinking: thinking }),

  setScreenshot: (data) => set({ screenshotData: data }),

  setFileAttachment: (file) => set({ fileAttachment: file }),

  appendStream: (delta) =>
    set((state) => ({
      streamContent: state.streamContent + delta,
    })),

  finishStream: () =>
    set((state) => {
      if (!state.streamContent) return { isStreaming: false, isThinking: false, streamContent: '' }
      const assistantMsg: Message = {
        id: generateUUID(),
        conversationId: state.currentConversationId!,
        role: 'assistant',
        content: state.streamContent,
        createdAt: new Date().toISOString(),
      }
      return {
        messages: [...state.messages, assistantMsg],
        isStreaming: false,
        isThinking: false,
        streamContent: '',
        error: null,
      }
    }),

  exportConversation: async (format = 'markdown') => {
    const { currentConversationId } = get()
    if (!currentConversationId) {
      throw new Error('No conversation selected')
    }
    const result = await bridge.send('conversation:export', {
      conversationId: currentConversationId,
      format,
    })
    return result || { success: false }
  },

  setError: (error) => set({ error, isStreaming: false, isThinking: false }),
}))

