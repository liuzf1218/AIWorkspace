import { useState, useEffect, useRef } from 'react'
import { useChatStore } from '@/stores/useChatStore'
import { useProviderStore } from '@/stores/useProviderStore'
import { AlertCircle, PanelLeftOpen, Settings, Server, ChevronDown, Search, MoreVertical, Download, FileJson } from 'lucide-react'
import MessageList from './MessageList'
import ChatInput from './ChatInput'

interface ChatAreaProps {
  onToggleSidebar: () => void
  sidebarCollapsed: boolean
  onOpenProviders: () => void
  onOpenSettings: () => void
}

export default function ChatArea({ onToggleSidebar, sidebarCollapsed, onOpenProviders, onOpenSettings }: ChatAreaProps) {
  const { currentConversationId, messages, error, setError, exportConversation } = useChatStore()
  const { providers, selectedProviderId, selectedModelId, selectProvider, selectModel, models } = useProviderStore()

  const [showProviderDropdown, setShowProviderDropdown] = useState(false)
  const [showModelDropdown, setShowModelDropdown] = useState(false)
  const [showExportMenu, setShowExportMenu] = useState(false)
  const [modelSearch, setModelSearch] = useState('')

  const messagesScrollRef = useRef<HTMLDivElement>(null)
  const providerRef = useRef<HTMLDivElement>(null)
  const modelRef = useRef<HTMLDivElement>(null)
  const exportRef = useRef<HTMLDivElement>(null)
  const prevMsgLength = useRef(0)

  // Scroll to last user message when new message is sent
  useEffect(() => {
    const currentLength = messages.length
    if (currentLength > prevMsgLength.current) {
      const lastMsg = messages[currentLength - 1]
      if (lastMsg?.role === 'user') {
        const isFirstMessage = currentLength === 1
        setTimeout(() => {
          const container = document.getElementById('chat-scroll-container') as HTMLDivElement | null
          if (!container) return
          if (isFirstMessage) {
            container.scrollTop = 0
          } else {
            const userMsgs = container.querySelectorAll('[data-role="user"]')
            const lastUserMsg = userMsgs[userMsgs.length - 1] as HTMLElement | undefined
            lastUserMsg?.scrollIntoView({ behavior: 'auto', block: 'start' })
          }
        }, 100)
      }
    }
    prevMsgLength.current = currentLength
  }, [messages.length])

  useEffect(() => {
    setModelSearch('')
  }, [selectedProviderId])

  // Close dropdowns when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (providerRef.current && !providerRef.current.contains(event.target as Node)) {
        setShowProviderDropdown(false)
      }
      if (modelRef.current && !modelRef.current.contains(event.target as Node)) {
        setShowModelDropdown(false)
      }
      if (exportRef.current && !exportRef.current.contains(event.target as Node)) {
        setShowExportMenu(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const currentProvider = providers.find((p) => p.id === selectedProviderId)
  const filteredModels = models.filter((m) => m.modelId.toLowerCase().includes(modelSearch.toLowerCase()))

  return (
    <div className="flex flex-col h-full">
      {/* Header / Menu Bar */}
      <div className="h-14 border-b border-border flex items-center px-4 shrink-0 justify-between gap-3 bg-background">
        {/* Left: sidebar toggle + conversation title */}
        <div className="flex items-center gap-2 min-w-0 flex-1">
          {sidebarCollapsed && (
            <button
              onClick={onToggleSidebar}
              title="Show sidebar"
              className="p-1.5 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors shrink-0"
            >
              <PanelLeftOpen size={16} />
            </button>
          )}
          <h2 className="text-sm font-medium text-text truncate">
            {currentConversationId
              ? useChatStore.getState().messages[0]?.content.slice(0, 50) || 'Chat'
              : 'Select or create a conversation'}
          </h2>
        </div>

        {/* Center: Provider + Model selectors */}
        <div className="flex items-center gap-2 shrink-0">
          {/* Provider Dropdown */}
          <div className="relative" ref={providerRef}>
            <button
              onClick={() => { setShowProviderDropdown(!showProviderDropdown); setShowModelDropdown(false) }}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors border border-border bg-surface"
            >
              <Server size={13} />
              <span className="truncate max-w-[100px]">{currentProvider?.name || 'Provider'}</span>
              <ChevronDown size={12} className={`transition-transform ${showProviderDropdown ? 'rotate-180' : ''}`} />
            </button>
            {showProviderDropdown && (
              <div className="absolute top-full left-0 mt-1 w-52 bg-surface border border-border rounded-lg shadow-xl z-50 py-1">
                {providers.map((provider) => (
                  <button
                    key={provider.id}
                    onClick={() => { selectProvider(provider.id); setShowProviderDropdown(false) }}
                    className={`w-full text-left px-3 py-2 text-xs transition-colors ${
                      selectedProviderId === provider.id
                        ? 'bg-accent/20 text-accent'
                        : 'text-textSecondary hover:text-text hover:bg-surfaceHover'
                    }`}
                  >
                    {provider.name}
                  </button>
                ))}
                <div className="border-t border-border mt-1 pt-1">
                  <button
                    onClick={() => { onOpenProviders(); setShowProviderDropdown(false) }}
                    className="w-full text-left px-3 py-2 text-xs text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors"
                  >
                    Manage Providers...
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Model Dropdown */}
          <div className="relative" ref={modelRef}>
            <button
              onClick={() => { setShowModelDropdown(!showModelDropdown); setShowProviderDropdown(false) }}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors border border-border bg-surface"
            >
              <span className="truncate max-w-[140px]">{selectedModelId || 'Model'}</span>
              <ChevronDown size={12} className={`transition-transform ${showModelDropdown ? 'rotate-180' : ''}`} />
            </button>
            {showModelDropdown && (
              <div className="absolute top-full right-0 mt-1 w-64 bg-surface border border-border rounded-lg shadow-xl z-50 py-1">
                <div className="px-2 py-1.5 border-b border-border">
                  <div className="flex items-center gap-1.5 px-2 py-1 bg-background rounded border border-border">
                    <Search size={12} className="text-textSecondary" />
                    <input
                      type="text"
                      placeholder="Search models..."
                      value={modelSearch}
                      onChange={(e) => setModelSearch(e.target.value)}
                      className="bg-transparent text-xs text-text placeholder-textSecondary outline-none w-full"
                      autoFocus
                    />
                  </div>
                </div>
                <div className="max-h-60 overflow-y-auto py-1">
                  {filteredModels.length === 0 && (
                    <div className="px-3 py-2 text-xs text-textSecondary">No models found</div>
                  )}
                  {filteredModels.map((model) => (
                    <button
                      key={model.modelId}
                      onClick={() => { selectModel(model.modelId); setShowModelDropdown(false) }}
                      className={`w-full text-left px-3 py-2 text-xs transition-colors ${
                        selectedModelId === model.modelId
                          ? 'bg-accent/20 text-accent'
                          : 'text-textSecondary hover:text-text hover:bg-surfaceHover'
                      }`}
                    >
                      <span className="truncate block">{model.modelId}</span>
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Right: Export + Settings */}
        <div className="flex items-center gap-1 shrink-0">
          <div className="relative" ref={exportRef}>
            <button
              onClick={() => setShowExportMenu(!showExportMenu)}
              disabled={!currentConversationId}
              title="Export conversation"
              className="p-1.5 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
            >
              <MoreVertical size={16} />
            </button>
            {showExportMenu && (
              <div className="absolute top-full right-0 mt-1 w-48 bg-surface border border-border rounded-lg shadow-xl z-50 py-1">
                <div className="px-3 py-1.5 text-[10px] text-textSecondary uppercase tracking-wider">Export</div>
                <button
                  onClick={async () => {
                    setShowExportMenu(false)
                    try {
                      const result = await exportConversation('markdown')
                      if (result?.success) console.log('Exported to', result.path)
                    } catch (e: any) {
                      alert(e?.message || 'Export failed')
                    }
                  }}
                  className="w-full text-left px-3 py-2 text-xs text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors flex items-center gap-2"
                >
                  <Download size={13} />
                  Export as Markdown
                </button>
                <button
                  onClick={async () => {
                    setShowExportMenu(false)
                    try {
                      const result = await exportConversation('json')
                      if (result?.success) console.log('Exported to', result.path)
                    } catch (e: any) {
                      alert(e?.message || 'Export failed')
                    }
                  }}
                  className="w-full text-left px-3 py-2 text-xs text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors flex items-center gap-2"
                >
                  <FileJson size={13} />
                  Export as JSON
                </button>
              </div>
            )}
          </div>
          <button
            onClick={onOpenSettings}
            title="Settings"
            className="p-1.5 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors"
          >
            <Settings size={16} />
          </button>
        </div>
      </div>

      {/* Chat Container: Messages + Input unified */}
      <div className="flex-1 min-h-0 flex flex-col mx-4 mb-4 bg-surface rounded-xl shadow-sm overflow-hidden">
        <div id="chat-scroll-container" ref={messagesScrollRef} className="flex-1 overflow-y-auto">
          <MessageList />
        </div>

        {/* Error Banner */}
        {error && (
          <div className="shrink-0 mx-4 mb-2 p-3 bg-red-500/10 border border-red-500/30 rounded-lg flex items-start gap-2">
            <AlertCircle size={16} className="text-red-400 shrink-0 mt-0.5" />
            <div className="flex-1 min-w-0">
              <p className="text-sm text-red-300">{error}</p>
            </div>
            <button
              onClick={() => setError(null)}
              className="text-xs text-red-400 hover:text-red-300 shrink-0"
            >
              Dismiss
            </button>
          </div>
        )}

        <ChatInput />
      </div>
    </div>
  )
}
