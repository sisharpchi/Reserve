# ReserveFlow: Frontend Task Board

This document maps out the specific implementation tasks, priorities, dependencies, and verification criteria for each milestone.

| Task ID | Area | Priority | Task Title | Description | Dependencies | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **FE-M0-001** | Core | P0 | Init Angular Workspace | Scaffold Angular 21 project with strict type configuration and routing. | None | Application compiles; runs locally. |
| **FE-M0-002** | Core | P0 | Setup Base Routes | Configure basic app routes for public, auth, platform admin, tenant admin, staff, and customer layouts. | FE-M0-001 | Routes load lazy-loaded component placeholders. |
| **FE-M0-003** | Core | P0 | Configure Layouts | Build core layout frames: `PublicLayout`, `AdminLayout`, `StaffLayout`, `AuthLayout`. | FE-M0-002 | Layouts wrap child components correctly. |
| **FE-M0-004** | Core | P1 | Dynamic App Config | Configure environment loaders to resolve `apiUrl` dynamically from Nginx config at runtime. | None | API base URL changes without rebuilding code assets. |
| **FE-M0-005** | Shared | P1 | Core Design System Atoms | Write base styling components: Button, Input, Select, Badge, Skeleton loader, Modal container. | None | Storybook/Demo page renders elements correctly in dark and light modes. |
| **FE-M1-001** | Auth | P0 | Keycloak Client Init | Incorporate Keycloak JS OIDC adapter using an Angular `APP_INITIALIZER`. | FE-M0-001 | App pauses bootstrap to initialize token checks. |
| **FE-M1-002** | Auth | P0 | Token Interceptor | Implement `HttpInterceptor` to attach active access tokens to outbound requests. | FE-M0-004 | Admin headers contain authorization bearer tokens. |
| **FE-M1-003** | Auth | P0 | Auth Route Guard | Create route guard redirecting unauthenticated traffic to Keycloak. | FE-M1-001 | Unauthorized page visits trigger login redirection. |
| **FE-M1-004** | Auth | P1 | Session Context Service | Call `GET /api/auth/me` to store local user metadata (roles, permissions, memberships) in Signals. | FE-M1-002 | Components resolve active permissions reactive. |
| **FE-M1-005** | Shared | P1 | Custom Permission Directive | Code structural directive `*rfHasPermission` to display or hide UI elements. | FE-M1-004 | Buttons are hidden if user lacks permission token. |
| **FE-M2-001** | Public | P0 | Public Home/Marketplace | Layout marketplace homepage showing categories and search bar. | FE-M0-003 | Page displays active tenant search inputs. |
| **FE-M2-002** | Public | P0 | Category Tenants List | Display list of tenants belonging to a selected category. | FE-M2-001 | Route updates filtering listings by category. |
| **FE-M2-003** | Public | P0 | Tenant Profile Page | Render public details of a business including service menus and contact info. | FE-M0-003 | Profile shows business info, hours, and service list. |
| **FE-M3-001** | Public | P0 | Stepper Booking Wizard | Code wizard flow (`Service -> Date -> Slot -> Info -> Success`). | FE-M2-003 | Submitting the form triggers booking creation API. |
| **FE-M3-002** | Public | P0 | Date & Time Picker UI | Build interactive date picker and grid displaying available slots. | FE-M3-001 | Calling availability API displays slots for selected day. |
| **FE-M3-003** | Public | P1 | Booking Confirmation Page | Display booking confirmation screen with unique booking code. | FE-M3-001 | Customer sees booking code and cancellation links. |
| **FE-M4-001** | Platform | P0 | Platform Admin Dashboard | Add dashboard layout with overview statistics. | FE-M1-004 | Platform admins see global KPI cards. |
| **FE-M4-002** | Platform | P0 | Tenant List & Management | Build tenant list table showing status tags and details. | FE-M4-001 | Admins can view and search all registered tenants. |
| **FE-M4-003** | Platform | P0 | Create Tenant Wizard | Build form to register new tenant with validation. | FE-M4-002 | Creating tenant fires provisioning workflow. |
| **FE-M4-004** | Platform | P1 | Tenant Status Controls | Add buttons to activate/suspend tenant with warning dialog. | FE-M4-002 | Suspending a tenant blocks its public pages instantly. |
| **FE-M5-001** | Tenant | P0 | Tenant Admin Dashboard | Build dashboard showing active bookings, utilization, and charts. | FE-M1-004 | Tenant admins see daily booking trends and alerts. |
| **FE-M5-002** | Tenant | P0 | Services CRUD View | Build table and form to add, edit, and remove services. | FE-M5-001 | Service form validates price, duration, and resources. |
| **FE-M5-003** | Tenant | P0 | Staff CRUD View | Build staff management panel. | FE-M5-001 | Admins can add staff and link them to services. |
| **FE-M5-004** | Tenant | P1 | Resources CRUD View | Build table to register physical spaces/assets. | FE-M5-001 | Admins can declare rooms, chairs, and devices. |
| **FE-M6-001** | Tenant | P0 | Working Hours Editor | Build weekly grid interface to set opening hours. | FE-M5-001 | Admins can modify shifts per weekday. |
| **FE-M6-002** | Tenant | P1 | Blackout Dates Planner | Add interface to register tenant holidays or staff leave. | FE-M6-001 | Blackout periods update availability calculations. |
| **FE-M6-003** | Tenant | P1 | Booking Policy Form | Build form to set cancellation deadlines and notification rules. | FE-M5-001 | Settings are saved and update booking validation. |
| **FE-M7-001** | Tenant | P0 | Booking Calendar Dashboard | Build calendar grid (day/week views) displaying bookings. | FE-M5-001 | Bookings are color-coded by status on the calendar. |
| **FE-M7-002** | Tenant | P0 | Booking Details Drawer | Create slide-out drawer showing customer info and booking actions. | FE-M7-001 | Drawer offers cancel, reschedule, and complete actions. |
| **FE-M7-003** | Tenant | P1 | Reschedule Booking Flow | Add workflow to move booking to a different slot. | FE-M7-002 | Selecting a new slot calls availability check before saving. |
| **FE-M8-001** | Staff | P0 | Staff Agenda Dashboard | Build mobile-responsive schedule listing appointments. | FE-M1-004 | Staff members see their daily/weekly bookings feed. |
| **FE-M8-002** | Staff | P1 | Complete/No-Show Actions | Add quick update buttons to mark appointments. | FE-M8-001 | Click action updates booking status. |
| **FE-M8-003** | Staff | P1 | Block Personal Time | Build form to log sudden personal leave. | FE-M8-001 | Blocks instantly remove staff slots from public view. |
| **FE-M9-001** | Tenant | P1 | Notification Logs View | Build list of sent/failed customer notifications from outbox. | FE-M5-001 | Admins see outbox delivery status and error messages. |
| **FE-M9-002** | Tenant | P1 | Retry Notification Action | Add button to retry failed outbox messages. | FE-M9-001 | Retry action triggers immediate resend attempt. |
| **FE-M9-003** | Tenant | P1 | Basic Report Visualizations | Build reports page with staff utilization charts. | FE-M5-001 | Dashboard displays graphical statistics. |
| **FE-M10-001**| Core | P0 | Global Error Boundaries | Implement global ErrorHandler to redirect page crashes. | FE-M0-001 | Intercepted frontend crashes display an error page. |
| **FE-M10-002**| Core | P0 | Responsive UX Pass | Audit and fix layouts on mobile/tablet viewports. | None | All views render properly on mobile screens. |
| **FE-M10-003**| Core | P1 | Production Build & Docker | Create production Nginx configurations and optimize bundles. | None | Build succeeds with optimized bundle sizes. |
