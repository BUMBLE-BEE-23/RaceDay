# RaceDay ERD Design Decisions

The RaceDay ERD uses six related entities.

Users stores both organizers and participants. The Role attribute determines the user's system permissions.

Events belongs to an organizer and an event type.

Categories belongs to an event and defines the age and distance requirements for participation.

Enrollments connects participants to events and categories.

Results belongs to an enrollment and stores the finishing time and finishing position.

The relationships are primarily one-to-many. An enrollment can have zero or one result because a participant may be enrolled before completing the event.
