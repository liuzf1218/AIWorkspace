import { useState, useEffect } from 'react'
import { X, Plus, Trash2, Check, AlertCircle, Loader2 } from 'lucide-react'
import { useProviderStore } from '@/stores/useProviderStore'
import type { Provider } from '@/types'

interface ProviderModalProps {
  onClose: () => void
}

export default function ProviderModal({ onClose }: ProviderModalProps) {
  const { providers, loadProviders, saveProvider, deleteProvider } = useProviderStore()
  const [editing, setEditing] = useState<Partial<Provider>>({})
  const [isEditing, setIsEditing] = useState(false)
  const [validating, setValidating] = useState<number | null>(null)
  const [validationResult, setValidationResult] = useState<{ id: number; valid: boolean } | null>(null)

  useEffect(() => {
    loadProviders()
  }, [])

  const handleSave = async () => {
    if (!editing.name || !editing.apiBaseUrl || !editing.apiKey) {
      alert('Name, API Base URL, and API Key are required')
      return
    }
    await saveProvider(editing as Provider)
    setIsEditing(false)
    setEditing({})
  }

  const handleValidate = async (provider: Provider) => {
    setValidating(provider.id)
    setValidationResult(null)
    try {
      const { bridge } = await import('@/services/bridge')
      const result = await bridge.send('provider:validate', { id: provider.id })
      setValidationResult({ id: provider.id, valid: result?.valid || false })
    } finally {
      setValidating(null)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50">
      <div className="bg-surface border border-border rounded-xl w-[600px] max-h-[80vh] flex flex-col shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <h2 className="text-lg font-medium">Providers</h2>
          <button onClick={onClose} className="p-1 rounded hover:bg-surfaceHover text-textSecondary">
            <X size={18} />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4">
          {providers.map((provider) => (
            <div
              key={provider.id}
              className="card mb-3 group"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <h3 className="font-medium">{provider.name}</h3>
                    {provider.supportsVision && (
                      <span className="text-[10px] bg-accent/20 text-accent px-1.5 py-0.5 rounded">
                        Vision
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-textSecondary mt-1">{provider.apiBaseUrl}</p>
                  {provider.proxyUrl && (
                    <p className="text-xs text-textSecondary">Proxy: {provider.proxyUrl}</p>
                  )}
                </div>
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleValidate(provider)}
                    disabled={validating === provider.id}
                    className="p-1.5 rounded hover:bg-surfaceHover text-textSecondary hover:text-text transition-colors"
                    title="Test connection"
                  >
                    {validating === provider.id ? (
                      <Loader2 size={14} className="animate-spin" />
                    ) : validationResult?.id === provider.id ? (
                      validationResult.valid ? (
                        <Check size={14} className="text-green-400" />
                      ) : (
                        <AlertCircle size={14} className="text-red-400" />
                      )
                    ) : (
                      <Check size={14} />
                    )}
                  </button>
                  <button
                    onClick={() => {
                      setEditing({ ...provider })
                      setIsEditing(true)
                    }}
                    className="p-1.5 rounded hover:bg-surfaceHover text-textSecondary hover:text-text transition-colors"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => {
                      if (confirm(`Delete provider "${provider.name}"?`)) {
                        deleteProvider(provider.id)
                      }
                    }}
                    className="p-1.5 rounded hover:bg-red-500/20 text-textSecondary hover:text-red-400 transition-colors"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>
            </div>
          ))}

          {providers.length === 0 && (
            <div className="text-center py-8 text-textSecondary">
              <p>No providers configured</p>
              <p className="text-sm mt-1">Add your first AI provider</p>
            </div>
          )}
        </div>

        {/* Add / Edit Form */}
        {isEditing && (
          <div className="border-t border-border p-4 space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-textSecondary block mb-1">Provider Name（平台名称）</label>
                <input
                  value={editing.name || ''}
                  onChange={(e) => setEditing({ ...editing, name: e.target.value })}
                  placeholder="如：OpenAI、DeepSeek 官方、老张 API"
                  className="input-field w-full"
                />
                <p className="text-[10px] text-textSecondary/60 mt-1">平台名称是服务商标识，不是具体模型名。模型会在添加后自动拉取。</p>
              </div>
              <div>
                <label className="text-xs text-textSecondary block mb-1">API Base URL</label>
                <input
                  value={editing.apiBaseUrl || ''}
                  onChange={(e) => setEditing({ ...editing, apiBaseUrl: e.target.value })}
                  placeholder="https://api.openai.com/v1"
                  className="input-field w-full"
                />
              </div>
            </div>
            <div>
              <label className="text-xs text-textSecondary block mb-1">API Key</label>
              <input
                type="password"
                value={editing.apiKey || ''}
                onChange={(e) => setEditing({ ...editing, apiKey: e.target.value })}
                placeholder="sk-..."
                className="input-field w-full"
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-textSecondary block mb-1">Proxy URL (optional)</label>
                <input
                  value={editing.proxyUrl || ''}
                  onChange={(e) => setEditing({ ...editing, proxyUrl: e.target.value || undefined })}
                  placeholder="http://127.0.0.1:7890"
                  className="input-field w-full"
                />
              </div>
              <div className="flex items-center gap-4 pt-5">
                <label className="flex items-center gap-2 text-sm cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editing.supportsVision || false}
                    onChange={(e) => setEditing({ ...editing, supportsVision: e.target.checked })}
                    className="rounded border-border bg-surface"
                  />
                  Supports Vision
                </label>
                <label className="flex items-center gap-2 text-sm cursor-pointer">
                  <input
                    type="checkbox"
                    checked={editing.isEnabled !== false}
                    onChange={(e) => setEditing({ ...editing, isEnabled: e.target.checked })}
                    className="rounded border-border bg-surface"
                  />
                  Enabled
                </label>
              </div>
            </div>
            <div className="flex gap-2 justify-end pt-2">
              <button
                onClick={() => {
                  setIsEditing(false)
                  setEditing({})
                }}
                className="btn-secondary"
              >
                Cancel
              </button>
              <button onClick={handleSave} className="btn-primary">
                Save
              </button>
            </div>
          </div>
        )}

        {!isEditing && (
          <div className="border-t border-border p-4">
            <button
              onClick={() => {
                setEditing({ isEnabled: true, supportsVision: false })
                setIsEditing(true)
              }}
              className="btn-primary w-full flex items-center justify-center gap-2"
            >
              <Plus size={16} />
              Add Provider
            </button>
          </div>
        )}
      </div>
    </div>
  )
}
