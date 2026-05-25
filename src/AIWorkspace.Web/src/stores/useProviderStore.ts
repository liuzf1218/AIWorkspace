import { create } from 'zustand'
import type { Provider, ModelInfo } from '@/types'
import { bridge } from '@/services/bridge'

interface ProviderState {
  providers: Provider[]
  models: ModelInfo[]
  selectedProviderId: number | null
  selectedModelId: string | null
  isLoading: boolean
  loadProviders: () => Promise<void>
  loadModels: (providerId: number) => Promise<void>
  saveProvider: (provider: Provider) => Promise<void>
  deleteProvider: (id: number) => Promise<void>
  selectProvider: (id: number | null) => void
  selectModel: (id: string | null) => void
}

export const useProviderStore = create<ProviderState>((set, get) => ({
  providers: [],
  models: [],
  selectedProviderId: null,
  selectedModelId: null,
  isLoading: false,

  loadProviders: async () => {
    set({ isLoading: true })
    try {
      const providers = await bridge.send('provider:list')
      set({ providers: providers || [] })
      if (providers?.length > 0 && !get().selectedProviderId) {
        set({ selectedProviderId: providers[0].id })
      }
    } finally {
      set({ isLoading: false })
    }
  },

  loadModels: async (providerId: number) => {
    set({ isLoading: true })
    try {
      const models = await bridge.send('provider:models', { id: providerId })
      set({ models: models || [] })
    } finally {
      set({ isLoading: false })
    }
  },

  saveProvider: async (provider) => {
    const result = await bridge.send('provider:save', provider)
    await get().loadProviders()
    if (result?.id) {
      set({ selectedProviderId: result.id })
      await get().loadModels(result.id)
    }
  },

  deleteProvider: async (id) => {
    await bridge.send('provider:delete', { id })
    await get().loadProviders()
    const { providers } = get()
    if (providers.length > 0) {
      set({ selectedProviderId: providers[0].id })
    } else {
      set({ selectedProviderId: null, selectedModelId: null })
    }
  },

  selectProvider: (id) => {
    set({ selectedProviderId: id, selectedModelId: null })
    if (id) {
      get().loadModels(id)
    }
  },

  selectModel: (id) => {
    set({ selectedModelId: id })
  },
}))
