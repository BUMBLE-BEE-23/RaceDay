# RaceDay Role and Permission Matrix

| Feature | Organizer | Participant |
|---|---|---|
| Register | Yes | Yes |
| Login | Yes | Yes |
| View own profile | Yes | Yes |
| Update own profile | Yes | Yes |
| View events | Yes | Yes |
| Create events | Yes | No |
| Update own events | Yes | No |
| Delete own events | Yes | No |
| View event types | Yes | Yes |
| Manage categories for own events | Yes | No |
| View categories | Yes | Yes |
| Enrol in events | No | Yes |
| View own enrolments | No | Yes |
| View enrolments for owned events | Yes | No |
| Record results for owned events | Yes | No |
| View own results | No | Yes |

Ownership checks will be enforced by the API using the authenticated user's ID.
