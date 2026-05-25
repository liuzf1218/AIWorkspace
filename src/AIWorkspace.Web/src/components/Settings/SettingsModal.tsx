import { X, Monitor, Sun, Moon, Server, ChevronRight } from 'lucide-react'
import { useSettingsStore } from '@/stores/useSettingsStore'
import { useProviderStore } from '@/stores/useProviderStore'
import { useState, useEffect } from 'react'

interface SettingsModalProps {
  onClose: () => void
  onOpenProviders: () => void
}

export default function SettingsModal({ onClose, onOpenProviders }: SettingsModalProps) {
  const settings = useSettingsStore()
  const { providers, selectedProviderId, selectedModelId } = useProviderStore()
  const [theme, setTheme] = useState(settings.theme || 'dark')

  useEffect(() => {
    setTheme(settings.theme || 'dark')
  }, [settings.theme])

  const handleThemeChange = (newTheme: 'dark' | 'light') => {
    setTheme(newTheme)
    const html = document.documentElement
    html.classList.remove('dark', 'light')
    html.classList.add(newTheme)
    html.setAttribute('data-theme', newTheme)
  }

  const handleSave = async () => {
    await settings.setSetting('theme', theme)
    onClose()
  }

  const currentProvider = providers.find((p) => p.id === selectedProviderId)

  return (
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50">
      <div className="bg-surface border border-border rounded-xl w-[480px] shadow-2xl">
        <div className="flex items-center justify-between p-4 border-b border-border">
          <h2 className="text-lg font-medium">Settings</h2>
          <button onClick={onClose} className="p-1 rounded hover:bg-surfaceHover text-textSecondary">
            <X size={18} />
          </button>
        </div>

        <div className="p-5 space-y-6">
          {/* Device Style / Theme */}
          <div>
            <h3 className="text-sm font-medium mb-3 flex items-center gap-2">
              <Monitor size={14} className="text-textSecondary" />
              设备风格
            </h3>
            <div className="grid grid-cols-2 gap-3">
              <button
                onClick={() => handleThemeChange('dark')}
                className={`flex items-center gap-3 p-3 rounded-lg border transition-colors ${
                  theme === 'dark'
                    ? 'border-accent bg-accent/10 text-accent'
                    : 'border-border hover:border-textSecondary'
                }`}
              >
                <Moon size={18} />
                <div className="text-left">
                  <p className="text-sm font-medium">深色模式</p>
                  <p className="text-[11px] text-textSecondary">Dark</p>
                </div>
              </button>
              <button
                onClick={() => handleThemeChange('light')}
                className={`flex items-center gap-3 p-3 rounded-lg border transition-colors ${
                  theme === 'light'
                    ? 'border-accent bg-accent/10 text-accent'
                    : 'border-border hover:border-textSecondary'
                }`}
              >
                <Sun size={18} />
                <div className="text-left">
                  <p className="text-sm font-medium">浅色模式</p>
                  <p className="text-[11px] text-textSecondary">Light</p>
                </div>
              </button>
            </div>
          </div>

          {/* Model Configuration */}
          <div>
            <h3 className="text-sm font-medium mb-3 flex items-center gap-2">
              <Server size={14} className="text-textSecondary" />
              模型配置
            </h3>
            <div className="card space-y-3">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs text-textSecondary">当前 Provider</p>
                  <p className="text-sm font-medium">{currentProvider?.name || '未选择'}</p>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs text-textSecondary">当前模型</p>
                  <p className="text-sm font-medium">{selectedModelId || '未选择'}</p>
                </div>
              </div>
              <button
                onClick={() => {
                  onClose()
                  onOpenProviders()
                }}
                className="w-full flex items-center justify-between p-2.5 rounded-lg border border-border hover:border-accent hover:text-accent transition-colors text-sm"
              >
                <span>管理模型配置</span>
                <ChevronRight size={14} />
              </button>
            </div>
          </div>
        </div>

        <div className="flex gap-2 justify-end p-4 border-t border-border">
          <button onClick={onClose} className="btn-secondary">
            取消
          </button>
          <button onClick={handleSave} className="btn-primary">
            保存
          </button>
        </div>
      </div>
    </div>
  )
}
