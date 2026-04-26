import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getUser, logout } from '../services/authService'
import { getAllTasks, createTask, updateTask, deleteTask } from '../services/taskService'

function Dashboard() {
  // ── State ──────────────────────────────────────────────────────────────────

  // The list of tasks fetched from the API
  const [tasks, setTasks] = useState([])

  // Loading and error states for the initial fetch
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // Form state for creating a NEW task
  const [newTitle, setNewTitle] = useState('')
  const [newDescription, setNewDescription] = useState('')
  const [creating, setCreating] = useState(false)
  const [createError, setCreateError] = useState('')

  // editingTask holds the task currently being edited (null = no task being edited)
  // When this is set, we show an inline edit form for that specific task
  const [editingTask, setEditingTask] = useState(null)

  // Temporary values while the user edits a task
  const [editTitle, setEditTitle] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editIsCompleted, setEditIsCompleted] = useState(false)
  const [saving, setSaving] = useState(false)

  const navigate = useNavigate()
  const user = getUser() // Read { userId, username, email } from localStorage

  // ── Fetch tasks on page load ───────────────────────────────────────────────
  //
  // useEffect(fn, []) runs fn ONCE after the component first appears on screen.
  // The empty array [] is the "dependency array" — it means "run this only once,
  // never again." If you put [tasks] there, it would run every time tasks changes
  // (which would cause an infinite loop here).

  useEffect(() => {
    fetchTasks()
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  const fetchTasks = async () => {
    try {
      setLoading(true)
      const data = await getAllTasks()  // GET /api/task
      setTasks(data)
    } catch (err) {
      setError('Failed to load tasks. Please refresh the page.')
    } finally {
      setLoading(false)
    }
  }

  // ── Logout ─────────────────────────────────────────────────────────────────

  const handleLogout = () => {
    logout()           // Clears token and user from localStorage
    navigate('/login') // Redirect to login page
  }

  // ── Create task ────────────────────────────────────────────────────────────

  const handleCreateTask = async (e) => {
    e.preventDefault()
    if (!newTitle.trim()) return  // Don't submit empty titles

    setCreating(true)
    setCreateError('')

    try {
      const created = await createTask(newTitle, newDescription)  // POST /api/task
      // Add the new task to the TOP of our local list (so user sees it immediately
      // without waiting for another API call to re-fetch everything)
      setTasks([created, ...tasks])
      // Clear the form
      setNewTitle('')
      setNewDescription('')
    } catch (err) {
      setCreateError(err.response?.data?.message || 'Failed to create task.')
    } finally {
      setCreating(false)
    }
  }

  // ── Start editing a task ───────────────────────────────────────────────────
  // When the user clicks "Edit" on a task, we populate the edit form with
  // that task's current values and store the task as "editingTask"

  const startEdit = (task) => {
    setEditingTask(task)
    setEditTitle(task.title)
    setEditDescription(task.description || '')
    setEditIsCompleted(task.isCompleted)
  }

  const cancelEdit = () => {
    setEditingTask(null)
  }

  // ── Save edit ──────────────────────────────────────────────────────────────

  const handleSaveEdit = async (e) => {
    e.preventDefault()
    if (!editTitle.trim()) return

    setSaving(true)

    try {
      const updated = await updateTask(          // PUT /api/task/{id}
        editingTask.id,
        editTitle,
        editDescription,
        editIsCompleted
      )

      // Replace the old task in our local list with the updated one.
      // map() creates a NEW array — React requires this, you can't mutate state directly.
      setTasks(tasks.map(t => t.id === updated.id ? updated : t))
      setEditingTask(null)
    } catch (err) {
      alert('Failed to update task. Please try again.')
    } finally {
      setSaving(false)
    }
  }

  // ── Delete task ────────────────────────────────────────────────────────────

  const handleDelete = async (id) => {
    // window.confirm() shows a browser confirmation popup — simple but effective
    if (!window.confirm('Are you sure you want to delete this task?')) return

    try {
      await deleteTask(id)  // DELETE /api/task/{id}
      // Remove the deleted task from local state using filter()
      // filter() returns a NEW array with only tasks where t.id !== id
      setTasks(tasks.filter(t => t.id !== id))
    } catch (err) {
      alert('Failed to delete task. Please try again.')
    }
  }

  // ── Toggle complete (quick checkbox) ──────────────────────────────────────

  const handleToggleComplete = async (task) => {
    try {
      const updated = await updateTask(
        task.id,
        task.title,
        task.description,
        !task.isCompleted  // Flip the completed status
      )
      setTasks(tasks.map(t => t.id === updated.id ? updated : t))
    } catch (err) {
      alert('Failed to update task.')
    }
  }

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="dashboard">
      {/* ── Header ── */}
      <header className="dashboard-header">
        <div className="header-left">
          <h1>Task Manager</h1>
          {/* Optional chaining (?.) prevents errors if user is null */}
          <span className="welcome-text">Welcome, {user?.username}!</span>
        </div>
        <button className="btn btn-outline" onClick={handleLogout}>
          Logout
        </button>
      </header>

      <main className="dashboard-main">
        {/* ── Add New Task Form ── */}
        <section className="card">
          <h2>Add New Task</h2>
          {createError && <div className="alert alert-error">{createError}</div>}
          <form onSubmit={handleCreateTask} className="task-form">
            <input
              type="text"
              placeholder="Task title (required)"
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              className="task-input"
              required
            />
            <textarea
              placeholder="Description (optional)"
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              className="task-textarea"
              rows={2}
            />
            <button type="submit" className="btn btn-primary" disabled={creating}>
              {creating ? 'Adding...' : '+ Add Task'}
            </button>
          </form>
        </section>

        {/* ── Task List ── */}
        <section className="card">
          <h2>
            My Tasks
            <span className="task-count">({tasks.length})</span>
          </h2>

          {loading && <p className="loading-text">Loading tasks...</p>}
          {error && <div className="alert alert-error">{error}</div>}

          {/* Show empty state if no tasks */}
          {!loading && tasks.length === 0 && (
            <p className="empty-text">No tasks yet. Add one above!</p>
          )}

          <ul className="task-list">
            {/*
              .map() loops over the tasks array and turns each task object
              into a <li> element. The "key" prop is required by React to
              efficiently track which items changed (always use a unique ID).
            */}
            {tasks.map((task) => (
              <li
                key={task.id}
                className={`task-item ${task.isCompleted ? 'task-completed' : ''}`}
              >
                {/* If this task is being edited, show the edit form instead */}
                {editingTask?.id === task.id ? (
                  <form onSubmit={handleSaveEdit} className="edit-form">
                    <input
                      type="text"
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      className="task-input"
                      required
                    />
                    <textarea
                      value={editDescription}
                      onChange={(e) => setEditDescription(e.target.value)}
                      className="task-textarea"
                      rows={2}
                    />
                    <label className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={editIsCompleted}
                        onChange={(e) => setEditIsCompleted(e.target.checked)}
                      />
                      Mark as completed
                    </label>
                    <div className="edit-buttons">
                      <button type="submit" className="btn btn-primary" disabled={saving}>
                        {saving ? 'Saving...' : 'Save'}
                      </button>
                      <button type="button" className="btn btn-outline" onClick={cancelEdit}>
                        Cancel
                      </button>
                    </div>
                  </form>
                ) : (
                  /* Normal view of a task */
                  <div className="task-content">
                    <div className="task-left">
                      {/* Quick complete toggle checkbox */}
                      <input
                        type="checkbox"
                        checked={task.isCompleted}
                        onChange={() => handleToggleComplete(task)}
                        className="task-checkbox"
                        title="Toggle complete"
                      />
                      <div className="task-text">
                        <span className="task-title">{task.title}</span>
                        {task.description && (
                          <span className="task-description">{task.description}</span>
                        )}
                        <span className="task-date">
                          {/* Format the ISO date string into a readable format */}
                          Created: {new Date(task.createdAt).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                    <div className="task-actions">
                      <button
                        className="btn btn-small btn-secondary"
                        onClick={() => startEdit(task)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn btn-small btn-danger"
                        onClick={() => handleDelete(task.id)}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                )}
              </li>
            ))}
          </ul>
        </section>
      </main>
    </div>
  )
}

export default Dashboard
