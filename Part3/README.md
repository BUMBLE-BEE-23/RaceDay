# RACEDAY – PART 3: MVC WEB APPLICATION

## 1. Project Overview

Part 3 of the RaceDay project focuses on developing an ASP.NET Core MVC web application that communicates with the RESTful API developed in Part 2.

The MVC application provides a user-friendly interface for Organisers and Participants to access race management functionality.

**Project:** RaceDay  
**Assignment:** Part 3 – MVC Web Application  
**Technology:** ASP.NET Core MVC, C#, Razor Views, Bootstrap and SQL Server  
**API Integration:** Part 2 – RaceDay REST API

## 2. Project Structure

The MVC application is located in:

`Part3/RaceDay.MVC`

Its main folders are:

- `Controllers` – Handles application requests and user interactions.
- `Models` – Contains the application's ViewModels.
- `Services` – Handles communication with the RaceDay API.
- `Views` – Contains the Razor user interface pages.
- `wwwroot` – Contains static files, CSS, JavaScript and uploaded profile pictures.

## 3. Implemented Functionality

### Account Management
- User registration.
- User login and logout.
- Session-based authentication.
- Role-based access for Organisers and Participants.

### Organiser Features
- Organiser dashboard.
- View and manage events.
- Create, edit and delete event categories.
- View participant enrolments.
- Record race results.
- Update personal profile information.
- Upload and display profile pictures.

### Participant Features
- Participant dashboard.
- Browse available events.
- View event categories.
- Enrol in events.
- View personal enrolments.
- View race results.
- Update personal profile information.
- Upload and display profile pictures.

## 4. Profile Picture Uploads

Profile pictures are uploaded and served locally through the MVC application's `wwwroot` folder.

Upload location:

`wwwroot/uploads/profiles`

Supported image formats:

- JPG
- JPEG
- PNG
- WebP

Maximum upload size: 5 MB.

The application does not use Azure Blob Storage for profile pictures.

## 5. API Integration

The MVC application communicates with the Part 2 REST API using `RaceDayApiService` and `HttpClient`.

The API handles the application's data operations, authentication and role-based access control.

The MVC application provides the user interface and communicates with the API rather than directly accessing the database.

## 6. Technologies Used

- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- Razor Views
- Entity Framework Core
- SQL Server
- Bootstrap
- HTML and CSS
- JavaScript
- Visual Studio 2022
- Git and GitHub

## 7. How to Run the Application

1. Open the RaceDay solution in Visual Studio 2022.
2. Ensure SQL Server is available and the Part 2 API database is configured.
3. Start the `RaceDay.API` project first.
4. Start the `RaceDay.MVC` project.
5. Open the MVC application in your browser.
6. Register or log in as an Organiser or Participant to explore the application.

The API must be running for the MVC application to retrieve and update data.

## 8. Important Notes

- Part 3 builds on the API developed in Part 2.
- The MVC application is contained in the `Part3` folder.
- Profile pictures are stored locally in `wwwroot`.
- Docker is not required for this project.

## 9. Assignment Separation

The RaceDay repository contains three assignment parts:

- **Part 1:** System Planning and Database Design.
- **Part 2:** RESTful API Development.
- **Part 3:** MVC Web Application.

This folder contains the implementation and documentation for **Part 3 – MVC Web Application**.



**RaceDay – Part 3 Submission**