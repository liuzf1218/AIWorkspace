import ErrorBoundary from './components/ErrorBoundary/ErrorBoundary'
import Layout from './components/Layout/Layout'

function App() {
  return (
    <ErrorBoundary>
      <Layout />
    </ErrorBoundary>
  )
}

export default App
