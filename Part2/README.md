\# RaceDay System



\## 1. System Description



RaceDay is a race event management system designed to manage running, walking and cycling events.



The system allows organisers to create and manage race events, define categories, view participant enrolments and record race results. Participants can register for an account, log in, view available events and categories, enrol in events and view their own race results.



The system uses ASP.NET Core Web API, C#, Entity Framework Core and Microsoft SQL Server to provide a RESTful API and relational database.



\## 2. System Roles



\### Organiser



An Organiser can:



\- Create, view, update and delete their own race events.

\- Create and manage categories belonging to their events.

\- View participants enrolled in their events.

\- Record participant results.

\- View results for their events.



\### Participant



A Participant can:



\- Register for an account.

\- Log in and log out.

\- View and update their own profile.

\- View available race events and categories.

\- Enrol in a race event and select a category.

\- View their own race results.



\## 3. Database Planning and Implementation



The RaceDay system uses a relational database with the following main entities:



\- Users

\- Events

\- Categories

\- EventEnrolments

\- Results



Primary keys, foreign keys, constraints and relationships help maintain data integrity.



Entity Framework Core connects the API to SQL Server, while database migrations manage the creation and updating of the database schema.



The database configuration and migration files are located in `Part2/RaceDay.API`.



The Part 1 Entity Relationship Diagram is available in `docs/RaceDay-ERD.png`, and the original SQL database script is available in `docs/RaceDay-Database.sql`.



\## 4. RESTful API Endpoints



| Endpoint | Method | Purpose |

|---|---|---|

| `/api/auth/register` | POST | Register a user |

| `/api/auth/login` | POST | Authenticate a user |

| `/api/auth/logout` | POST | Log out an authenticated user |

| `/api/profile/me` | GET | Retrieve the current user's profile |

| `/api/profile/me` | PUT | Update the current user's profile |

| `/api/events` | GET | List events |

| `/api/events/{id}` | GET | Retrieve an event |

| `/api/events` | POST | Create an event |

| `/api/events/{id}` | PUT | Update an event |

| `/api/events/{id}` | DELETE | Delete an event |

| `/api/events/{eventId}/categories` | GET | List event categories |

| `/api/events/{eventId}/categories` | POST | Create a category |

| `/api/categories/{id}` | PUT | Update a category |

| `/api/categories/{id}` | DELETE | Delete a category |

| `/api/enrolments` | POST | Enrol a participant |

| `/api/events/{eventId}/enrolments` | GET | List event enrolments |

| `/api/results` | POST | Record a race result |

| `/api/results/me` | GET | View the participant's results |

| `/api/events/{eventId}/results` | GET | List an event's results |



The complete Part 1 endpoint plan is available in `docs/endpoint-plan.md`.



\## 5. Authentication and Role-Based Access Control



RaceDay uses cookie-based authentication and server-side session management to maintain authenticated user sessions.



Passwords are hashed before being stored in the database. The API supports two roles: Organiser and Participant.



Authorisation attributes restrict protected endpoints to the appropriate role. Event ownership checks prevent organisers from modifying events and related information belonging to other organisers.



The API also validates registration roles, duplicate email addresses, category selections and enrolment requests.



\## 6. SQL Server Database Setup



Microsoft SQL Server is the database platform. Entity Framework Core and its SQL Server provider handle database access.



The original Part 1 SQL script includes database creation, tables, constraints, sample data and verification queries.



The Part 2 API uses the `DefaultConnection` connection string in:



`Part2/RaceDay.API/appsettings.json`



The local development configuration uses SQL Server LocalDB. Before running the API, ensure SQL Server LocalDB is installed and the connection string matches the database configuration on your computer.



\### Apply database migrations



Open PowerShell in the repository root and run:



```powershell

dotnet ef database update --project Part2/RaceDay.API/RaceDay.API.csproj --startup-project Part2/RaceDay.API/RaceDay.API.csproj

```



If the `dotnet ef` command is unavailable, install the compatible Entity Framework Core command-line tool.



For deployment, configure the connection string for the target SQL Server environment.



\## 7. How to Run the API and Open Swagger



These instructions allow the lecturer to run the project on their own computer.



\### Prerequisites



\- Visual Studio 2022 or a compatible .NET development environment.

\- .NET 8 SDK.

\- SQL Server LocalDB or a compatible SQL Server instance.

\- A copy of the RaceDay GitHub repository.



\### Step 1: Obtain the project



Clone the repository:



```powershell

git clone https://github.com/BUMBLE-BEE-23/RaceDay.git

```



Open the downloaded `RaceDay` folder in PowerShell.



If the repository is already downloaded, navigate to its root folder instead.



\### Step 2: Restore the project dependencies



Run:



```powershell

dotnet restore .\\Part2\\RaceDay.API\\RaceDay.API.csproj

```



\### Step 3: Configure the database



Check `Part2/RaceDay.API/appsettings.json` and confirm that `DefaultConnection` points to an available SQL Server instance.



Apply the database migrations using the command in Section 6.



\### Step 4: Start the API



From the repository root, run:



```powershell

dotnet run --project .\\Part2\\RaceDay.API\\RaceDay.API.csproj

```



Wait for the terminal to display the application URLs.



\### Step 5: Open Swagger



Open a browser and navigate to the application URL displayed in the terminal, followed by `/swagger`.



For example, if the terminal displays `https://localhost:7001`, open:



`https://localhost:7001/swagger`



The example port is illustrative only. Use the actual URL displayed by your application.



Swagger UI allows the lecturer to inspect endpoints, view request and response formats, and test API operations.



\*\*Important:\*\* A `localhost` URL only works on the computer running the API. The lecturer must start the API on their own computer before opening Swagger.



\## 8. Automated Testing



Automated tests are maintained in `Part2/RaceDay.Tests`.



The test suite covers authentication, password hashing, registration validation, duplicate email addresses, role-based access, event ownership, profile operations, categories, participant enrolments, race results and API integration.



Run the tests from the repository root:



```powershell

dotnet test .\\Part2\\RaceDay.Tests\\RaceDay.Tests.csproj --configuration Release

```



The latest reported local test run passed \*\*46 tests, with zero failures and zero skipped tests\*\*.



To verify the current result, run the command above and check the test summary in the terminal.



\## 9. Continuous Integration and Continuous Delivery (CI/CD)



GitHub Actions is configured to validate the RaceDay repository.



The workflow is stored in `.github/workflows/ci.yml`.



The workflow checks required documentation, restores API dependencies, builds the .NET 8 API and runs the automated tests.



The workflow is configured to run on pushes and pull requests targeting the relevant branches.



The repository includes the Part 1 CI/CD success screenshot at `docs/CI-CD-Success.png`.



Before final submission, check the latest GitHub Actions run on the repository's `main` branch and capture an updated screenshot of the successful Part 2 workflow.



GitHub repository:



https://github.com/BUMBLE-BEE-23/RaceDay



\## 10. Project Documentation



Important Part 1 documents:



\- `docs/RaceDay-ERD.png` — Entity Relationship Diagram.

\- `docs/endpoint-plan.md` — RESTful API endpoint plan.

\- `docs/RaceDay-Database.sql` — SQL Server database script.

\- `docs/CI-CD-Success.png` — Part 1 CI/CD success screenshot.



Important Part 2 folders:



\- `Part2/RaceDay.API` — ASP.NET Core Web API, models, controllers, database context and migrations.

\- `Part2/RaceDay.Tests` — automated unit and integration tests.

\- `.github/workflows` — GitHub Actions CI/CD workflow.



\## 11. Video Presentation



\### Part 1 Presentation



The Part 1 presentation demonstrates the planning documents, ERD, endpoint plan, SQL Server database script and CI/CD setup.



YouTube link:



https://youtu.be/YYzX46cWQPE?si=88JKjWftkSXpd6Rv



\### Part 2 Presentation



The Part 2 presentation will demonstrate:



\- The RaceDay API running locally.

\- Swagger/OpenAPI documentation and endpoint testing.

\- User registration, login and logout.

\- Organiser and Participant roles and access restrictions.

\- Event, category, enrolment and results operations.

\- SQL Server integration.

\- Automated test results and GitHub Actions.



\*\*Part 2 video link:\*\* To be added after recording and uploading the demonstration.



\## 12. Project Status



\### Part 1 — System Planning and Database



\- \[x] Project folder structure created.

\- \[x] Entity Relationship Diagram completed.

\- \[x] RESTful API endpoint plan completed.

\- \[x] SQL Server database script completed.

\- \[x] SQL script tested in SQL Server Management Studio.

\- \[x] GitHub Actions workflow configured.

\- \[x] CI/CD success screenshot documented.

\- \[x] Part 1 presentation uploaded.



\### Part 2 — RESTful API Development



\- \[x] ASP.NET Core Web API project created.

\- \[x] Entity Framework Core SQL Server integration configured.

\- \[x] Database migrations created.

\- \[x] User registration and login implemented.

\- \[x] Password hashing implemented.

\- \[x] Cookie authentication and session management configured.

\- \[x] User profile endpoints implemented.

\- \[x] Event management endpoints implemented.

\- \[x] Category management endpoints implemented.

\- \[x] Participant enrolment endpoints implemented.

\- \[x] Race result endpoints implemented.

\- \[x] Role-based authorisation implemented.

\- \[x] Swagger/OpenAPI configured.

\- \[x] Automated tests implemented.

\- \[x] Latest reported local test run passed 46 tests.

\- \[x] GitHub Actions build-and-test workflow configured.

\- \[ ] Final GitHub Actions result verified.

\- \[ ] Part 2 presentation recorded and uploaded.

\- \[ ] Final submission files reviewed.



\## 13. Conclusion



RaceDay is a role-based race event management system developed using ASP.NET Core Web API, C#, Entity Framework Core and Microsoft SQL Server.



Part 1 established the system design, database plan and RESTful API specification. Part 2 implements the API endpoints, authentication, role-based access control, database integration, automated testing and continuous integration workflow.



The completed API provides a foundation for the continued development of the RaceDay system in subsequent assessment stages.



