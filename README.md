# ITPE3200 StudyRoom MVC

A group room booking web app built with ASP.NET Core MVC and Entity Framework Core (code-first, SQLite).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (matching the version in `MVC.csproj`, e.g. .NET 10)
- EF Core CLI tools:
  ```
  dotnet tool install --global dotnet-ef
  ```
  If you already have it installed, make sure it's up to date:
  ```
  dotnet tool update --global dotnet-ef
  ```

## Getting started

Clone the repository, then from the project root (the folder containing `MVC.csproj`):

### 1. Create/update the local database

The SQLite database file (`RoomBookingDb.db`) is **not** committed to the repository (see `.gitignore`) to avoid binary merge conflicts. Each person working on the project needs to generate it locally from the migrations:

```
dotnet ef database update
```

This creates `RoomBookingDb.db` in the project root and applies all migrations in the `Migrations/` folder, so your local schema matches the current model.

> Run this again any time you pull changes that include new migrations.

### 2. Run the app

```
dotnet run
```

The console output will show the local URL (`http://localhost:xxxx`). Open it in your browser.
