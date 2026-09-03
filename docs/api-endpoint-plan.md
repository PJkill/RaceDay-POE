# RaceDay API Endpoint Plan

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/auth/register | Register new user | Public | { email, password, fullName, role } | 201 Created / 400 Bad Request |
| POST | /api/auth/login | Login get JWT | Public | { email, password } | 200 OK with token / 401 |
| GET | /api/users/profile | Get my profile | Any logged in | None | 200 OK user / 401 |
| PUT | /api/users/profile | Update my profile | Any logged in | { fullName } | 200 OK / 400 |
| GET | /api/events | List all events | Any | None | 200 OK list |
| GET | /api/events/{id} | Get single event | Any | None | 200 OK / 404 |
| POST | /api/events | Create event | Organiser | { name, description, date, location } | 201 Created / 400 |
| PUT | /api/events/{id} | Update event | Organiser | { name, description, date, location } | 200 OK / 403 / 404 |
| DELETE | /api/events/{id} | Delete event | Organiser | None | 204 No Content / 403 / 404 |
| POST | /api/events/{eventId}/categories | Add category | Organiser | { name, description, entryFee } | 201 Created / 404 |
| PUT | /api/categories/{id} | Update category | Organiser | { name, entryFee } | 200 OK / 403 / 404 |
| DELETE | /api/categories/{id} | Delete category | Organiser | None | 204 No Content |
| GET | /api/enrolments/my | My enrolments | Participant | None | 200 OK list |
| POST | /api/events/{eventId}/enrol | Enrol in event | Participant | { categoryId } | 201 Created / 400 / 404 |
| DELETE | /api/enrolments/{id} | Cancel enrolment | Participant | None | 204 No Content / 403 / 404 |
| GET | /api/results/my | My results | Participant | None | 200 OK list |
| POST | /api/enrolments/{enrolmentId}/results | Add result | Organiser | { finishTime, position } | 201 Created / 404 / 409 |