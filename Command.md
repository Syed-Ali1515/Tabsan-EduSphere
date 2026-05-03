# Command Center

## Purpose
Use this file as the single handover reference between sessions and devices.
Before starting any work, the assistant must:
1. Read this file.
2. Read Project startup Docs/Final-Touches.md.
3. Continue from the exact Current Execution Pointer below.

## Non-Negotiable Rule Per Completed Phase
After each completed phase, update all three files:
1. Docs/Function-List.md
2. Project startup Docs/PRD.md
3. Project startup Docs/Final-Touches.md

Also update this file with:
- completed work
- validation summary
- next steps
- pending extras

---

## Current Execution Pointer
- Plan Source: Project startup Docs/Final-Touches.md
- Active Phase: Phase 1 - Navigation, Session Stability, Sidebar Structure
- Active Stage: Stage 1.1 - Fix Session/Sidebar Reset Bug
- Status: Not Started
- Last Updated: 2026-05-03

## Immediate Next Steps
1. Reproduce Buildings page issue where sidebar resets to legacy menu and session appears broken.
2. Fix dynamic sidebar/session persistence so navigation does not fall back to legacy layout.
3. Validate SuperAdmin sees complete sidebar after navigation and refresh.
4. Start Stage 1.2 grouping strategy (Student/Faculty/Finance/Settings).

## Pending Extra Tasks (Cross-Phase)
- Keep Report Center menu visible by role and working links.
- Ensure all menus are assignable in Sidebar Settings.
- Ensure export button text and actions are validated when reporting phase is executed.
- Keep test credentials and run commands verified after major backend changes.

---

## Session Resume Prompt Template
Copy/paste this in a new chat:

Resume from Command.md and Final-Touches.md.
Continue from Current Execution Pointer.
Do not replan completed items.
When a phase is completed, update:
- Docs/Function-List.md
- Project startup Docs/PRD.md
- Project startup Docs/Final-Touches.md
- Command.md

---

## Work Log

### Entry 001
- Date: 2026-05-03
- Action: Created Command.md as persistent handover controller.
- Changes:
  - Added execution pointer linked to Final-Touches Phase 1 Stage 1.1.
  - Added mandatory documentation update rule per completed phase.
  - Added resume template and cross-phase pending extras list.
- Validation:
  - File created successfully.
  - Paths and phase names align with Final-Touches.md.
- Next:
  - Begin implementation of Phase 1 Stage 1.1.
