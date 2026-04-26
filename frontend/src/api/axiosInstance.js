import axios from 'axios'

// axios.create() makes a custom Axios "client" pre-configured for our backend.
// Instead of writing the full URL every time (http://localhost:5000/api/task),
// we only write the path (/api/task) and Axios adds the baseURL automatically.
const axiosInstance = axios.create({
  // ⚠️  THIS IS WHERE YOUR BACKEND URL GOES.
  // If your dotnet run shows "Now listening on: http://localhost:5001", change 5000 → 5001
  baseURL: 'http://localhost:5000',

  headers: {
    'Content-Type': 'application/json',  // Tell the API we're sending JSON
  },
})

// An "interceptor" is a function that runs BEFORE every request is sent.
// Think of it as a checkpoint that adds the JWT token automatically.
// Without this, you'd have to manually add the Authorization header in every API call.
axiosInstance.interceptors.request.use(
  (config) => {
    // Read the token from localStorage
    // localStorage is a browser built-in key-value store that persists across page refreshes
    const token = localStorage.getItem('token')

    if (token) {
      // Add the token to the Authorization header
      // Format: "Bearer eyJhbGci..." — your ASP.NET API reads this header
      config.headers.Authorization = `Bearer ${token}`
    }

    return config  // Always return the config, or the request won't be sent
  },
  (error) => {
    // If something goes wrong building the request, reject it
    return Promise.reject(error)
  }
)

// An interceptor can also watch RESPONSES.
// Here we check if any response comes back as 401 Unauthorized —
// that means the token expired. We log the user out automatically.
axiosInstance.interceptors.response.use(
  (response) => response,  // If response is fine, just pass it through
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid — clear storage and redirect to login
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default axiosInstance
