# RaceDay API Endpoint Plan

**Database:** RaceDayDB
**Author:** Annie
**Part of:** RaceDay PoE – Part 1 (Planning), updated to match the Part 2 implementation

This document lists every planned REST API endpoint for the RaceDay Event Management system, based on the RaceDayDB schema (User, Venue, Event, Category, Enrolment, Result). This is a planning artifact 

**Roles:** None = public, no login required. Any = any logged-in user. Organiser / Participant = restricted to that role. Every endpoint except register and login returns 401 Unauthorized when nobody is logged in, and 403 Forbidden when the logged-in role is wrong.
**Authentication approach:** Server-side sessions. `POST /api/auth/login` creates a session that stores the user id and role (kept in a session cookie), and every later request is checked against that session. Passwords are hashed with BCrypt before they are stored.

---

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user as either an Organiser or a Participant | None | `{ name, email, password, role }` | 201 Created – user id and email returned. 400 Bad Request – invalid role or invalid fields. 409 Conflict – email already registered. |
| POST | /api/auth/login | Authenticates a user and starts a server-side session storing the user id and role | None | `{ email, password }` | 200 OK – user id, name, email and role returned, session created. 401 Unauthorized – invalid email or password. |
| POST | /api/auth/logout | Ends the current session | None | None | 200 OK – session cleared. |

---

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Retrieves the currently logged-in user's profile | Any | None | 200 OK – user profile returned. 401 Unauthorized – not logged in. |
| PUT | /api/users/me | Updates the currently logged-in user's name or email | Any | `{ name, email }` | 200 OK – updated profile returned. 400 Bad Request – invalid data. 409 Conflict – email already used by another account. |
| DELETE | /api/users/me | Deletes the currently logged-in user's account | Any | None | 204 No Content – account deleted. 401 Unauthorized – not logged in. 409 Conflict – the user still has events or enrolments. |

---

## Venues

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/venues | Lists all venues | Any | None | 200 OK – array of venues. |
| GET | /api/venues/{id} | Retrieves a single venue by id | Any | None | 200 OK – venue returned. 404 Not Found – venue does not exist. |
| POST | /api/venues | Creates a new venue | Organiser | `{ name, address, city }` | 201 Created – new venue returned. 400 Bad Request – missing required fields. 403 Forbidden – not an Organiser. |
| PUT | /api/venues/{id} | Updates an existing venue | Organiser | `{ name, address, city }` | 200 OK – updated venue returned. 404 Not Found – venue does not exist. |
| DELETE | /api/venues/{id} | Removes a venue | Organiser | None | 204 No Content – venue deleted. 404 Not Found – venue does not exist. 409 Conflict – venue is used by an event. |

---

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all events | Any | None | 200 OK – array of events. |
| GET | /api/events/{id} | Retrieves a single event by id | Any | None | 200 OK – event returned. 404 Not Found – event does not exist. |
| GET | /api/events/{id}/categories | Lists all categories belonging to a specific event | Any | None | 200 OK – array of categories. 404 Not Found – event does not exist. |
| GET | /api/events/{id}/results | Lists all results for a specific event (leaderboard) | Any | None | 200 OK – array of results. 404 Not Found – event does not exist. |
| POST | /api/events | Creates a new event, linking the logged-in organiser and a venue | Organiser | `{ venueId, name, eventDate, description, eventType, distanceKm }` | 201 Created – new event returned. 400 Bad Request – eventType is not Run, Walk or Cycle, or the venue does not exist. |
| PUT | /api/events/{id} | Updates an existing event | Organiser | `{ venueId, name, eventDate, description, eventType, distanceKm }` | 200 OK – updated event returned. 400 Bad Request – invalid eventType or venue. 403 Forbidden – not the owning organiser. 404 Not Found – event does not exist. |
| DELETE | /api/events/{id} | Removes an event | Organiser | None | 204 No Content – event deleted. 403 Forbidden – not the owning organiser. 404 Not Found – event does not exist. 409 Conflict – the event still has categories. |

---

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/categories | Lists all categories | Any | None | 200 OK – array of categories. |
| GET | /api/categories/{id} | Retrieves a single category by id | Any | None | 200 OK – category returned. 404 Not Found – category does not exist. |
| POST | /api/categories | Adds a new category to an event | Organiser | `{ eventId, name, distanceKm }` | 201 Created – new category returned. 403 Forbidden – not the owning organiser. 404 Not Found – event does not exist. |
| PUT | /api/categories/{id} | Updates an existing category | Organiser | `{ name, distanceKm }` | 200 OK – updated category returned. 403 Forbidden – not the owning organiser. 404 Not Found – category does not exist. |
| DELETE | /api/categories/{id} | Removes a category | Organiser | None | 204 No Content – category deleted. 403 Forbidden – not the owning organiser. 404 Not Found – category does not exist. 409 Conflict – the category has enrolments. |

---

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/enrolments | Lists the enrolments for the events owned by the logged-in organiser | Organiser | None | 200 OK – array of enrolments. |
| GET | /api/users/me/enrolments | Lists the logged-in participant's own enrolments | Participant | None | 200 OK – array of the participant's enrolments. |
| POST | /api/enrolments | Enrols the logged-in participant in a category | Participant | `{ categoryId }` | 201 Created – new enrolment returned. 404 Not Found – category does not exist. 409 Conflict – already enrolled in this category. |
| DELETE | /api/enrolments/{id} | Cancels an enrolment (own enrolment, or any if Organiser) | Any | None | 204 No Content – enrolment cancelled. 403 Forbidden – not the enrolment owner. 404 Not Found – enrolment does not exist. 409 Conflict – the enrolment already has a result. |

---

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/results | Lists all results | Any | None | 200 OK – array of results. |
| GET | /api/results/{id} | Retrieves a single result by id | Any | None | 200 OK – result returned. 404 Not Found – result does not exist. |
| GET | /api/users/me/results | Lists the logged-in participant's own results | Participant | None | 200 OK – array of the participant's results. |
| POST | /api/results | Captures a finish time and position for an enrolment | Organiser | `{ enrolmentId, finishTime, position }` | 201 Created – new result returned. 400 Bad Request – finishTime is not HH:MM:SS. 403 Forbidden – not the owning organiser. 404 Not Found – enrolment does not exist. 409 Conflict – a result already exists for this enrolment. |
| PUT | /api/results/{id} | Updates an existing result | Organiser | `{ finishTime, position }` | 200 OK – updated result returned. 400 Bad Request – invalid finishTime. 403 Forbidden – not the owning organiser. 404 Not Found – result does not exist. |
| DELETE | /api/results/{id} | Removes a result | Organiser | None | 204 No Content – result deleted. 403 Forbidden – not the owning organiser. 404 Not Found – result does not exist. |

---

## Summary

| Entity | Base Route | Relationships |
|---|---|---|
| User | /api/users, /api/auth | Referenced by Event (organiser), Enrolment (participant) |
| Venue | /api/venues | Referenced by Event |
| Event | /api/events | References User, Venue → referenced by Category |
| Category | /api/categories | References Event → referenced by Enrolment |
| Enrolment | /api/enrolments | References User, Category → referenced by Result |
| Result | /api/results | References Enrolment (1:1) |


---

## Appendix: Data Dictionary

| Field | Type | Notes |
|---|---|---|
| role | string | One of: "Organiser", "Participant" |
| password | string | Plaintext on input only; server hashes before storing as password_hash |
| eventDate | datetime | ISO 8601 format, e.g. 2026-10-15T06:00:00 |
| eventType | string | One of: "Run", "Walk", "Cycle" |
| distanceKm | decimal | Distance in kilometres (on an event and on a category), e.g. 42.2 |
| finishTime | time | Format HH:MM:SS, e.g. 01:45:30 |
| position | integer | Finishing rank within the category, starting at 1 |
