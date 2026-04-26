import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Register from './pages/Register'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import ProtectedRoute from './components/ProtectedRoute'
import './App.css'

// App is the ROOT component — the very top of your component tree.
// It sets up the Router which watches the browser URL and decides what to show.
function App() {
  return (
    // BrowserRouter: provides the routing context to everything inside it.
    // It watches the browser's URL bar and re-renders when the URL changes.
    <BrowserRouter>
      {/*
        Routes: a container that looks at the current URL and renders the
        first matching <Route> child.
      */}
      <Routes>
        {/* / → redirect to /login */}
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* /register → show the Register page */}
        <Route path="/register" element={<Register />} />

        {/* /login → show the Login page */}
        <Route path="/login" element={<Login />} />

        {/*
          /dashboard → wrapped in ProtectedRoute.
          ProtectedRoute checks if the user is logged in.
          If yes → show Dashboard.
          If no → redirect to /login.
        */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />

        {/* Catch-all: any unknown URL → redirect to /login */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
