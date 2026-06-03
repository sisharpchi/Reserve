# ReserveFlow: Frontend Feature Plan

This document details the layout, behaviors, and components required for all target features in the ReserveFlow frontend.

---

## 1. Public Features (`features/public/*`)

### Home / Marketplace
* **Objective:** Discover local scheduling businesses.
* **Layout:** Centered search banner, category grid cards, list of popular tenants.
* **Actions:** Search tenants by text, browse categories.

### Category Page (`/categories/:categorySlug`)
* **Objective:** View tenants filtered by category.
* **Layout:** Left sidebar filters, grid layout of tenant cards with logos, descriptions, and ratings.

### Tenant Profile Page (`/t/:tenantSlug`)
* **Objective:** Public-facing page for a specific tenant business.
* **Layout:** Header image, logo, business details (hours, description, address), service list organized by category.
* **Actions:** Click "Book" on a service card.

### Booking Wizard (`/t/:tenantSlug/book`)
* **Objective:** Guided booking process.
* **Layout:** Multi-step stepper component:
  1. **Service Selection:** Preselected or editable service list.
  2. **Provider Selection:** Choose a specific staff member or select "Any Available".
  3. **Time Picker:** Interactive date calendar + responsive time-slot grid.
  4. **Customer Info Form:** Fields for Name, Email, Phone, and special requirements.
  5. **Review & Confirm:** Summary card showing service, staff, date, slot, and price.
* **UI States:** Loading skeletons for slots, empty state if no times are available, dynamic booking failure error alerts.

### Booking Confirmation (`/booking/:bookingCode`)
* **Objective:** Success notification page for customers.
* **Layout:** Confetti effect (optional), booking code badge, map direction placeholder, and actions.
* **Actions:** Print confirmation, copy code, "Add to Calendar" (.ics), cancel/reschedule.

### Customer Booking Lookup (`/booking/lookup`)
* **Objective:** Retrieve details of a booking without logging in.
* **Form:** Input field for Booking Code and Email/Phone.
* **Validation:** Both fields are required. Displays error if not found.

---

## 2. Authentication Features (`features/auth/*`)
Since Keycloak is the identity provider, standard login/register routes initiate redirection:
* **Login Redirector:** Redirects users to the Keycloak client login screen.
* **Register Redirector:** Navigates to Keycloak registration page.
* **Profile Management (`/customer/profile`):** Connects to the Keycloak account console interface for profile updates and password changes.

---

## 3. Platform Admin Features (`features/platform-admin/*`)

### Dashboard
* **KPI Cards:** Total Active Tenants, Total Bookings, Month-over-Month Growth, Active Users.
* **Charts:** Active booking volume over time, tenant sign-ups.

### Tenant Management
* **List View:** Table of tenants displaying Name, Slug, Category, Creation Date, Status (`Pending`, `Active`, `Suspended`), and Owner Admin.
* **Create Form:** Modal or slide-out form with Slug validation (automatic kebab-case typing).
* **Toggle Actions:** One-click activate/suspend buttons with warning modal.

### Category Management
* **Layout:** List table of categories (e.g., Medical, Wellness, Barber, Coworking).
* **Actions:** Inline add, edit, and delete category icons.

### Platform Audit Logs
* **Layout:** Table of platform events detailing Date, Actor User, Event Type, Target Tenant, and Description.

---

## 4. Tenant Admin Features (`features/tenant-admin/*`)

### Dashboard
* **KPI Cards:** Active Bookings Today, Staff Utilization Rate, Revenue Estimate, Cancel/No-Show rates.
* **Quick List:** List of upcoming appointments.

### Services CRUD
* **Layout:** Cards representing services with details (Price, Duration, Resources required).
* **Form:** Reactive form with validations for Name (required), Price (positive number), Duration (in 15-minute increments), and Buffer time.

### Staff CRUD
* **Layout:** Grid of staff members, names, roles, and status.
* **Form:** First name, last name, email, OIDC account mapping field, and toggle active.

### Resources CRUD
* **Layout:** Equipment and rooms inventory table.
* **Form:** Resource Name, Category (Room, Chair, Machine), and capacity.

### Schedule & Hours Editor
* **Layout:** Weekly Grid Editor.
* **Actions:** Slider inputs to set start/end times per day, toggle days off, and log one-off blackout dates.

### Booking Calendar & List
* **Layout:** Interactive Calendar (Day, Week, Month views) with color-coded slots representing booking statuses.
* **Details Drawer:** Clicking a slot pulls open a side drawer showing full customer info, service log, and status update actions.

---

## 5. Staff Features (`features/staff/*`)

### My Schedule
* **Layout:** Responsive weekly/daily agenda view optimized for mobile.
* **Actions:** View upcoming appointments, swipe to complete or mark no-show.

### Add Unavailable Period
* **Form:** Modal form. Select Start Date/Time, End Date/Time, and Reason (e.g., Lunch, Personal). Instantly updates available slots.

---

## 6. Customer Portal (`features/customer/*`)

### My Bookings
* **Layout:** Accordion list showing Active and Past Bookings.
* **Actions:** Request Cancellation (disabled if the cancellation deadline is passed), Reschedule Booking (opens slot picker).
