## Project Tracker API

### Overview
ASP.NET Core API for collaborative project/task tracking with custom role management:
- Project Owner: full control over project and tasks (create/edit/delete)
- Employee: can view assigned tasks and update status only

### Prerequisites
- .NET SDK 9
- SQL Server (LocalDB or your instance)

### Configure Database
1. Open `appsettings.json` in `ProjectTracker.Api`.
2. Set `ConnectionStrings:DefaultConnection` to your SQL Server instance, e.g.:
   `Server=YOUR_SERVER_NAME;Database=ProjectTracker;Trusted_Connection=True;TrustServerCertificate=True`

### Run
Start the API (e.g., `dotnet run` or F5 in your IDE). On startup, the app automatically applies migrations and seeds roles (`Project Owner`, `Employee`).

Swagger UI: `/swagger`

### Auth
- Register: `POST /api/auth/register` (email, password, fullName, designation)
- Login: `POST /api/auth/login` → `{ token }`
- Send `Authorization: Bearer <token>` on API requests

### Projects
- `GET /api/projects` (my projects)
- `POST /api/projects` (create; owner only)
- `GET /api/projects/{id}` (includes members)
- `POST /api/projects/join` (inviteCode)
- `DELETE /api/projects/{id}` (owner only)

### Tasks
- `GET /api/projects/{projectId}/tasks` (owner: all, employee: assigned only)
- `POST /api/projects/{projectId}/tasks` (owner only)
- `PUT /api/projects/{projectId}/tasks/{taskId}` (owner only)
- `PATCH /api/projects/{projectId}/tasks/{taskId}/status` (owner or any project member)
- `DELETE /api/projects/{projectId}/tasks/{taskId}` (owner only)

### Attachments
- `GET/POST /api/projects/{projectId}/tasks/{taskId}/attachments`

### Realtime
- SignalR Hub: `/hubs/tasks` (clients call `JoinProject(projectId)`)


