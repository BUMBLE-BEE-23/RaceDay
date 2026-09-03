# RaceDay API Endpoint Plan

## 1. Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Register a new user as an Organiser or Participant | Public | FirstName, LastName, Email, Password, Role | 201 Created with user details |
| POST | /api/auth/login | Authenticate a user and return an access token | Public | Email, Password | 200 OK with JWT token and user details |

---

## 2. User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/profile | View the logged-in user's profile | Organiser / Participant | None | 200 OK with profile |
| PUT | /api/profile | Update the logged-in user's profile | Organiser / Participant | FirstName, LastName, Email | 200 OK with updated profile |

---

## 3. Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | View all race events | Organiser / Participant | None | 200 OK with events |
| GET | /api/events/{id} | View a specific race event | Organiser / Participant | None | 200 OK with event |
| POST | /api/events | Create a new race event | Organiser | Name, Description, EventDate, Location, Distance, EventTypeID | 201 Created with event |
| PUT | /api/events/{id} | Update an event owned by the logged-in Organiser | Organiser | Name, Description, EventDate, Location, Distance, EventTypeID | 200 OK with updated event |
| DELETE | /api/events/{id} | Delete an event owned by the logged-in Organiser | Organiser | None | 204 No Content |

### Event Requirements

Each event must contain:

- Event name
- Description
- Date
- Location
- Distance
- Event type

Allowed event types:

- Run
- Walk
- Cycle

Organisers may only update or delete events that they own.

---

## 4. Event Types

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/event-types | View available event types | Organiser / Participant | None | 200 OK with event types |

---

## 5. Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/categories | View available categories | Organiser / Participant | None | 200 OK with categories |
| GET | /api/events/{eventId}/categories | View categories for a specific event | Organiser / Participant | None | 200 OK with categories |
| POST | /api/events/{eventId}/categories | Create a category for an event owned by the Organiser | Organiser | Name, MinimumAge, MaximumAge, Distance | 201 Created with category |
| PUT | /api/categories/{id} | Update a category belonging to an event owned by the Organiser | Organiser | Name, MinimumAge, MaximumAge, Distance | 200 OK with updated category |
| DELETE | /api/categories/{id} | Delete a category belonging to an event owned by the Organiser | Organiser | None | 204 No Content |

---

## 6. Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{eventId}/enrolments | Enrol the logged-in Participant in an event and selected category | Participant | CategoryID | 201 Created with enrolment |
| GET | /api/enrolments/me | View the logged-in Participant's enrolments | Participant | None | 200 OK with enrolments |
| GET | /api/events/{eventId}/enrolments | View enrolments for an event owned by the Organiser | Organiser | None | 200 OK with enrolments |
| GET | /api/enrolments/{id} | View a specific enrolment | Participant / Organiser | None | 200 OK with enrolment |

### Enrolment Rules

A Participant must:

1. Be authenticated.
2. Select an event.
3. Select a category belonging to that event.
4. Have their ParticipantID recorded.
5. Have the EventID recorded.
6. Have the selected CategoryID recorded.

A Participant may not enrol in the same event more than once.

Organisers may only view enrolments for their own events.

---

## 7. Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{enrolmentId}/result | Record a result for a participant enrolment | Organiser | FinishTime, FinishingPosition | 201 Created with result |
| PUT | /api/results/{id} | Update a result for an event owned by the Organiser | Organiser | FinishTime, FinishingPosition | 200 OK with updated result |
| GET | /api/results/me | View the logged-in Participant's own results | Participant | None | 200 OK with results |
| GET | /api/events/{eventId}/results | View results for an event owned by the Organiser | Organiser | None | 200 OK with results |

### Result Requirements

Each result must contain:

- Finish time
- Finishing position
- The related enrolment

Organisers may only record or update results for participants enrolled in their own events.

Participants may only view their own results.

---

## 8. Authentication and Authorisation Rules

All protected endpoints require authentication.

The API will use JWT authentication.

### Public Endpoints

The following endpoints do not require authentication:

- POST /api/auth/register
- POST /api/auth/login

### Organiser Permissions

Organisers can:

- Create events
- View events
- Update their own events
- Delete their own events
- Create categories for their events
- Update their own event categories
- Delete their own event categories
- View enrolments for their events
- Record results for participants in their events
- Update results for their events
- View results for their events

### Participant Permissions

Participants can:

- View events
- View event categories
- View and update their own profile
- Enrol in events
- View their own enrolments
- View their own results

### Security Rules

- Unauthenticated users cannot access protected endpoints.
- Participants cannot create, update or delete events.
- Participants cannot manage event categories.
- Participants cannot view another participant's private enrolments.
- Participants cannot create or modify race results.
- Organisers cannot modify events belonging to another Organiser.
- Organisers cannot manage categories belonging to another Organiser's events.
- Organisers cannot record or modify results for another Organiser's event.
- Users can only update their own profile.

---

## 9. Standard HTTP Responses

The API should use appropriate HTTP status codes.

| Status Code | Meaning |
|---|---|
| 200 | Request successful |
| 201 | Resource successfully created |
| 204 | Resource successfully deleted |
| 400 | Invalid request |
| 401 | Authentication required or invalid credentials |
| 403 | User does not have permission |
| 404 | Resource not found |
| 409 | Conflict, such as duplicate registration or enrolment |
| 500 | Unexpected server error |

---

## 10. API Documentation

All endpoints will be documented using Swagger/OpenAPI.

Swagger will show:

- HTTP method
- Route
- Description
- Required authentication
- Required role
- Request body
- Expected response
- HTTP status codes

All endpoints will be testable through Swagger during Part 2.