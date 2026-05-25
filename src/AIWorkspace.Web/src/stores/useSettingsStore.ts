import { create } from 'zustand'
import { bridge } from '@/services/bridge'
import type { AppSettings } from '@/types'

interface SettingsState extends Partial<AppSettings> {
  loadSettings: () => Promise<void>
  setSetting: (key: string, value: string) => Promise<void>
}

const defaults: AppSettings = {
  theme: 'dark',
  windowWidth: 1280,
  windowHeight: 800,
  shortcutScreenshot: 'Ctrl+Shift+A',
  shortcutActivate: 'Ctrl+`',
  maxFileSizeMb: 10,
  maxImageLongEdge: 1536,
}

export const useSettingsStore = create<SettingsState>((set) => ({
  ...defaults,

  loadSettings: async () => {
    const settings = await bridge.send('settings:all')
    if (settings) {
      set((state) => ({
        ...state,
        ...settings,
      }))
    }
  },

  setSetting: async (key, value) => {
    await bridge.send('settings:set', { key, value })
    set((state) => ({ ...state, [key]: value }))
  },
}))
