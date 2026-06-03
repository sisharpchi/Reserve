# ReserveFlow: UI/UX Design System

This document outlines the design tokens, component specifications, theme parameters, and visual status states for the ReserveFlow UI.

---

## 1. Design Style & Palette (Sleek Dark/Light Mode)
ReserveFlow uses a premium, high-contrast, modern interface design featuring clean typography, glassmorphism panel accents, and harmonic colors defined in HSL format for easy opacity manipulation.

### Color Tokens

| Token | Light Mode Value (HSL) | Dark Mode Value (HSL) | Description |
| :--- | :--- | :--- | :--- |
| **`--bg-primary`** | `210, 20%, 98%` | `222, 47%, 11%` | Global body background |
| **`--bg-card`** | `0, 0%, 100%` | `217, 33%, 17%` | Dialog boxes, tables, sidebars, cards |
| **`--text-primary`**| `222, 47%, 11%` | `210, 40%, 98%` | Titles, primary label text |
| **`--text-secondary`**|`215, 16%, 47%` | `215, 20%, 65%` | Subtitles, helper text |
| **`--brand-primary`**|`262, 83%, 58%` | `263, 90%, 65%` | Premium Indigo (CTAs, selected states) |
| **`--brand-accent`** |`190, 95%, 45%` | `190, 100%, 50%` | Teal accent highlight |
| **`--border-color`** |`214, 32%, 91%` | `217, 19%, 27%` | Structural container dividers |

---

## 2. Status & Badge Colors
Colors correspond strictly to entity lifecycles to ensure visual clarity.

### Booking Status

| Status | Color | UI Element Presentation |
| :--- | :--- | :--- |
| **`Pending`** | Yellow | Subtle amber text, solid amber outline, amber dotted pulse dot. |
| **`Confirmed`** | Blue | Bright blue pill badge, solid outline. |
| **`Rescheduled`**| Purple | Lavender background, dark purple text, swap/arrow icon. |
| **`Cancelled`** | Red | Muted red background, red strikethrough text. |
| **`Completed`** | Green | Forest green pill, checkmark icon. |
| **`NoShow`** | Orange | Dark orange background, high-contrast text. |
| **`Expired`** | Gray | Muted gray pill background, strike-out text. |

### Tenant Status

| Status | Color | Description |
| :--- | :--- | :--- |
| **`Pending`** | Yellow | Awaiting platform activation or setup. |
| **`Active`** | Green | Public pages route, API responds successfully. |
| **`Suspended`** | Red | Suspended banner shown, public bookings blocked. |
| **`Deleted`** | Gray | Soft delete state, metadata preserved for audit trails. |

---

## 3. Typography & Spacing
* **Font Family:** Primary is **Inter** (sans-serif) loaded via Google Fonts; Monospace font is **JetBrains Mono** for code IDs, correlation tokens, and dates.
* **Scale:**
  * `Heading 1 (h1)`: `2.25rem / 36px`, Bold
  * `Heading 2 (h2)`: `1.5rem / 24px`, Semi-Bold
  * `Body Regular`: `1rem / 16px`, Regular
  * `Caption / Small`: `0.875rem / 14px`, Medium
* **Spacing Scale:** Multiple of `4px` (8px, 12px, 16px, 24px, 32px, 48px, 64px) implemented using Tailwind utility tags (`p-2`, `m-4`, `gap-6`).

---

## 4. Primitive Components Design

### Buttons (`shared/ui/button`)
* **Primary:** Filled `--brand-primary` with smooth transition on hover (`transition: all 0.2s ease`).
* **Secondary:** Border outline using `--border-color`, background fills with light gray/dark gray on hover.
* **Danger:** Red background (`hsl(0, 72%, 51%)`) for destructive cancellations, complete with a confirmation dialog hook.

### Tables (`shared/table`)
* Borderless modern design, `--bg-card` background, subtle row border-bottom.
* **Interactive Column Headers:** Table headers for sortable fields display a subtle up/down caret.
* Hover effect adds a light scale transform or background tint to rows.

### Booking Slot Cards (`shared/calendar`)
* Available slots are displayed as rounded square cards.
* **Unselected:** Transparent background, primary border color, brand color text.
* **Selected:** Solid `--brand-primary` background, white text, scale-up pop micro-animation (`transform: scale(1.03)`).
* **Hover:** Slide highlight background, scale animation.
* **Disabled / Unavailable:** Strikethrough text, gray background, dotted border, pointer-events-none.

---

## 5. Skeleton States & Toasts
* **Loading Skeleton Loader:** Layout components containing gray blocks with a pulse animation (`animate-pulse`).
* **Interactive Dialogs (Modals):** Glassmorphic backdrop filter (`backdrop-blur-md`) blur-covering the background page, modal content sliding up from the bottom center.
* **System Toasts:** Displayed in the top-right corner. Automatically dismisses in 4 seconds. Features a status icon (success, error, warning).
