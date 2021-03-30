# Change Log

Note: These are condensed summaries, not a full list of all changes

## Version 0.3.4 alpha 2

- Rewrote region generation system to generate more geographically consistent and well formed regions
- Rewrote entity Id system which now allows the simulation to encompass a much much larger timeline
- Completely converted all hard coded decisions into mod scripts
- Improved the presentation of polity territories
- Land areas fully encompassed by a polity's territory are now considered part of that same territory (but only if depopulated)
- Rewrote the entire population migration system to separate migrating groups by polity membership (or lack thereof)
- Increased the speed at which population groups migrate
- Removed polity influence transference, now polities grow exclusively through migration
- Polity populations are now limited to migrate only within a polity's core regions, with each polity starting with a single core region
- Added scriptable player actions and the action toolbar
- Added a player action that allows a polity to add neighboring regions to their set of core regions
- Added an event to allow AI-controlled polities to also add core regions
- Removed the highly repetitive and annoying event-decision chain to increase a tribe's openness (no longer needed)
- Reduced simulation performance issues tied to decision and event evaluations
- Multiple small tweaks and fixes

## Version 0.3.4 alpha 1

- Added new modding framework for decisions and events
- Added semi-complete modding guide
- Fixed Image Export
- Can export zoomed in map images
- Improved Drainage Basin Generation

## Version 0.3.3 and before

To be written...
