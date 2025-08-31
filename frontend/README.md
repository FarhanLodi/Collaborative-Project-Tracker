# Collaborative Project Tracker - Frontend

## Overview
Angular 20 frontend for the Project Tracker. Works with the API to provide a Kanban board, role-aware UI, and real-time updates.

### Roles
- Project Owner: full control (create/edit/delete tasks and projects)
- Employee: sees only assigned tasks and can move them between statuses

## Setup & Run

```bash
npm install
npm start
```

The app expects API at `https://localhost:7177` by default. Configure via `src/environments/environment.ts` (`apiBaseUrl`).

## Tech Stack

 - Angular 20 (Standalone + Signals)
 - Angular CDK Drag & Drop (Kanban)
 - Bootstrap 5 + Font Awesome
  

## Features
 - Auth (Register/Login) with JWT
 - Dashboard: list/create projects, join via invite code, copy invite, owner-only delete
 - Project Detail: Kanban board, attachments, realtime updates via SignalR
 - Role-aware UI: owner-only controls; employees see only their tasks

## Build
- `npm run build`
