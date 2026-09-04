# RaceDay Authentication Plan

Authentication uses registration and login endpoints.

Registration:
- POST /api/auth/register
- Public endpoint
- Accepts first name, last name, email, password and role.
- Email addresses must be unique.
- Passwords will be stored as hashes rather than plain text.

Login:
- POST /api/auth/login
- Public endpoint
- Validates email and password.
- Returns an authentication token on success.

Protected endpoints will require authentication. The authenticated user's role will determine which operations are permitted.
