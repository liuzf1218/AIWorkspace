import { useState } from 'react'
import { Send, Camera, FileText, X, Loader2, Square } from 'lucide-react'
import { useChatStore } from '@/stores/useChatStore'
import { useProviderStore } from '@/stores/useProviderStore'
import { bridge } from '@/services/bridge'

export default function ChatInput() {
  const [input, setInput] = useState('')
  const [isUploading, setIsUploading] = useState(false)

  const {
    isStreaming,
    sendMessage,
    abortChat,
    screenshotData,
    setScreenshot,
    fileAttachment,
    setFileAttachment,
  } = useChatStore()

  const { selectedProviderId, selectedModelId } = useProviderStore()

  const handleSend = () => {
    if (!input.trim() && !screenshotData && !fileAttachment) return
    if (!selectedProviderId || !selectedModelId) return
    sendMessage(input, selectedProviderId, selectedModelId)
    setInput('')
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleScreenshot = () => {
    bridge.send('screenshot:capture')
  }

  const handleFileOpen = async () => {
    setIsUploading(true)
    try {
      const result = await bridge.send('file:open')
      if (result) {
        setFileAttachment({
          name: result.name,
          content: result.content,
          tokens: result.tokens,
        })
      }
    } catch (error) {
      console.error('File upload error:', error)
      alert('Failed to open file: ' + (error as Error).message)
    } finally {
      setIsUploading(false)
    }
  }

  const canSend = selectedProviderId && selectedModelId

  return (
    <div className="p-4 pt-3 shrink-0">
      {/* Attachments */}
      {(screenshotData || fileAttachment) && (
        <div className="flex gap-2 mb-3">
          {screenshotData && (
            <div className="relative inline-block">
              <img
                src={`data:image/jpeg;base64,${screenshotData}`}
                alt="Screenshot"
                className="h-16 w-auto rounded border border-border"
              />
              <button
                onClick={() => setScreenshot(null)}
                className="absolute -top-1 -right-1 bg-red-500 rounded-full p-0.5 text-white"
              >
                <X size={10} />
              </button>
            </div>
          )}
          {fileAttachment && (
            <div className="flex items-center gap-2 bg-surfaceHover border border-border rounded px-3 py-1.5 text-sm">
              <FileText size={14} />
              <span className="truncate max-w-[200px]">{fileAttachment.name}</span>
              <span className="text-textSecondary text-xs">({fileAttachment.tokens} tokens)</span>
              <button
                onClick={() => setFileAttachment(null)}
                className="ml-1 text-red-400 hover:text-red-300"
              >
                <X size={12} />
              </button>
            </div>
          )}
        </div>
      )}

      {/* Input Area — aligned with MessageList */}
      <div className="w-full max-w-3xl md:max-w-4xl mx-auto px-4 md:px-6">
        <div className="flex gap-2 border border-border rounded-xl px-3 py-2 bg-background">
          <div className="flex gap-1">
            <button
              onClick={handleScreenshot}
              title="Screenshot"
              className="p-2 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors"
            >
              <Camera size={18} />
            </button>
            <button
              onClick={handleFileOpen}
              title="Upload file"
              className="p-2 rounded-lg text-textSecondary hover:text-text hover:bg-surfaceHover transition-colors"
              disabled={isUploading}
            >
              {isUploading ? <Loader2 size={18} className="animate-spin" /> : <FileText size={18} />}
            </button>
          </div>

          <div className="flex-1 relative">
            <textarea
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={canSend ? 'Type your message...' : 'Select a provider and model first'}
              disabled={!canSend || isStreaming}
              rows={1}
              className="w-full resize-none min-h-[40px] max-h-[200px] py-2.5 pr-10 bg-transparent text-sm text-text placeholder-textSecondary outline-none"
              style={{ height: 'auto' }}
            />
          </div>

          {isStreaming ? (
            <button
              onClick={abortChat}
              className="p-2 rounded-lg bg-red-500/20 text-red-400 hover:bg-red-500/30 transition-colors"
            >
              <Square size={18} />
            </button>
          ) : (
            <button
              onClick={handleSend}
              disabled={(!input.trim() && !screenshotData && !fileAttachment) || !canSend}
              className="btn-primary p-2.5 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Send size={18} />
            </button>
          )}
        </div>

        {/* Provider hint removed */}
      </div>
    </div>
  )
}
