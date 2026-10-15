# Endpoint reference

All request bodies are JSON. ASP.NET Core model validation returns `400 Bad Request` for invalid annotated input.

## Authentication
- `POST /api/auth/register` — `firstName`, `lastName`, `email`, `password` (minimum 8 chars), `role` (`Organiser` or `Participant`), optional `phoneNumber`.
- `POST /api/auth/login` — `email`, `password`; establishes an HTTP-only cookie and session.
- `POST /api/auth/logout` — authenticated; clears session and cookie.

## Profile
- `GET /api/profile/me` — current user's profile.
- `PUT /api/profile/me` — `firstName`, `lastName`, optional `phoneNumber`.

## Events
- `GET /api/events`; `GET /api/events/{id}` — public.
- `POST /api/events` — Organiser; `name`, `description`, `eventDate`, `location`, `distanceKm`, `eventType` (`Run`, `Walk`, `Cycle`).
- `PUT /api/events/{id}` — owning Organiser, same body.
- `DELETE /api/events/{id}` — owning Organiser.

## Categories
- `GET /api/events/{eventId}/categories` — public.
- `POST /api/events/{eventId}/categories` — owning Organiser; `name`, `categoryType` (`Age` or `Distance`), plus age limits or `distanceKm`.
- `PUT /api/categories/{id}` and `DELETE /api/categories/{id}` — owning Organiser.

## Enrolments
- `POST /api/enrolments` — Participant; `eventId`, `categoryId`.
- `GET /api/events/{eventId}/enrolments` — owning Organiser.

## Results
- `POST /api/results` — owning Organiser; `enrolmentId`, `finishingPosition`, `finishTime` (TimeSpan JSON format such as `"00:32:15"`).
- `GET /api/results/me` — Participant.
- `GET /api/events/{eventId}/results` — owning Organiser.
