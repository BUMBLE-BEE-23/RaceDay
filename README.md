# RaceDay System

## 1. System Description

RaceDay is a race event management system designed to manage running, walking and cycling events.

The system allows organisers to create and manage race events, define categories, view participant enrolments and record race results.

Participants can register for an account, view available events and categories, enrol in events and view their own race results.

The system will use a relational SQL Server database and a RESTful Web API.

---

## 2. System Roles

### Organiser

The Organiser is responsible for managing race events.

An Organiser can:

- Create race events
- View race events
- Update their own race events
- Delete their own race events
- Create event categories
- Update and delete categories for their own events
- View participants enrolled in their events
- Record participant results
- View results for their events

### Participant

The Participant is responsible for entering races and viewing their own information.

A Participant can:

- Register for an account
- Log in
- View and update their own profile
- View available race events
- View event categories
- Enrol in a race event
- Select a category when enrolling
- View their own enrolments
- View their own race results

---

## 3. Database Planning

The RaceDay system will use a relational database containing the following main entities:

- Users
- EventTypes
- Events
- Categories
- Enrollments
- Results

The database design will use primary keys and foreign keys to maintain relationships between entities.

The complete Entity Relationship Diagram is available in the `docs` folder.

---

## 4. RESTful API Planning

The RESTful API will provide endpoints for:

- Authentication
- User profiles
- Events
- Event types
- Categories
- Event enrolments
- Results

The complete API endpoint plan is available in:

`docs/endpoint-plan.md`

---

## 5. Role-Based Access Control

The system will use role-based access control.

Organisers and Participants will have different permissions.

Organisers will be able to manage their own events and the information associated with those events.

Participants will be able to enrol in events and view their own enrolments and results.

Users will not be allowed to access or modify information that they do not have permission to access.

---

## 6. SQL Server Database

The database will be implemented using Microsoft SQL Server.

The SQL script will:

- Create the required database
- Create all database tables
- Define primary keys
- Define foreign keys
- Define NOT NULL constraints
- Define UNIQUE constraints
- Define DEFAULT constraints
- Insert sample data
- Provide verification queries

The SQL script will be stored in:

`docs/RaceDay-Database.sql`

---

## 7. API Documentation

The RESTful API will be documented using Swagger/OpenAPI.

Swagger will allow the API endpoints to be viewed and tested during Part 2 of the project.

---

## 8. Testing

The RaceDay API will include automated unit tests.

The tests will cover:

- Authentication
- Registration
- Login
- Authenticated requests
- Unauthenticated requests
- Role-based access
- Organiser event management
- Participant enrolment
- Recording results
- Failure scenarios

---

## 9. CI/CD

GitHub Actions will be used to validate the project.

The CI/CD workflow will check that the required project structure and planning documents are present.

The successful GitHub Actions build will be documented with a screenshot in this README.

### CI/CD Screenshot

_Add successful GitHub Actions screenshot here._

---

## 10. Project Documentation

The Part 1 planning documents are stored in the `docs` folder.

Important files include:

- `RaceDay-ERD.png`
- `endpoint-plan.md`
- `RaceDay-Database.sql`

---

## 11. Video Presentation

An unlisted YouTube video will be provided demonstrating:

- The RaceDay planning documents
- The ERD and database design decisions
- The RESTful API endpoint plan
- The SQL Server database script
- The SQL script being executed in SQL Server Management Studio

### YouTube Video

_Add YouTube video link here._

---

## 12. Project Status

### Part 1 - System Planning and Database

- [x] Project folder structure created
- [x] ERD completed
- [x] API endpoint plan completed
- [x] SQL Server database script completed
- [x] SQL script tested in SSMS
- [ ] GitHub Actions configured
- [ ] Minimum 20 meaningful commits completed
- [ ] CI/CD successful
- [ ] CI/CD screenshot added
- [ ] YouTube presentation completed

### Part 2 - RESTful API

- [ ] API project created
- [ ] Authentication implemented
- [ ] User profiles implemented
- [ ] Events implemented
- [ ] Categories implemented
- [ ] Enrolments implemented
- [ ] Results implemented
- [ ] Swagger configured
- [ ] Unit tests implemented
- [ ] CI/CD tests passing