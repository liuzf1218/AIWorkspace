import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { vs } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { Copy, Check } from 'lucide-react'
import { useState, useEffect } from 'react'

interface MarkdownRendererProps {
  content: string
}

export default function MarkdownRenderer({ content }: MarkdownRendererProps) {
  const isDark = useIsDark()
  return (
    <div className={`${isDark ? 'prose prose-invert' : 'prose'} prose-sm max-w-none`}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          code({ node, inline, className, children, ...props }: any) {
            const match = /language-(\w+)/.exec(className || '')
            const language = match ? match[1] : ''
            const codeString = String(children).replace(/\n$/, '')

            if (!inline && language) {
              return <CodeBlock code={codeString} language={language} />
            }

            return (
              <code className="text-accent font-medium px-1 py-0.5 rounded text-sm" {...props}>
                {children}
              </code>
            )
          },
          pre({ children }) {
            return <>{children}</>
          },
          table({ children }) {
            return (
              <div className="overflow-x-auto">
                <table className="border-collapse border border-border text-sm">
                  {children}
                </table>
              </div>
            )
          },
          th({ children }) {
            return (
              <th className="border border-border px-3 py-2 bg-surfaceHover text-left">
                {children}
              </th>
            )
          },
          td({ children }) {
            return (
              <td className="border border-border px-3 py-2">
                {children}
              </td>
            )
          },
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  )
}

function useIsDark() {
  const [isDark, setIsDark] = useState(() =>
    document.documentElement.classList.contains('dark')
  )

  useEffect(() => {
    const el = document.documentElement
    const observer = new MutationObserver(() => {
      setIsDark(el.classList.contains('dark'))
    })
    observer.observe(el, { attributes: true, attributeFilter: ['class'] })
    return () => observer.disconnect()
  }, [])

  return isDark
}

function CodeBlock({ code, language }: { code: string; language: string }) {
  const isDark = useIsDark()
  const [copied, setCopied] = useState(false)

  const handleCopy = async () => {
    try {
      if (navigator.clipboard && navigator.clipboard.writeText) {
        await navigator.clipboard.writeText(code)
      } else {
        const textarea = document.createElement('textarea')
        textarea.value = code
        textarea.style.position = 'fixed'
        textarea.style.opacity = '0'
        document.body.appendChild(textarea)
        textarea.select()
        document.execCommand('copy')
        document.body.removeChild(textarea)
      }
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Silently fail
    }
  }

  return (
    <div className={`relative group my-3 rounded-lg overflow-hidden border border-border ${isDark ? 'bg-codeBg' : 'bg-[#cbd5e1]'}`}>
      <div className={`flex items-center justify-between px-4 py-2 border-b border-border ${isDark ? 'bg-[#252526]' : 'bg-[#94a3b8]'}`}>
        <span className={`text-xs font-medium ${isDark ? 'text-textSecondary' : 'text-slate-700'}`}>{language}</span>
        <button
          onClick={handleCopy}
          className={`flex items-center gap-1.5 px-2 py-1 rounded text-xs transition-colors ${isDark ? 'text-textSecondary hover:text-text hover:bg-[#1e1e1e]' : 'text-slate-700 hover:text-slate-900 hover:bg-[#cbd5e1]'}`}
          title="Copy"
        >
          {copied ? <Check size={14} /> : <Copy size={14} />}
          <span>{copied ? 'Copied' : 'Copy'}</span>
        </button>
      </div>
      <SyntaxHighlighter
        language={language}
        style={isDark ? vscDarkPlus : vs}
        customStyle={{
          margin: 0,
          borderRadius: 0,
          fontSize: '13px',
          lineHeight: '1.6',
          overflowX: 'auto',
          background: isDark ? '#2a2a2a' : '#e2e8f0',
        }}
      >
        {code}
      </SyntaxHighlighter>
    </div>
  )
}
