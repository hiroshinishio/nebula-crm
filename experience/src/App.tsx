import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { ThemeContext, useThemeProvider } from './hooks/useTheme'
import { useAuthEventHandler } from './features/auth/useAuthEventHandler'
import DashboardPage from './pages/DashboardPage'
import BrokerListPage from './pages/BrokerListPage'
import CreateBrokerPage from './pages/CreateBrokerPage'
import BrokerDetailPage from './pages/BrokerDetailPage'
import NotFoundPage from './pages/NotFoundPage'
import { UnauthorizedPage } from './pages/UnauthorizedPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
})

/**
 * AppInner is rendered inside BrowserRouter so hooks that need useNavigate
 * (useAuthEventHandler -> useSessionTeardown) are valid here.
 *
 * useAuthEventHandler subscribes to the auth event bus and triggers
 * session teardown when the API 401 interceptor fires.
 */
function AppInner() {
  useAuthEventHandler()

  return (
    <Routes>
      <Route path="/" element={<DashboardPage />} />
      <Route path="/brokers" element={<BrokerListPage />} />
      <Route path="/brokers/new" element={<CreateBrokerPage />} />
      <Route path="/brokers/:brokerId" element={<BrokerDetailPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

function App() {
  const themeValue = useThemeProvider()

  return (
    <ThemeContext.Provider value={themeValue}>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AppInner />
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeContext.Provider>
  )
}

export default App
