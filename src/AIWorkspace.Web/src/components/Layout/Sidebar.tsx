import { Plus, MessageSquare, Trash2, PanelLeftClose, User, Circle } from 'lucide-react'
import { useChatStore } from '@/stores/useChatStore'

interface SidebarProps {
  onToggleCollapse: () => void
}

export default function Sidebar({ onToggleCollapse }: SidebarProps) {
  const { conversations, currentConversationId, selectConversation, deleteConversation, createConversation } = useChatStore()

  return (
    <div className="w-56 bg-surface border-r border-border flex flex-col h-full">
      {/* Header */}
      <div className="p-3 border-b border-border flex items-center gap-2">
        <button
          onClick={() => createConversation()}
          className="btn-primary flex-1 flex items-center justify-center gap-2"
        >
          <Plus size={16} />
          New Chat
        </button>
        <button
          onClick={onToggleCollapse}
          title="Hide sidebar"
          className="p-2 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors"
        >
          <PanelLeftClose size={16} />
        </button>
      </div>

      {/* Conversation List */}
      <div className="flex-1 overflow-y-auto p-2">
        <div className="text-xs font-medium text-textSecondary px-2 py-1 mb-1">Recent</div>
        {conversations.length === 0 && (
          <div className="px-2 py-6 text-center">
            <p className="text-xs text-textSecondary">No conversations yet</p>
            <p className="text-[11px] text-textSecondary/60 mt-1">Click "New Chat" to start</p>
          </div>
        )}
        <div className="space-y-0.5">
          {conversations.map((conv) => (
            <div
              key={conv.id}
              className={`group flex items-center justify-between px-2 py-2 rounded-lg cursor-pointer transition-colors ${
                currentConversationId === conv.id
                  ? 'bg-surfaceHover border border-border'
                  : 'hover:bg-surfaceHover'
              }`}
            >
              <button
                onClick={() => selectConversation(conv.id)}
                className="flex items-center gap-2 flex-1 text-left overflow-hidden"
              >
                <MessageSquare size={14} className="text-textSecondary shrink-0" />
                <span className="text-sm truncate">{conv.title}</span>
              </button>
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  deleteConversation(conv.id)
                }}
                className="opacity-0 group-hover:opacity-100 p-1 rounded hover:bg-red-500/20 text-red-400 transition-all"
              >
                <Trash2 size={12} />
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* User Info Footer */}
      <div className="p-3 border-t border-border flex items-center gap-3">
        <div className="w-8 h-8 rounded-full bg-accent/20 flex items-center justify-center shrink-0">
          <User size={14} className="text-accent" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">Guest User</p>
          <div className="flex items-center gap-1.5">
            <Circle size={6} className="text-green-400 fill-green-400" />
            <span className="text-[11px] text-textSecondary">Online</span>
          </div>
        </div>
      </div>
    </div>
  )
}
