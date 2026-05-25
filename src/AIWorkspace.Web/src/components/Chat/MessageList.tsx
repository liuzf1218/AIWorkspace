import { useRef, useEffect } from 'react'
import { useChatStore } from '@/stores/useChatStore'
import MarkdownRenderer from '../Markdown/MarkdownRenderer'
import { User, Bot, Eye, Loader2 } from 'lucide-react'

export default function MessageList() {
  const { messages, isStreaming, isThinking, streamContent, screenshotData } = useChatStore()
  const bottomRef = useRef<HTMLDivElement>(null)

  const allMessages = [...messages]
  if (isStreaming) {
    allMessages.push({
      id: 'streaming',
      conversationId: '',
      role: 'assistant',
      content: streamContent,
      createdAt: new Date().toISOString(),
    })
  }

  // Streaming auto-scroll: only scroll when new content arrives
  useEffect(() => {
    if (!isStreaming || !streamContent) return
    requestAnimationFrame(() => {
      bottomRef.current?.scrollIntoView({ behavior: 'auto', block: 'nearest' })
    })
  }, [streamContent, isStreaming])

  return (
    <div>
      {allMessages.length === 0 && (
        <div className="flex flex-col items-center justify-center h-96 text-textSecondary px-4">
          <Bot size={48} className="mb-4 opacity-50" />
          <p className="text-lg font-medium">Welcome to AI Workspace</p>
          <p className="text-sm mt-2 text-center max-w-sm">
            Your technical analysis assistant. Analyze logs, errors, and screenshots with AI.
          </p>
          <div className="mt-6 space-y-2 text-xs text-textSecondary/80">
            <div className="flex items-center gap-2">
              <span className="w-5 h-5 rounded-full bg-accent/20 text-accent flex items-center justify-center text-[10px] font-bold">1</span>
              <span>Select a provider and model in the sidebar</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="w-5 h-5 rounded-full bg-accent/20 text-accent flex items-center justify-center text-[10px] font-bold">2</span>
              <span>Type your question or paste a log</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="w-5 h-5 rounded-full bg-accent/20 text-accent flex items-center justify-center text-[10px] font-bold">3</span>
              <span>Use the camera button to attach screenshots</span>
            </div>
          </div>
          {screenshotData && (
            <div className="mt-4 p-4 bg-surface rounded-lg border border-border">
              <p className="text-sm mb-2 flex items-center gap-2">
                <Eye size={14} />
                Screenshot captured (will be sent with next message)
              </p>
              <img
                src={`data:image/jpeg;base64,${screenshotData}`}
                alt="Screenshot"
                className="max-h-48 rounded border border-border"
              />
            </div>
          )}
        </div>
      )}

      {allMessages.map((msg, index) => {
        return (
          <div key={msg.id || index}>
            {msg.role === 'user' ? (
              /* User message — right aligned bubble */
              <div
                data-role="user"
                className="w-full max-w-3xl md:max-w-4xl mx-auto px-4 md:px-6 py-4"
              >
                <div className="flex gap-3 justify-end">
                  <div className="flex flex-col items-end gap-1 max-w-[85%]">
                    {msg.imageData && (
                      <img
                        src={`data:image/jpeg;base64,${msg.imageData}`}
                        alt="Attached"
                        className="max-h-48 rounded-lg mb-1 border border-border/50"
                      />
                    )}
                    <div className="bg-surfaceHover rounded-2xl rounded-tr-sm px-4 py-2.5">
                      <p className="whitespace-pre-wrap text-sm text-text leading-relaxed">{msg.content}</p>
                    </div>
                  </div>
                  <div className="w-7 h-7 rounded-full bg-accent flex items-center justify-center shrink-0 mt-0.5">
                    <User size={14} className="text-white" />
                  </div>
                </div>
              </div>
            ) : (
              /* Assistant message — left aligned, full width markdown */
              <div className="w-full max-w-3xl md:max-w-4xl mx-auto px-4 md:px-6 py-4">
                <div className="flex gap-3">
                  <div className="w-7 h-7 rounded-full bg-surface border border-border flex items-center justify-center shrink-0 mt-0.5">
                    <Bot size={14} className="text-textSecondary" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm text-text leading-relaxed">
                      {msg.id === 'streaming' && isThinking && !msg.content ? (
                        <div className="flex items-center gap-2 text-textSecondary">
                          <Loader2 size={14} className="animate-spin" />
                          <span>正在思考...</span>
                        </div>
                      ) : (
                        <MarkdownRenderer content={msg.content} />
                      )}
                      {msg.id === 'streaming' && !isThinking && (
                        <span className="inline-block w-2 h-4 ml-0.5 bg-accent animate-pulse align-middle" />
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        )
      })}

      <div ref={bottomRef} className="h-1" />
    </div>
  )
}
