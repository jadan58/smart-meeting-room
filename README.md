> **Smart Space: Smarter Meetings, Better Collaboration**
>
> The **Smart Space API** is the backbone of a modern meeting management and room booking solution designed for organizations that value time, productivity, and seamless collaboration. With powerful scheduling, action tracking, and analytics, Smart Space transforms how teams organize and manage their workspace.
>
> From **creating recurring meetings** to **assigning tasks**, **tracking attendance**, and even **ranking top organizers**, this API equips your apps with everything needed to supercharge productivity.

---

## Base URL

```
https://yourdomain.com/api
```

All endpoints are prefixed with `/api`. Use `Authorization: Bearer <token>` for endpoints that require authentication.

---

## Conventions

* **{param}** — path parameter
* **?query=** — query parameter
* **Auth** — Bearer JWT required unless noted otherwise
* **Roles** — where applicable, indicate likely role requirements (Admin, Organizer, etc.) — check your authorization policies for exact details
* Common response codes: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Server Error`.

---

# Full Endpoint List

> Endpoints grouped by resource. Each line: **METHOD path — short description**

---

## Authentication & Authorization (`/Auth`)

* `POST /Auth/register` — Register a new user (Admin)
* `POST /Auth/login` — Authenticate user and return JWT token
* `PUT /Auth/role/{id}` — Update the role(s) of a user (Admin)
* `POST /Auth/change-password` — Change password for current authenticated user 
* `DELETE /Auth/{id}` — Delete a user account by id (Admin)

---

## Features (`/Features`)

* `GET /Features` — Get list of all features 
* `POST /Features` — Create a new feature (Admin)
* `GET /Features/{id}` — Get a single feature by id
* `PUT /Features/{id}` — Update a feature (Admin)
* `DELETE /Features/{id}` — Delete a feature (Admin)

---

## Files (`/files`)

> Files are typically served from storage with proper authorization checks.

* `GET /files/meetings/{meetingId}/{fileName}` — Download or stream a meeting-related file (Attendees)
* `GET /files/action-items/{itemId}/assignment/{fileName}` — Download assignment attachment for an action-item (Attendees - assigner + assignee)
* `GET /files/action-items/{itemId}/submission/{fileName}` — Download submission attachment for an action-item (Attendees - assigner + assignee)

---

## Meetings (`/Meeting`)

* `GET /Meeting` — List all meetings (Admin)
* `POST /Meeting` — Create a meeting (Admin , Employee)
* `GET /Meeting/{id}` — Get meeting details by id (Attendee)
* `PUT /Meeting/{id}` — Update a meeting (Organizer)
* `DELETE /Meeting/{id}` — Delete a meeting (Organizer)
* `POST /Meeting/recurring` — Create recurring meeting(s) or recurring rule (Admin, Employee)

### Meeting — Notes

* `POST /Meeting/{meetingId}/notes` — Add a note to a meeting (Attendee)
* `PUT /Meeting/{meetingId}/notes/{noteId}` — Update a note (Note Owner)
* `DELETE /Meeting/{meetingId}/notes/{noteId}` — Delete a note (Organizer and Note Owner)

### Meeting — Action Items

* `POST /Meeting/{meetingId}/action-items` — Create an action item for a meeting (Organizer)
* `PUT /Meeting/{meetingId}/action-items/{itemId}/toggle-status` — Toggle action-item status (Assignee)
* `PUT /Meeting/{meetingId}/action-items/{itemId}/accept` — Mark action-item as accepted (Organizer)
* `PUT /Meeting/{meetingId}/action-items/{itemId}/reject` — Mark action-item as rejected (Organizer)
* `DELETE /Meeting/{meetingId}/action-items/{itemId}` — Delete an action item (Organizer)

#### Action Item Attachments

* `POST /Meeting/{meetingId}/action-items/{itemId}/assignment-attachments` — Upload assignment attachment(s) (files attached by assigner)
* `POST /Meeting/{meetingId}/action-items/{itemId}/submission-attachments` — Upload submission attachment(s) (files uploaded by assignee)

### Meeting — Invitees

* `POST /Meeting/{meetingId}/invitees` — Add invitee(s) to a meeting (Organizer)
* `DELETE /Meeting/{meetingId}/invitees/{inviteId}` — Remove an invitee (Organizer)
* `PUT /Meeting/{meetingId}/invitees/{inviteId}/accept` — Invitee accepts the invite 
* `PUT /Meeting/{meetingId}/invitees/{inviteId}/decline` — Invitee declines the invite

### Meeting — Attachments

* `POST /Meeting/{meetingId}/attachments` — Upload generic meeting attachments (presentations, minutes, etc.) (Organizer)

### Meeting — Analytics / Meta

* `GET /Meeting/count` — Get total meeting count
* `GET /Meeting/top-rooms` — Get most used rooms (top 3) for meetings analytics

---

## Notifications (`/Notifications`)

* `GET /Notifications` — List notifications for current user 
* `POST /Notifications` — Create/push a notification 
* `PUT /Notifications/mark-as-read/{id}` — Mark a notification as read (Notified User)
* `DELETE /Notifications/{id}` — Delete a notification

---

## Rooms (`/Room`)

* `GET /Room` — List all rooms (supports filters: capacity, features, availability)
* `POST /Room` — Create a new room (Admin)
* `GET /Room/{id}` — Get room details 
* `PUT /Room/{id}` — Update room meta (name, capacity, location, etc.) (Admin)
* `DELETE /Room/{id}` — Delete a room (Admin)

### Room — Features

* `POST /Room/{roomId}/features/{featureId}` — Add an existing feature to a room (Admin)
* `DELETE /Room/{roomId}/features/{featureId}` — Remove a feature from a room (Admin)

### Room — Images

* `POST /Room/{id}/upload-image` — Upload an image for room (Admin)
* `DELETE /Room/{id}/delete-image` — Delete room image (Admin)

### Room — Analytics

* `GET /Room/biggest-rooms` — Get rooms sorted by largest capacity or usage 
* `GET /Room/count` — Get total number of rooms

---

## Users (`/Users`)

* `GET /Users` — List users (Admin)
* `GET /Users/me` — Get profile of current authenticated user
* `GET /Users/{id}` — Get a user's public profile by id (Admin)
* `GET /Users/email/{email}` — Get a user by email (Admin)
* 
### User — Meetings & Invites

* `GET /Users/me/meetings/organized` — Meetings organized by current user
* `GET /Users/me/meetings/invited` — Meetings current user is invited to
* `GET /Users/me/meetings/all` — All meetings related to current user (organized + invited)
* `GET /Users/me/meetings/previous/all` — All past meetings (history) for current user
* `GET /Users/me/meetings/dailycount` — Daily meeting counts (time-series) for current user
* `GET /Users/me/meetings/heatmap` — Heatmap data showing meeting distribution
* `GET /Users/me/invites/pending` — Pending invites for current user
* `GET /Users/me/invites/accepted` — Accepted invites for current user

### Profile Media

* `POST /Users/me/upload-profile` — Upload profile picture for current user
* `DELETE /Users/me/delete-profile` — Delete profile picture for current user

### User Analytics

* `GET /Users/top-organizers` — Get top meeting organizers (leaderboard)

---

* Swagger / OpenAPI UI (commonly exposed at `/swagger` or `/swagger/index.html`) — developer UI for testing if enabled
* Static file routes for web client (e.g. `/` or `/index.html`) 

---

*Made with ❤️ by your API dev-pal — Smart Space: because meetings should empower, not exhaust.*
