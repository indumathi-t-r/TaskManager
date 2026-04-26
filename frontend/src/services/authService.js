import axiosInstance from '../api/axiosInstance'

// register() sends the user's details to POST /api/auth/register
// It returns the response data (userId, username, email)
export const register = async (username, email, password) => {
  // axiosInstance.post(url, body) makes a POST request with JSON body
  // Axios automatically converts the JS object to JSON string
  const response = await axiosInstance.post('/api/auth/register', {
    username,   // shorthand for username: username
    email,
    password,
  })

  return response.data  // { message, userId, username, email }
}

// login() sends credentials to POST /api/auth/login
// On success, saves the JWT token and user info to localStorage
export const login = async (email, password) => {
  const response = await axiosInstance.post('/api/auth/login', {
    email,
    password,
  })

  const data = response.data  // { message, token, userId, username, email }

  // Save the token so axiosInstance interceptor can attach it to future requests
  // localStorage stores strings only, so we use JSON.stringify for objects
  localStorage.setItem('token', data.token)
  localStorage.setItem('user', JSON.stringify({
    userId: data.userId,
    username: data.username,
    email: data.email,
  }))

  return data
}

// logout() clears the saved token and user info from the browser
export const logout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
}

// getUser() reads the saved user object from localStorage
// Returns null if nobody is logged in
export const getUser = () => {
  const user = localStorage.getItem('user')
  return user ? JSON.parse(user) : null  // Parse the JSON string back to an object
}

// isLoggedIn() is a simple true/false check: do we have a token?
export const isLoggedIn = () => {
  return localStorage.getItem('token') !== null
}
