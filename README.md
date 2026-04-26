# Task Manager

A full-stack task management web application with JWT authentication.

Built with **ASP.NET Core** (backend) and **React + Vite** (frontend).

---

## Features

- User registration and login
- JWT token-based authentication
- Create, view, edit, and delete tasks
- Mark tasks as complete
- Protected dashboard (redirects to login if not authenticated)
- Auto logout when token expires

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core Web API (.NET 10) |
| Database | SQLite via Entity Framework Core |
| Auth | JWT Bearer Tokens + BCrypt password hashing |
| Frontend | React 19 + Vite |
| HTTP Client | Axios |
| Routing | React Router v7 |

---

## Project Structure

```
TaskManager/
├── backend/     # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Services/
│   ├── Data/
│   └── Migrations/
└── frontend/    # React + Vite
    └── src/
        ├── api/
        ├── services/
        ├── pages/
        └── components/
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dot.net)
- [Node.js](https://nodejs.org) (v18 or higher)

---

### Run the Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

API runs at: `http://localhost:5000`  
Swagger UI: `http://localhost:5000` (in Development mode)

---

### Run the Frontend

```bash
cd frontend
npm install
npm run dev
```

App runs at: `http://localhost:5173`

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Create account |
| POST | `/api/auth/login` | No | Login, get JWT token |
| GET | `/api/task` | Yes | Get all tasks |
| POST | `/api/task` | Yes | Create task |
| PUT | `/api/task/{id}` | Yes | Update task |
| DELETE | `/api/task/{id}` | Yes | Delete task |

---

## Environment Notes

- The SQLite database (`taskmanager.db`) is created automatically on first run
- JWT secret is in `backend/appsettings.json` — change it before deploying to production
- CORS is configured to allow `http://localhost:5173` (the Vite dev server)
