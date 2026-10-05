# ITPE3200 StudyRoom MVC

A group room booking web app built with ASP.NET Core MVC and Entity Framework Core (code-first, SQLite).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (matching the version in `MVC.csproj`, e.g. .NET 10)
- Node.js is not used and is not required.
- EF Core CLI tools:
  ```
  dotnet tool install --global dotnet-ef
  ```
  If you already have it installed, make sure it's up to date:
  ```
  dotnet tool update --global dotnet-ef
  ```

## Getting started

If you received the project as a ZIP file, extract it first and open a terminal in the folder containing `MVC.csproj`.

Clone the repository, then from the project root (the folder containing `MVC.csproj`):

Restore the required packages before continuing:

```
dotnet restore MVC.csproj
```

### 1. Create/update the local database

The SQLite database file (`RoomBookingDb.db`) is **not** committed to the repository (see `.gitignore`) to avoid binary merge conflicts. Each person working on the project needs to generate it locally from the migrations:

```
dotnet ef database update
```

This creates `RoomBookingDb.db` in the project root and applies all migrations in the `Migrations/` folder, so your local schema matches the current model.

The migrations also add eight sample rooms, so you can test the application without creating rooms manually.

To explicitly select the project, use `dotnet ef database update --project MVC.csproj`.

> Run this again any time you pull changes that include new migrations.

### 2. Run the app

```
dotnet run
```

The console output will show the local URL (`http://localhost:xxxx`). Open it in your browser.

You can also explicitly select the project with `dotnet run --project MVC.csproj`. Press `Ctrl+C` in the terminal to stop the application.

## Prototype login

The login page is a prototype and is not connected to authentication.
Enter any username and password, then click **Logg inn** to continue.
Do not use a real password.

All reservations currently belong to the same temporary demo user.

## Use of generative AI

We used generative AI as a collaborative assistant and a learning tool during development. It helped us understand MVC, Entity Framework and the existing code, discuss possible solutions, troubleshoot errors, and write clearer comments and documentation. AI also helped develop parts of the room and booking functionality.

We used the discussions to ask questions and understand how and why the code works. The group remained responsible for the final decisions, adapting suggestions to the project and reviewing and testing the functionality.
