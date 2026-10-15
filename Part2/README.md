# RaceDay API — Part 2

ASP.NET Core Web API for the RaceDay event system. The solution contains the API and an automated test project.

## Features

- ASP.NET Core Web API and Swagger UI.
- SQL Server with Entity Framework Core Code First.
- Registration and login with securely hashed passwords (`PasswordHasher<TUser>`).
- Cookie authentication plus server-side session state.
- `Organiser` and `Participant` role restrictions.
- Profiles, events, event categories, enrolments, and results.
- Unit tests and a GitHub Actions build/test workflow.

> **Important Part 1 alignment note:** This project is generated from the Part 2 brief and the limited project information available. Before final submission, compare the entities, fields, relationships, and endpoint routes against your approved Part 1 ERD, SQL script, and endpoint plan. Update the code if your approved specification differs.

## Requirements

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code with C# extensions

## Setup

1. Open `RaceDay.sln`.
2. Update `RaceDay.API/appsettings.json` so `ConnectionStrings:DefaultConnection` points to your SQL Server instance.
3. From the solution folder, run:

   ```powershell
   dotnet restore
   dotnet build
   dotnet test
   ```

4. Create/update the database:

   ```powershell
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project RaceDay.API --startup-project RaceDay.API
   dotnet ef database update --project RaceDay.API --startup-project RaceDay.API
   ```

   If `dotnet-ef` is already installed, skip the install command. Do not create a second `InitialCreate` migration if one already exists.

5. Run the API:

   ```powershell
   dotnet run --project RaceDay.API
   ```

6. Open the Swagger URL printed in the terminal (normally `https://localhost:7xxx/swagger` or `http://localhost:5xxx/swagger`).

## Roles

- **Participant:** view events/categories, manage own profile, enrol in an event, and view own results.
- **Organiser:** view events, manage events and their categories, see enrolments for events they own, and record participant results.

Registration accepts either role because the assignment says users select their role. For a production system, public self-selection of Organiser would normally be restricted or require approval.

## Main endpoints

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register |
| POST | `/api/auth/login` | Public | Log in |
| POST | `/api/auth/logout` | Authenticated | Log out |
| GET/PUT | `/api/profile/me` | Authenticated | Read/update own profile |
| GET | `/api/events` | Public | List events |
| GET | `/api/events/{id}` | Public | Event details |
| POST | `/api/events` | Organiser | Create event |
| PUT/DELETE | `/api/events/{id}` | Owning Organiser | Update/delete event |
| GET | `/api/events/{eventId}/categories` | Public | List categories |
| POST | `/api/events/{eventId}/categories` | Organiser | Add category |
| PUT/DELETE | `/api/categories/{id}` | Owning Organiser | Update/delete category |
| POST | `/api/enrolments` | Participant | Enrol in event |
| GET | `/api/events/{eventId}/enrolments` | Owning Organiser | View event enrolments |
| POST | `/api/results` | Owning Organiser | Record/update a result |
| GET | `/api/results/me` | Participant | View own results |
| GET | `/api/events/{eventId}/results` | Owning Organiser | View event results |

## Authentication and Swagger

Login issues an HTTP-only authentication cookie and also stores the user ID and role in ASP.NET Core session state. Use Swagger's **Try it out** to register, then log in; the browser should retain the cookie for subsequent requests. Log out to clear the session and authentication cookie.

For local HTTP development, secure-cookie behaviour is adjusted by the environment. Use HTTPS for any real deployment.

## Tests and CI

`RaceDay.Tests` contains automated tests for password hashing, role-protected endpoints' metadata, and controller behaviour. Run all tests using `dotnet test`.

The workflow at `.github/workflows/ci.yml` restores, builds, and tests the solution on pushes and pull requests. After the first green GitHub Actions run, capture a screenshot of the successful run and add it to this README or your submission evidence folder.

## Required submission tasks still to do

- Compare schema and routes with the approved Part 1 documents.
- Run the migration against your own SQL Server instance.
- Run `dotnet build` and `dotnet test` locally.
- Commit your work in at least 20 meaningful, genuine commits; do not create empty or misleading commits just to reach the number.
- Add the actual successful CI screenshot.
- Add your unlisted YouTube demonstration URL.
- Submit your repository link on ARC.
