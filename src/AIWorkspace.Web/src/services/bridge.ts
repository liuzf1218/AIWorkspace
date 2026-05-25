import type { IpcMessage } from '@/types'

function generateUUID(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0
    const v = c === 'x' ? r : (r & 0x3) | 0x8
    return v.toString(16)
  })
}

type MessageHandler = (payload: any) => void

class Bridge {
  private handlers: Map<string, MessageHandler[]> = new Map()
  private pending: Map<string, { resolve: Function; reject: Function }> = new Map()

  constructor() {
    if (typeof window !== 'undefined' && (window as any).chrome?.webview) {
      ;(window as any).chrome.webview.addEventListener('message', (event: any) => {
        this.onMessage(event.data)
      })
    }
  }

  private onMessage(msg: IpcMessage) {
    if (msg.id && this.pending.has(msg.id)) {
      const pending = this.pending.get(msg.id)!
      this.pending.delete(msg.id)
      if (msg.error) {
        pending.reject(new Error(msg.error))
      } else {
        pending.resolve(msg.payload)
      }
      return
    }

    const handlers = this.handlers.get(msg.channel)
    if (handlers) {
      handlers.forEach((h) => h(msg.payload))
    }
  }

  send(channel: string, payload?: any): Promise<any> {
    return new Promise((resolve, reject) => {
      const id = generateUUID()
      this.pending.set(id, { resolve, reject })

      const msg: IpcMessage = { id, channel, payload }

      if (typeof window !== 'undefined' && (window as any).chrome?.webview) {
        ;(window as any).chrome.webview.postMessage(msg)
      } else {
        // Fallback for browser development
        console.log('[Bridge]', channel, payload)
        setTimeout(() => {
          this.pending.delete(id)
          resolve(null)
        }, 100)
      }

      // Timeout after 60 seconds
      setTimeout(() => {
        if (this.pending.has(id)) {
          this.pending.delete(id)
          reject(new Error('Request timeout'))
        }
      }, 60000)
    })
  }

  on(channel: string, handler: MessageHandler) {
    if (!this.handlers.has(channel)) {
      this.handlers.set(channel, [])
    }
    this.handlers.get(channel)!.push(handler)
    return () => this.off(channel, handler)
  }

  off(channel: string, handler: MessageHandler) {
    const handlers = this.handlers.get(channel)
    if (handlers) {
      const index = handlers.indexOf(handler)
      if (index > -1) handlers.splice(index, 1)
    }
  }
}

export const bridge = new Bridge()
