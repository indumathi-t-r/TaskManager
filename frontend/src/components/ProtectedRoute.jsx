import { Navigate } from 'react-router-dom'
import { isLoggedIn } from '../services/authService'

// ProtectedRoute wraps any page that requires authentication.
// Usage in App.jsx:
//   <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
//
// How it works:
//   - If the user IS logged in → show the children (the wrapped page)
//   - If the user is NOT logged in → redirect them to /login automatically
//
// "children" is a special prop that React automatically fills with whatever
// is placed BETWEEN the opening and closing tags of a component.
// <ProtectedRoute>  ← opening tag
//   <Dashboard />   ← this becomes "children"
// </ProtectedRoute> ← closing tag

function ProtectedRoute({ children }) {
  if (!isLoggedIn()) {
    // <Navigate> is a React Router component that performs a redirect.
    // "replace" means the /login page replaces /dashboard in the browser history —
    // so pressing the browser Back button doesn't send them back to /dashboard.
    return <Navigate to="/login" replace />
  }

  // If logged in, render the actual page
  return children
}

export default ProtectedRoute
