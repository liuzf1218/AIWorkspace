/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        background: 'var(--background)',
        surface: 'var(--surface)',
        surfaceHover: 'var(--surfaceHover)',
        border: 'var(--border)',
        text: 'var(--text)',
        textSecondary: 'var(--textSecondary)',
        accent: 'var(--accent)',
        accentHover: 'var(--accentHover)',
        userBubble: 'var(--userBubble)',
        assistantBubble: 'var(--assistantBubble)',
        codeBg: 'var(--codeBg)',
      },
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
  ],
}
