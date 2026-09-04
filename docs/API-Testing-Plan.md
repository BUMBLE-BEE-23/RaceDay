# RaceDay API Testing Plan

Authentication tests:
- Registration succeeds with valid data.
- Registration rejects duplicate email addresses.
- Login succeeds with valid credentials.
- Login rejects invalid credentials.
- Protected endpoints reject unauthenticated requests.

Role tests:
- Organizer-only endpoints reject participants.
- Participant-only enrolment endpoints reject organizers.
- Users can update only their own profiles.
- Organizers cannot modify another organizer's events.

Enrolment tests:
- Participant can enrol in an event.
- Invalid category or event data is rejected.
- Organizer can view enrolments for their own event.

Result tests:
- Organizer can record a result for an enrolment belonging to their event.
- Participant can view their own result.
- Participant cannot view another participant's result.
