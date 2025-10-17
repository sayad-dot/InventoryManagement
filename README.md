# Inventory Management System

A web application for managing custom inventories with features for customizable IDs, real-time discussions, multi-user collaboration, and more. Built with ASP.NET Core MVC and Entity Framework Core.

## Features

- Custom inventory definitions with arbitrary fields
- Configurable/custom ID formats for items and inventories
- Multi-user collaboration and access control (public/private inventories)
- Real-time discussions per-inventory using SignalR
- Full-text search across inventories and items
- Social authentication (Google, Facebook) and ASP.NET Core Identity
- Admin panel, optimistic locking, multi-language support, and responsive UI

## Technology Stack

- Backend: ASP.NET Core MVC (C#)
- Database: PostgreSQL with Entity Framework Core
- Frontend: Bootstrap 5, JavaScript, jQuery, Razor views
- Real-time: SignalR
- Authentication: ASP.NET Core Identity (+ social logins)

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 16+ (or compatible)
- Optional: Visual Studio Code / Visual Studio

## Quickstart

1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/InventoryManagement.git
cd InventoryManagement
```

1. Configure the database connection

Edit `appsettings.json` (or `appsettings.Development.json`) and set the `DefaultConnection` connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=InventoryDB;Username=postgres;Password=your_password"
}
```

Replace `your_password` and other values as appropriate.

1. Apply database migrations

```bash
dotnet ef database update
```

1. Run the app

```bash
dotnet run
```

By default the app will bind to the URLs configured in `Properties/launchSettings.json`. Typical local URLs are:

- [https://localhost:7000](https://localhost:7000)
- [http://localhost:5000](http://localhost:5000)

Open one of those in your browser.

## Development Notes

- The EF Core DbContext is in `Data/ApplicationDbContext.cs`.
- Controllers live in the `Controllers/` folder; views are under `Views/`.
- Static files (CSS, JS, images) are in `wwwroot/`.

If you add new EF Core migrations, create them with:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Environment & Configuration

- Use `appsettings.Development.json` for local dev overrides.
- Store secrets like production DB passwords or OAuth client secrets in environment variables or a secrets manager; do not commit them to source control.

Example environment variables you might set in production (Linux systemd or container):

```bash
export ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."
export ASPNETCORE_ENVIRONMENT=Production
```

## Docker (optional)

You can containerize the app by creating a Dockerfile and publishing the app. A basic workflow:

```bash
dotnet publish -c Release -o out
docker build -t inventory-management:latest .
docker run -e "ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=..." -p 5000:80 inventory-management:latest
```

## Troubleshooting

- If migrations fail, confirm the connection string and that PostgreSQL is running and accessible.
- For runtime errors, check the logs printed to the console and the files under `bin/Debug/net8.0` when running locally.

## Contributing

Contributions are welcome. Typical steps:

1. Fork the repository
2. Create a feature branch
3. Add tests and ensure the app builds
4. Open a pull request with a clear description

## License

This project is released under the MIT License. See the `LICENSE` file for details.

## Author

Sayad Ibna Azad

---
If you'd like, I can also:

- Add a minimal `Dockerfile` to the repo
- Add a `Makefile` or VS Code tasks for common commands
- Create a short CONTRIBUTING.md with contribution guidelines
