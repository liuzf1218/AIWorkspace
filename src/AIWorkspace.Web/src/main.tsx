import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'
import { bridge } from './services/bridge'

// Initialize theme before first render with a small delay to ensure WPF bridge is ready
async function initTheme() {
  const applyTheme = (theme: string) => {
    const html = document.documentElement
    html.classList.remove('dark', 'light')
    html.classList.add(theme)
    html.setAttribute('data-theme', theme)
  }

  // Wait briefly for WPF bridge to be ready
  await new Promise(resolve => setTimeout(resolve, 300))

  try {
    const settings = await bridge.send('settings:all')
    const theme = settings?.theme || 'dark'
    applyTheme(theme)
  } catch {
    applyTheme('dark')
  }
}

initTheme().then(() => {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>,
  )
})
