import axiosInstance from '../api/axiosInstance'

// ⚠️  ENDPOINT NOTE:
// Your ASP.NET controller is named "TaskController" → route is /api/task (singular)
// If you ever rename the controller, update the path here.
const BASE = '/api/task'

// GET /api/task — fetch all tasks for the logged-in user
// The JWT token is attached automatically by the axiosInstance interceptor
export const getAllTasks = async () => {
  const response = await axiosInstance.get(BASE)
  return response.data  // Returns an array: [{ id, title, description, isCompleted, createdAt }, ...]
}

// POST /api/task — create a new task
export const createTask = async (title, description) => {
  const response = await axiosInstance.post(BASE, {
    title,
    description,
  })
  return response.data  // Returns the created task object
}

// PUT /api/task/{id} — update an existing task
export const updateTask = async (id, title, description, isCompleted) => {
  const response = await axiosInstance.put(`${BASE}/${id}`, {
    title,
    description,
    isCompleted,
  })
  return response.data  // Returns the updated task object
}

// DELETE /api/task/{id} — delete a task
// Returns nothing (204 No Content from the API)
export const deleteTask = async (id) => {
  await axiosInstance.delete(`${BASE}/${id}`)
}
