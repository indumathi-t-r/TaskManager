import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { register } from '../services/authService'

function Register() {
  // useState creates a "state variable" — a value React watches.
  // When you call setUsername("Alice"), React re-renders this component
  // and the input field updates automatically.
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')        // Stores error messages to show the user
  const [success, setSuccess] = useState('')    // Stores success messages
  const [loading, setLoading] = useState(false) // Disables the button while request is in-flight

  // useNavigate() gives us a function to redirect the user programmatically
  // navigate('/login') takes the user to the Login page without a page reload
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    // e.preventDefault() stops the form from doing its default browser behavior
    // (which is to reload the page and send data in the URL). We handle it ourselves.
    e.preventDefault()

    setError('')    // Clear any previous errors
    setLoading(true) // Disable the button

    try {
      // Call our authService register function
      await register(username, email, password)

      setSuccess('Account created! Redirecting to login...')

      // Wait 1.5 seconds so the user can read the success message, then redirect
      setTimeout(() => navigate('/login'), 1500)
    } catch (err) {
      // Axios puts the server's error response in err.response.data
      // Our API returns: { message: "An account with this email already exists." }
      const message =
        err.response?.data?.message ||           // Server returned a message
        err.response?.data?.errors               // Validation errors object
          ? Object.values(err.response.data.errors).flat().join(' ')
          : 'Registration failed. Please try again.'
      setError(message)
    } finally {
      // "finally" runs whether the try succeeded or failed
      setLoading(false) // Re-enable the button
    }
  }

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1>Create Account</h1>
        <p className="auth-subtitle">Join Task Manager to organize your work</p>

        {/* Conditional rendering: only show error div if error is not empty */}
        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        {/* onSubmit calls our handleSubmit when the form is submitted (Enter key or button click) */}
        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            {/*
              Controlled input pattern:
              - value={username} makes React control what's displayed
              - onChange updates the state when the user types
              This keeps the input and state always in sync.
            */}
            <input
              id="username"
              type="text"
              placeholder="Your name"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              placeholder="At least 6 characters"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          {/* disabled={loading} greys out and disables the button while the API call is running */}
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Creating account...' : 'Register'}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Login here</Link>
        </p>
      </div>
    </div>
  )
}

export default Register
