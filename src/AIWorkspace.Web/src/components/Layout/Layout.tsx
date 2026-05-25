import { useEffect, useState } from 'react'
import { useChatStore } from '@/stores/useChatStore'
import { useProviderStore } from '@/stores/useProviderStore'
import { useSettingsStore } from '@/stores/useSettingsStore'
import { bridge } from '@/services/bridge'
import Sidebar from './Sidebar'
import ChatArea from '../Chat/ChatArea'
import ProviderModal from '../Settings/ProviderModal'
import SettingsModal from '../Settings/SettingsModal'

export default function Layout() {
  const [showProviderModal, setShowProviderModal] = useState(false)
  const [showSettingsModal, setShowSettingsModal] = useState(false)
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)

  const handleOpenProviders = () => {
    setShowSettingsModal(false)
    setShowProviderModal(true)
  }

  useEffect(() => {
    useChatStore.getState().loadConversations()
    useProviderStore.getState().loadProviders()
    useSettingsStore.getState().loadSettings().then(() => {
      // Ensure theme is synced after settings load
      const theme = useSettingsStore.getState().theme || 'dark'
      const html = document.documentElement
      if (!html.classList.contains(theme)) {
        html.classList.remove('dark', 'light')
        html.classList.add(theme)
        html.setAttribute('data-theme', theme)
      }
    })

    // Listen for stream events
    const unsubChunk = bridge.on('chat:chunk', (payload) => {
      useChatStore.getState().setIsThinking(false)
      useChatStore.getState().appendStream(payload)
    })
    const unsubDone = bridge.on('chat:done', () => {
      useChatStore.getState().finishStream()
    })
    const unsubError = bridge.on('chat:error', (payload) => {
      console.error('Chat error:', payload)
      useChatStore.getState().setError(payload || 'An error occurred during chat')
    })
    const unsubScreenshot = bridge.on('screenshot:ready', (payload) => {
      if (payload?.base64) {
        useChatStore.getState().setScreenshot(payload.base64)
      }
    })
    const unsubCommand = bridge.on('app:command', (payload) => {
      if (payload?.command === 'openSettings') {
        setShowSettingsModal(true)
      }
    })

    return () => {
      unsubChunk()
      unsubDone()
      unsubError()
      unsubScreenshot()
      unsubCommand()
    }
  }, [])

  return (
    <div className="flex h-screen bg-background text-text overflow-hidden">
      <div className={`transition-all duration-200 ease-in-out ${sidebarCollapsed ? 'w-0 overflow-hidden' : 'w-56'}`}>
        <Sidebar onToggleCollapse={() => setSidebarCollapsed(true)} />
      </div>
      <div className="flex-1 flex flex-col min-w-0">
        <ChatArea
          onToggleSidebar={() => setSidebarCollapsed(!sidebarCollapsed)}
          sidebarCollapsed={sidebarCollapsed}
          onOpenProviders={() => setShowProviderModal(true)}
          onOpenSettings={() => setShowSettingsModal(true)}
        />
      </div>

      {showProviderModal && <ProviderModal onClose={() => setShowProviderModal(false)} />}
      {showSettingsModal && <SettingsModal onClose={() => setShowSettingsModal(false)} onOpenProviders={handleOpenProviders} />}
    </div>
  )
}
