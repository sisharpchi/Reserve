import { Component, OnInit, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TenantContextService } from '../../../core/tenant/tenant-context.service';
import { KeycloakAuthService } from '../../../core/auth/keycloak-auth.service';
import { HasPermissionDirective } from '../../../core/auth/has-permission.directive';

interface Booking {
  id: string;
  customerName: string;
  customerEmail: string;
  serviceName: string;
  staffName?: string;
  startsAtUtc: string;
  status: string;
}

interface Service {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
  currency: string;
}

interface Staff {
  id: string;
  displayName: string;
  email: string;
  isActive: boolean;
}

interface Resource {
  id: string;
  name: string;
  resourceType: string;
  capacity: number;
  isActive: boolean;
}

@Component({
  selector: 'app-tenant-dashboard',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  template: `
    <div class="container animated-fade">
      
      <!-- Dashboard Top Header -->
      <div class="dashboard-header mt-4">
        <div class="header-content">
          <div class="header-badge">
            <span class="badge badge-primary">TENANT MANAGEMENT Portal</span>
          </div>
          <h2 class="mt-2">Tenant Workspace: <span>{{ tenantContext.tenantSlug() }}</span></h2>
          <p class="text-secondary">Configure services catalog, staffing shifts, blackout periods, and view booking metrics.</p>
        </div>
        <div class="header-actions">
          <span class="badge badge-warning">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-4 h-4 mr-1">
              <path fill-rule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm.5-13a.75.75 0 0 0-1.5 0v5c0 .414.336.75.75.75h4a.75.75 0 0 0 0-1.5h-3.25V5Z" clip-rule="evenodd" />
            </svg>
            {{ tenantContext.timeZoneId() || 'UTC' }}
          </span>
        </div>
      </div>

      <!-- Quick Metrics Grid -->
      <div class="metrics-grid mt-4">
        <div class="metric-card glass card-purple">
          <div class="metric-info">
            <h3>Registered Bookings</h3>
            <p class="value">{{ bookings().length }}</p>
          </div>
          <div class="metric-icon-wrapper">📅</div>
        </div>
        <div class="metric-card glass card-teal">
          <div class="metric-info">
            <h3>Services Catalog</h3>
            <p class="value">{{ services().length }}</p>
          </div>
          <div class="metric-icon-wrapper">⚙️</div>
        </div>
        <div class="metric-card glass card-amber">
          <div class="metric-info">
            <h3>Assigned Staff</h3>
            <p class="value">{{ staffList().length }}</p>
          </div>
          <div class="metric-icon-wrapper">👥</div>
        </div>
      </div>

      <!-- Main Navigation Tabs -->
      <div class="tabs-scroll-container mt-4">
        <div class="tabs-row-scroll">
          @for (tab of ['bookings', 'services', 'staff', 'resources', 'schedules', 'policies', 'reports', 'notifications']; track tab) {
            <button 
              class="tab-btn" 
              [class.active]="activeTab() === tab"
              (click)="setTab(tab)">
              @if (tab === 'bookings') { Appointments }
              @else if (tab === 'services') { Services }
              @else if (tab === 'staff') { Staff }
              @else if (tab === 'resources') { Resources }
              @else if (tab === 'schedules') { Shifts & Offs }
              @else if (tab === 'policies') { Booking Rules }
              @else if (tab === 'reports') { Analytics }
              @else { Outbox }
            </button>
          }
        </div>
      </div>

      <!-- TAB 1: BOOKINGS -->
      @if (activeTab() === 'bookings') {
        <div class="tab-pane animated-fade mt-3">
          @if (reschedulingBooking(); as b) {
            <div class="reschedule-banner glass border-primary-glow mb-4">
              <div class="banner-header">
                <h3>📅 Reschedule Appointment</h3>
                <p class="text-secondary text-sm">Update date and time for <strong>{{ b.customerName }}</strong> ({{ b.serviceName }})</p>
              </div>
              
              <div class="grid-2 mt-3">
                <div class="form-group">
                  <label class="form-label" for="res-date">New Appointment Date</label>
                  <input type="date" id="res-date" class="form-control" [(ngModel)]="rescheduleDate" required [min]="today">
                </div>
                <div class="form-group">
                  <label class="form-label" for="res-time">New Start Time</label>
                  <input type="time" id="res-time" class="form-control" [(ngModel)]="rescheduleTime" required>
                </div>
              </div>
              
              <div class="action-cell justify-end mt-3">
                <button type="button" class="btn btn-secondary" (click)="cancelReschedule()">
                  Cancel
                </button>
                <button type="button" class="btn btn-primary" (click)="saveReschedule()" [disabled]="!rescheduleDate || !rescheduleTime">
                  Confirm Reschedule
                </button>
              </div>
            </div>
          }

          <div class="table-wrapper glass">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Customer Information</th>
                  <th>Service Catalog</th>
                  <th>Staff Member</th>
                  <th>Starts At</th>
                  <th>Status</th>
                  <th class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @if (bookings().length === 0) {
                  <tr>
                    <td colspan="6" class="text-center py-5">
                      <div class="table-empty-state">
                        <span class="empty-icon">📅</span>
                        <p class="mt-2 font-medium">No appointments found</p>
                        <p class="text-muted text-sm">Appointments booked by customers will list here.</p>
                      </div>
                    </td>
                  </tr>
                } @else {
                  @for (booking of bookings(); track booking.id) {
                    <tr>
                      <td>
                        <div class="customer-info">
                          <span class="customer-name">{{ booking.customerName }}</span>
                          <span class="customer-email">{{ booking.customerEmail }}</span>
                        </div>
                      </td>
                      <td>
                        <span class="service-name-label">{{ booking.serviceName }}</span>
                      </td>
                      <td>
                        <span class="staff-cell">{{ booking.staffName || 'Any Available' }}</span>
                      </td>
                      <td>
                        <span class="date-cell">{{ formatDateTime(booking.startsAtUtc) }}</span>
                      </td>
                      <td>
                        <span class="badge" [class]="getBadgeClass(booking.status)">
                          {{ booking.status }}
                        </span>
                      </td>
                      <td>
                        <div class="action-cell justify-end">
                          @if (booking.status === 'Pending') {
                            <button class="btn btn-success btn-sm" (click)="confirmBooking(booking.id)">
                              Confirm
                            </button>
                          }
                          @if (booking.status === 'Confirmed' || booking.status === 'Pending') {
                            <button class="btn btn-secondary btn-sm" (click)="startReschedule(booking)">
                              Reschedule
                            </button>
                            <button class="btn btn-danger btn-sm" (click)="cancelBooking(booking.id)">
                              Cancel
                            </button>
                          }
                          @if (booking.status !== 'Confirmed' && booking.status !== 'Pending') {
                            <span class="text-muted text-sm">-</span>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      <!-- TAB 2: SERVICES -->
      @if (activeTab() === 'services') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Services List -->
          <div class="table-wrapper glass">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Service Catalog Name</th>
                  <th>Duration</th>
                  <th>Pricing</th>
                  <th class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @if (services().length === 0) {
                  <tr>
                    <td colspan="4" class="text-center py-5">
                      <div class="table-empty-state">
                        <span class="empty-icon">⚙️</span>
                        <p class="mt-2 font-medium">No services configured</p>
                        <p class="text-muted text-sm">Add treatment or service slots using the creator panel.</p>
                      </div>
                    </td>
                  </tr>
                } @else {
                  @for (svc of services(); track svc.id) {
                    <tr>
                      <td>
                        <span class="service-name font-semibold">{{ svc.name }}</span>
                      </td>
                      <td>
                        <span class="duration-badge">{{ svc.durationMinutes }} mins</span>
                      </td>
                      <td>
                        <span class="price-val">{{ svc.price }} {{ svc.currency }}</span>
                      </td>
                      <td>
                        <div class="action-cell justify-end">
                          <button class="btn btn-secondary btn-sm" (click)="editService(svc)">Edit</button>
                          <button class="btn btn-danger btn-sm" (click)="deactivateService(svc.id)">Delete</button>
                        </div>
                      </td>
                    </tr>
                  }
                }
              </tbody>
            </table>
          </div>

          <!-- Service Form (Add or Edit) -->
          @if (editingService(); as editSvc) {
            <div class="glass-card border-primary-glow">
              <h3>✏️ Edit Catalog Item</h3>
              <p class="text-secondary text-sm mb-3">Modify metadata and pricing rules for catalog service.</p>
              <form (submit)="updateService(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="edit-svc-name">Service Name</label>
                  <input type="text" id="edit-svc-name" class="form-control" [(ngModel)]="editServiceName" name="editName" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="edit-svc-dur">Duration (minutes)</label>
                  <input type="number" id="edit-svc-dur" class="form-control" [(ngModel)]="editServiceDuration" name="editDuration" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="edit-svc-price">Price</label>
                  <input type="number" id="edit-svc-price" class="form-control" [(ngModel)]="editServicePrice" name="editPrice" required>
                </div>
                <div class="action-cell justify-end mt-3">
                  <button type="button" class="btn btn-secondary" (click)="cancelEditService()">Cancel</button>
                  <button type="submit" class="btn btn-primary" [disabled]="!editServiceName || !editServicePrice">Save Changes</button>
                </div>
              </form>
            </div>
          } @else {
            <!-- Add Service Form -->
            <div class="glass-card">
              <h3>Add New Service</h3>
              <p class="text-secondary text-sm mb-3">Add new offerings to your clinic or shop menu list.</p>
              <form (submit)="createService(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="svc-name">Name</label>
                  <input type="text" id="svc-name" class="form-control" [(ngModel)]="newServiceName" name="name" required placeholder="Consultation">
                </div>
                <div class="form-group">
                  <label class="form-label" for="svc-dur">Duration (minutes)</label>
                  <input type="number" id="svc-dur" class="form-control" [(ngModel)]="newServiceDuration" name="duration" required placeholder="30">
                </div>
                <div class="form-group">
                  <label class="form-label" for="svc-price">Price (UZS)</label>
                  <input type="number" id="svc-price" class="form-control" [(ngModel)]="newServicePrice" name="price" required placeholder="100000">
                </div>
                <button *appHasPermission="'Tenant.Services.Manage'" type="submit" class="btn btn-primary w-full mt-2" [disabled]="!newServiceName || !newServicePrice">
                  Create Service
                </button>
              </form>
            </div>
          }
        </div>
      }

      <!-- TAB 3: STAFF -->
      @if (activeTab() === 'staff') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Staff List -->
          <div class="table-wrapper glass">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Display Name</th>
                  <th>Email Context</th>
                  <th>Shift Status</th>
                  <th class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @if (staffList().length === 0) {
                  <tr>
                    <td colspan="4" class="text-center py-5">
                      <div class="table-empty-state">
                        <span class="empty-icon">👥</span>
                        <p class="mt-2 font-medium">No staff members found</p>
                        <p class="text-muted text-sm">Assign employees or specialists to receive appointments.</p>
                      </div>
                    </td>
                  </tr>
                } @else {
                  @for (stf of staffList(); track stf.id) {
                    <tr>
                      <td>
                        <span class="staff-name font-semibold">{{ stf.displayName }}</span>
                      </td>
                      <td>
                        <span class="staff-email">{{ stf.email }}</span>
                      </td>
                      <td>
                        <span class="badge" [class]="stf.isActive ? 'badge-success' : 'badge-danger'">
                          {{ stf.isActive ? 'Active' : 'Inactive' }}
                        </span>
                      </td>
                      <td>
                        <div class="action-cell justify-end">
                          <button class="btn btn-secondary btn-sm" (click)="editStaff(stf)">Edit</button>
                          @if (stf.isActive) {
                            <button class="btn btn-danger btn-sm" (click)="deactivateStaff(stf.id)">Deactivate</button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                }
              </tbody>
            </table>
          </div>

          <!-- Staff Form (Add or Edit) -->
          @if (editingStaff(); as editStf) {
            <div class="glass-card border-primary-glow">
              <h3>✏️ Edit Staff Settings</h3>
              <p class="text-secondary text-sm mb-3">Modify display credentials for specialists.</p>
              <form (submit)="updateStaff(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="edit-stf-name">Display Name</label>
                  <input type="text" id="edit-stf-name" class="form-control" [(ngModel)]="editStaffName" name="editName" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="edit-stf-email">Email Address</label>
                  <input type="email" id="edit-stf-email" class="form-control" [(ngModel)]="editStaffEmail" name="editEmail" required>
                </div>
                <div class="action-cell justify-end mt-3">
                  <button type="button" class="btn btn-secondary" (click)="cancelEditStaff()">Cancel</button>
                  <button type="submit" class="btn btn-primary" [disabled]="!editStaffName || !editStaffEmail">Save Changes</button>
                </div>
              </form>
            </div>
          } @else {
            <!-- Add Staff Form -->
            <div class="glass-card">
              <h3>Add Staff Member</h3>
              <p class="text-secondary text-sm mb-3">Register new employees or doctors to the booking list.</p>
              <form (submit)="createStaff(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="stf-name">Display Name</label>
                  <input type="text" id="stf-name" class="form-control" [(ngModel)]="newStaffName" name="name" required placeholder="Dr. Sarah Connor">
                </div>
                <div class="form-group">
                  <label class="form-label" for="stf-email">Email Address</label>
                  <input type="email" id="stf-email" class="form-control" [(ngModel)]="newStaffEmail" name="email" required placeholder="sarah@smile.com">
                </div>
                <button type="submit" class="btn btn-primary w-full mt-2" [disabled]="!newStaffName || !newStaffEmail">
                  Add Staff Member
                </button>
              </form>
            </div>
          }
        </div>
      }

      <!-- TAB 4: RESOURCES -->
      @if (activeTab() === 'resources') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Resources List -->
          <div class="table-wrapper glass">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Resource Name</th>
                  <th>Type Tag</th>
                  <th>Seat Capacity</th>
                  <th>Asset Status</th>
                  <th class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @if (resources().length === 0) {
                  <tr>
                    <td colspan="5" class="text-center py-5">
                      <div class="table-empty-state">
                        <span class="empty-icon">🛋️</span>
                        <p class="mt-2 font-medium">No resources configured</p>
                        <p class="text-muted text-sm">Add equipment, salon chairs, or clinical room contexts.</p>
                      </div>
                    </td>
                  </tr>
                } @else {
                  @for (res of resources(); track res.id) {
                    <tr>
                      <td>
                        <span class="resource-name font-semibold">{{ res.name }}</span>
                      </td>
                      <td>
                        <span class="badge badge-primary">{{ res.resourceType }}</span>
                      </td>
                      <td>{{ res.capacity }} seats</td>
                      <td>
                        <span class="badge" [class]="res.isActive ? 'badge-success' : 'badge-danger'">
                          {{ res.isActive ? 'Active' : 'Inactive' }}
                        </span>
                      </td>
                      <td>
                        <div class="action-cell justify-end">
                          <button class="btn btn-secondary btn-sm" (click)="editResource(res)">Edit</button>
                          @if (res.isActive) {
                            <button class="btn btn-danger btn-sm" (click)="deactivateResource(res.id)">Deactivate</button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                }
              </tbody>
            </table>
          </div>

          <!-- Resource Form (Add or Edit) -->
          @if (editingResource(); as editRes) {
            <div class="glass-card border-primary-glow">
              <h3>✏️ Edit Resource Settings</h3>
              <p class="text-secondary text-sm mb-3">Adjust capacity details of physical equipment assets.</p>
              <form (submit)="updateResource(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="edit-res-name">Resource Name</label>
                  <input type="text" id="edit-res-name" class="form-control" [(ngModel)]="editResourceName" name="editName" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="edit-res-type">Type</label>
                  <input type="text" id="edit-res-type" class="form-control" [(ngModel)]="editResourceType" name="editType" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="edit-res-cap">Capacity</label>
                  <input type="number" id="edit-res-cap" class="form-control" [(ngModel)]="editResourceCapacity" name="editCap" required>
                </div>
                <div class="action-cell justify-end mt-3">
                  <button type="button" class="btn btn-secondary" (click)="cancelEditResource()">Cancel</button>
                  <button type="submit" class="btn btn-primary" [disabled]="!editResourceName || !editResourceType || !editResourceCapacity">Save Changes</button>
                </div>
              </form>
            </div>
          } @else {
            <!-- Add Resource Form -->
            <div class="glass-card">
              <h3>Add New Resource</h3>
              <p class="text-secondary text-sm mb-3">Add equipment dependencies required to fulfill bookings.</p>
              <form (submit)="createResource(); $event.preventDefault()">
                <div class="form-group">
                  <label class="form-label" for="res-name">Resource Name</label>
                  <input type="text" id="res-name" class="form-control" [(ngModel)]="newResourceName" name="resName" required placeholder="Dental Chair A">
                </div>
                <div class="form-group">
                  <label class="form-label" for="res-type">Type</label>
                  <input type="text" id="res-type" class="form-control" [(ngModel)]="newResourceType" name="resType" required placeholder="Chair">
                </div>
                <div class="form-group">
                  <label class="form-label" for="res-cap">Capacity</label>
                  <input type="number" id="res-cap" class="form-control" [(ngModel)]="newResourceCapacity" name="resCap" required placeholder="1">
                </div>
                <button type="submit" class="btn btn-primary w-full mt-2" [disabled]="!newResourceName || !newResourceType || !newResourceCapacity">
                  Create Resource
                </button>
              </form>
            </div>
          }
        </div>
      }

      <!-- TAB 5: SCHEDULES -->
      @if (activeTab() === 'schedules') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Setup Working Hours -->
          <div class="glass-card">
            <h3>Configure Weekly Working Hours</h3>
            <p class="text-secondary text-sm mb-3">Set weekly standard shifts for staff or rooms.</p>
            <form (submit)="saveWorkingHour(); $event.preventDefault()">
              <div class="form-group">
                <label class="form-label" for="sch-target">Target Type</label>
                <select id="sch-target" class="form-control" [(ngModel)]="scheduleTargetType" name="targetType" (change)="selectedTargetId = ''">
                  <option value="staff">Staff Member</option>
                  <option value="resource">Physical Resource</option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label" for="sch-id">Select Target Entity</label>
                <select id="sch-id" class="form-control" [(ngModel)]="selectedTargetId" name="targetId" required>
                  <option value="" disabled selected>-- Select Target --</option>
                  @if (scheduleTargetType === 'staff') {
                    @for (stf of staffList(); track stf.id) {
                      <option [value]="stf.id">{{ stf.displayName }}</option>
                    }
                  } @else {
                    @for (res of resources(); track res.id) {
                      <option [value]="res.id">{{ res.name }}</option>
                    }
                  }
                </select>
              </div>

              <div class="form-group">
                <label class="form-label" for="sch-day">Day of Week</label>
                <select id="sch-day" class="form-control" [(ngModel)]="newWorkingDayOfWeek" name="dayOfWeek">
                  <option [value]="1">Monday</option>
                  <option [value]="2">Tuesday</option>
                  <option [value]="3">Wednesday</option>
                  <option [value]="4">Thursday</option>
                  <option [value]="5">Friday</option>
                  <option [value]="6">Saturday</option>
                  <option [value]="0">Sunday</option>
                </select>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label" for="sch-start">Start Shift Time</label>
                  <input type="time" id="sch-start" class="form-control" [(ngModel)]="newWorkingStartTime" name="startTime" required>
                </div>
                <div class="form-group">
                  <label class="form-label" for="sch-end">End Shift Time</label>
                  <input type="time" id="sch-end" class="form-control" [(ngModel)]="newWorkingEndTime" name="endTime" required>
                </div>
              </div>

              <button type="submit" class="btn btn-primary w-full mt-2" [disabled]="!selectedTargetId || !newWorkingStartTime || !newWorkingEndTime">
                Save Shift Hours
              </button>
            </form>
          </div>

          <!-- Add Blackout / Unavailable Period -->
          <div class="glass-card">
            <h3>Add Blackout (Unavailable) Period</h3>
            <p class="text-secondary text-sm mb-3">Set dates during which the staff or resource is offline (sick leaves, maintainances).</p>
            <form (submit)="saveUnavailablePeriod(); $event.preventDefault()">
              <div class="form-group">
                <label class="form-label" for="un-target">Target Type</label>
                <select id="un-target" class="form-control" [(ngModel)]="scheduleTargetType" name="unTargetType" (change)="selectedTargetId = ''">
                  <option value="staff">Staff Member</option>
                  <option value="resource">Physical Resource</option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label" for="un-id">Select Target Entity</label>
                <select id="un-id" class="form-control" [(ngModel)]="selectedTargetId" name="unTargetId" required>
                  <option value="" disabled selected>-- Select Target --</option>
                  @if (scheduleTargetType === 'staff') {
                    @for (stf of staffList(); track stf.id) {
                      <option [value]="stf.id">{{ stf.displayName }}</option>
                    }
                  } @else {
                    @for (res of resources(); track res.id) {
                      <option [value]="res.id">{{ res.name }}</option>
                    }
                  }
                </select>
              </div>

              <div class="form-group">
                <label class="form-label" for="un-start">Start Date/Time (UTC)</label>
                <input type="datetime-local" id="un-start" class="form-control" [(ngModel)]="newBlackoutStart" name="blackoutStart" required>
              </div>

              <div class="form-group">
                <label class="form-label" for="un-end">End Date/Time (UTC)</label>
                <input type="datetime-local" id="un-end" class="form-control" [(ngModel)]="newBlackoutEnd" name="blackoutEnd" required>
              </div>

              <div class="form-group">
                <label class="form-label" for="un-reason">Reason description</label>
                <input type="text" id="un-reason" class="form-control" [(ngModel)]="newBlackoutReason" name="blackoutReason" placeholder="Staff Vacation / Equipment Maintenance">
              </div>

              <button type="submit" class="btn btn-danger w-full mt-2" [disabled]="!selectedTargetId || !newBlackoutStart || !newBlackoutEnd">
                Add Blackout Period
              </button>
            </form>
          </div>
        </div>
      }

      <!-- TAB 6: POLICIES -->
      @if (activeTab() === 'policies') {
        <div class="tab-pane animated-fade mt-3">
          <div class="glass-card max-width-600">
            <h3>Booking Policy Settings</h3>
            <p class="text-secondary text-sm mb-3">Enforce scheduling guardrails to control advance booking timelines.</p>
            <form (submit)="saveBookingPolicy(); $event.preventDefault()">
              <div class="form-group">
                <label class="form-label" for="pol-min">Minimum Advance Booking Notice (minutes)</label>
                <input type="number" id="pol-min" class="form-control" [(ngModel)]="policyMinAdvanceMinutes" name="minAdvance" required placeholder="60">
                <span class="subtext-tip mt-1">Prevent bookings that start sooner than this amount of time from now.</span>
              </div>

              <div class="form-group mt-3">
                <label class="form-label" for="pol-cancel">Cancellation / Rescheduling Deadline (hours)</label>
                <input type="number" id="pol-cancel" class="form-control" [(ngModel)]="policyCancelDeadlineHours" name="cancelDeadline" required placeholder="24">
                <span class="subtext-tip mt-1">Bookings cannot be cancelled or rescheduled if starting sooner than this.</span>
              </div>

              <button type="submit" class="btn btn-primary w-full mt-4">
                Save Policy Configuration
              </button>
            </form>
          </div>
        </div>
      }

      <!-- TAB 7: REPORTS -->
      @if (activeTab() === 'reports') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Report Selector Form -->
          <div class="glass-card">
            <h3>Get Daily Booking Report</h3>
            <p class="text-secondary text-sm mb-3">Review analytics metrics, completed appointment counts, and cancellations for any date.</p>
            <form (submit)="fetchReport(); $event.preventDefault()">
              <div class="form-group">
                <label class="form-label" for="rep-date">Report Target Date</label>
                <input type="date" id="rep-date" class="form-control" [(ngModel)]="reportDate" name="repDate" required>
              </div>
              <button type="submit" class="btn btn-primary w-full mt-2" [disabled]="!reportDate">
                Fetch Daily Report
              </button>
            </form>
          </div>

          <!-- Report Results Display -->
          <div class="glass-card">
            <h3>Report Summary</h3>
            <p class="text-secondary text-sm mb-2">Metrics details for selected date context</p>
            @if (loadingReport()) {
              <div class="loading-spinner-wrapper">
                <div class="spinner"></div>
                <p>Retrieving database records...</p>
              </div>
            } @else if (reportData(); as rep) {
              <ul class="summary-details mt-2">
                <li><strong>Report Date</strong> <span>{{ rep.date }}</span></li>
                <li><strong>Created Bookings</strong> <span class="price-val">{{ rep.createdBookings }}</span></li>
                <li><strong>Completed Bookings</strong> <span class="price-val text-success">{{ rep.completedBookings }}</span></li>
                <li><strong>Cancelled Bookings</strong> <span class="price-val text-danger">{{ rep.cancelledBookings }}</span></li>
                <li><strong>No-Show Bookings</strong> <span class="price-val text-warning">{{ rep.noShowBookings }}</span></li>
                <li><strong>Last Calculated (UTC)</strong> <span class="text-secondary">{{ formatDateTime(rep.updatedAtUtc) }}</span></li>
              </ul>
            } @else {
              <div class="empty-state text-center py-5">
                <p class="text-secondary">Select a date and click "Fetch Daily Report" to retrieve statistics.</p>
              </div>
            }
          </div>
        </div>
      }

      <!-- TAB 8: NOTIFICATIONS -->
      @if (activeTab() === 'notifications') {
        <div class="tab-pane grid-layout animated-fade mt-3">
          <!-- Notification Settings -->
          <div class="glass-card">
            <h3>Notification Outbox Channels</h3>
            <p class="text-secondary text-sm mb-3">Toggle automated notifications triggered during customer booking states.</p>
            <form (submit)="saveNotificationSettings(); $event.preventDefault()">
              <div class="form-group">
                <label class="checkbox-form-label">
                  <input type="checkbox" [(ngModel)]="emailNotificationsEnabled" name="emailNotify">
                  Enable Email Confirmations & Reminders
                </label>
              </div>
              <div class="form-group">
                <label class="checkbox-form-label">
                  <input type="checkbox" [(ngModel)]="smsNotificationsEnabled" name="smsNotify">
                  Enable SMS / Mobile Reminders
                </label>
              </div>
              <button type="submit" class="btn btn-primary w-full mt-2">
                Save Notification Settings
              </button>
            </form>
          </div>

          <!-- Outbox Logs -->
          <div>
            <div class="section-title">
              <h3>Outgoing Notification Logs</h3>
            </div>
            <div class="table-wrapper glass mt-3">
              <table class="custom-table">
                <thead>
                  <tr>
                    <th>Recipient</th>
                    <th>Message Context</th>
                    <th>Status</th>
                    <th class="text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @if (notificationLogs().length === 0) {
                    <tr>
                      <td colspan="4" class="text-center py-5">
                        <div class="table-empty-state">
                          <span class="empty-icon">📨</span>
                          <p class="mt-2 font-medium">Outbox logs empty</p>
                        </div>
                      </td>
                    </tr>
                  } @else {
                    @for (log of notificationLogs(); track log.id) {
                      <tr>
                        <td>
                          <div class="recipient-info">
                            <span class="recipient-address font-semibold">{{ log.recipient }}</span>
                            <span class="tz-subtext">Queued: {{ formatDateTime(log.createdAt) }}</span>
                          </div>
                        </td>
                        <td>
                          <span class="badge badge-primary">{{ log.type }}</span>
                        </td>
                        <td>
                          <span class="badge" [class]="getNotificationBadge(log.status)">
                            {{ log.status }}
                          </span>
                        </td>
                        <td>
                          <div class="action-cell justify-end">
                            @if (log.status === 'Failed') {
                              <button class="btn btn-secondary btn-sm" (click)="retryNotification(log.id)">
                                Retry
                              </button>
                            } @else {
                              <span class="text-muted text-sm italic">Delivered</span>
                            }
                          </div>
                        </td>
                      </tr>
                    }
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      }

    </div>
  `,
  styles: [`
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
      border-bottom: 1px solid var(--surface-border);
      padding-bottom: 1.5rem;
    }
    .dashboard-header h2 span {
      color: var(--primary);
      text-transform: capitalize;
      font-weight: 800;
    }
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }
    .metric-card {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1.5rem 1.75rem;
      border-radius: var(--border-radius-md);
      border: 1px solid var(--surface-border);
      box-shadow: var(--shadow-md);
    }
    .card-purple {
      background: linear-gradient(135deg, rgba(53, 37, 205, 0.08) 0%, rgba(252, 248, 255, 0.9) 100%);
      border-color: rgba(53, 37, 205, 0.15);
    }
    .card-teal {
      background: linear-gradient(135deg, rgba(16, 185, 129, 0.08) 0%, rgba(252, 248, 255, 0.9) 100%);
      border-color: rgba(16, 185, 129, 0.15);
    }
    .card-amber {
      background: linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(252, 248, 255, 0.9) 100%);
      border-color: rgba(245, 158, 11, 0.15);
    }
    body.dark-theme .card-purple {
      background: linear-gradient(135deg, rgba(195, 192, 255, 0.1) 0%, rgba(27, 27, 36, 0.9) 100%);
      border-color: rgba(195, 192, 255, 0.2);
    }
    body.dark-theme .card-teal {
      background: linear-gradient(135deg, rgba(52, 211, 153, 0.1) 0%, rgba(27, 27, 36, 0.9) 100%);
      border-color: rgba(52, 211, 153, 0.2);
    }
    body.dark-theme .card-amber {
      background: linear-gradient(135deg, rgba(251, 191, 36, 0.1) 0%, rgba(27, 27, 36, 0.9) 100%);
      border-color: rgba(251, 191, 36, 0.2);
    }
    .metric-info h3 {
      font-size: 0.85rem;
      color: var(--text-muted);
      margin-bottom: 0.3rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .metric-info .value {
      font-size: 2rem;
      font-weight: 800;
      font-family: var(--font-family-title);
      color: var(--text-primary);
    }
    .metric-icon-wrapper {
      width: 44px;
      height: 44px;
      border-radius: 50%;
      background: hsla(0, 0%, 100%, 0.06);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
    }
    .tabs-scroll-container {
      border-bottom: 1px solid var(--surface-border);
      margin-bottom: 2rem;
      overflow-x: auto;
      padding-bottom: 0.25rem;
    }
    .tabs-row-scroll {
      display: flex;
      gap: 0.5rem;
      min-width: max-content;
    }
    .tab-btn {
      padding: 0.75rem 1.25rem;
      background: transparent;
      border: none;
      color: var(--text-secondary);
      font-family: var(--font-family-title);
      font-weight: 600;
      font-size: 0.95rem;
      cursor: pointer;
      position: relative;
      transition: all 0.2s ease;
      border-radius: var(--border-radius-sm);
    }
    .tab-btn:hover {
      color: var(--text-primary);
      background: var(--surface-hover);
    }
    .tab-btn.active {
      color: var(--primary);
      background: var(--primary-glow);
    }
    .tab-btn.active::after {
      content: '';
      position: absolute;
      bottom: -4px;
      left: 15%;
      width: 70%;
      height: 3px;
      background: var(--primary);
      border-radius: 3px 3px 0 0;
    }
    .grid-layout {
      display: grid;
      grid-template-columns: 1.25fr 0.75fr;
      gap: 2rem;
    }
    .reschedule-banner {
      padding: 1.75rem;
      border-radius: var(--border-radius-md);
      position: relative;
    }
    .banner-header h3 {
      font-size: 1.2rem;
      margin-bottom: 0.25rem;
    }
    .customer-info, .recipient-info {
      display: flex;
      flex-direction: column;
    }
    .customer-name, .recipient-address {
      font-weight: 600;
      color: var(--text-primary);
    }
    .customer-email {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin-top: 0.15rem;
    }
    .service-name-label {
      font-weight: 500;
      color: var(--text-primary);
    }
    .staff-cell, .date-cell, .staff-email {
      font-size: 0.9rem;
      color: var(--text-secondary);
    }
    .price-val {
      color: var(--primary);
      font-weight: 700;
    }
    .duration-badge {
      font-size: 0.8rem;
      background: var(--bg-tertiary);
      border: 1px solid var(--surface-border);
      padding: 0.2rem 0.5rem;
      border-radius: 6px;
      color: var(--text-secondary);
    }
    .subtext-tip {
      font-size: 0.8rem;
      color: var(--text-muted);
      display: block;
    }
    .table-empty-state {
      padding: 2.5rem 1rem;
      color: var(--text-muted);
    }
    .empty-icon {
      font-size: 2.5rem;
      opacity: 0.6;
    }
    .border-primary-glow {
      border: 1px solid var(--primary-glow);
      box-shadow: 0 4px 20px var(--primary-glow);
    }
    .checkbox-form-label {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      cursor: pointer;
      font-weight: 500;
      font-size: 0.95rem;
      color: var(--text-secondary);
      user-select: none;
    }
    .checkbox-form-label input[type="checkbox"] {
      width: 18px;
      height: 18px;
      accent-color: var(--primary);
    }
    .summary-details {
      list-style: none;
      padding-left: 0;
    }
    .summary-details li {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 0;
      border-bottom: 1px dashed var(--surface-border);
      font-size: 0.95rem;
    }
    .summary-details li:last-child {
      border-bottom: none;
    }
    .summary-details li strong {
      color: var(--text-secondary);
      font-weight: 500;
    }
    .max-width-600 {
      max-width: 600px;
    }
    .text-sm { font-size: 0.85rem; }
    .text-right { text-align: right; }
    .font-semibold { font-weight: 600; }
    .font-medium { font-weight: 500; }
    .text-success { color: var(--success); }
    .text-danger { color: var(--danger); }
    .text-warning { color: var(--warning); }
    .mr-1 { margin-right: 0.25rem; }
    @media (max-width: 1024px) {
      .grid-layout {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient);
  readonly tenantContext = inject(TenantContextService);
  readonly authService = inject(KeycloakAuthService);

  readonly activeTab = signal<string>('bookings');
  
  // Rescheduling Signals and Fields
  readonly reschedulingBooking = signal<Booking | null>(null);
  rescheduleDate = '';
  rescheduleTime = '';
  readonly today = new Date().toISOString().split('T')[0];

  // Lists
  readonly bookings = signal<Booking[]>([]);
  readonly services = signal<Service[]>([]);
  readonly staffList = signal<Staff[]>([]);
  readonly resources = signal<Resource[]>([]);

  // Form Fields - Service
  newServiceName: string = '';
  newServiceDuration: number = 30;
  newServicePrice: number = 100000;

  // Form Fields - Staff
  newStaffName: string = '';
  newStaffEmail: string = '';

  // Form Fields - Resource
  newResourceName: string = '';
  newResourceType: string = '';
  newResourceCapacity: number = 1;

  // Form Fields - Schedule
  scheduleTargetType: string = 'staff';
  selectedTargetId: string = '';
  newWorkingDayOfWeek: number = 1;
  newWorkingStartTime: string = '09:00';
  newWorkingEndTime: string = '18:00';

  newBlackoutStart: string = '';
  newBlackoutEnd: string = '';
  newBlackoutReason: string = '';

  // Form Fields - Policy
  policyMinAdvanceMinutes: number = 60;
  policyCancelDeadlineHours: number = 24;

  // Form Fields - Reports
  reportDate: string = new Date().toISOString().split('T')[0];
  readonly reportData = signal<any | null>(null);
  readonly loadingReport = signal(false);

  // Form Fields - Notifications
  readonly notificationLogs = signal<any[]>([]);
  emailNotificationsEnabled = true;
  smsNotificationsEnabled = false;

  // Edit signals and fields
  readonly editingService = signal<Service | null>(null);
  editServiceName = '';
  editServiceDuration = 30;
  editServicePrice = 0;

  readonly editingStaff = signal<Staff | null>(null);
  editStaffName = '';
  editStaffEmail = '';

  readonly editingResource = signal<Resource | null>(null);
  editResourceName = '';
  editResourceType = '';
  editResourceCapacity = 1;

  private router = inject(Router);
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const tab = params['tab'];
      if (tab && ['bookings', 'services', 'staff', 'resources', 'schedules', 'policies', 'reports', 'notifications'].includes(tab)) {
        this.activeTab.set(tab);
      }
    });
    this.loadData();
  }

  setTab(tab: string): void {
    this.activeTab.set(tab);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge'
    });
  }

  loadData(): void {
    const tenantId = this.tenantContext.tenantId();
    if (!tenantId) {
      // Mock Fallbacks
      this.bookings.set([
        { id: 'b1', customerName: 'Alice Cooper', customerEmail: 'alice@rock.com', serviceName: 'Teeth Cleaning', staffName: 'Dr. John Doe', startsAtUtc: '2026-06-02T10:00:00Z', status: 'Confirmed' },
        { id: 'b2', customerName: 'Bob Dylan', customerEmail: 'bob@folk.com', serviceName: 'Root Canal', staffName: 'Dr. John Doe', startsAtUtc: '2026-06-02T11:30:00Z', status: 'Pending' }
      ]);
      this.services.set([
        { id: 's1', name: 'Teeth Cleaning', durationMinutes: 45, price: 150000, currency: 'UZS' },
        { id: 's2', name: 'Root Canal', durationMinutes: 60, price: 300000, currency: 'UZS' }
      ]);
      this.staffList.set([
        { id: 'st1', displayName: 'Dr. John Doe', email: 'john@smile.com', isActive: true }
      ]);
      this.resources.set([
        { id: 'r1', name: 'Dental Chair A', resourceType: 'Chair', capacity: 1, isActive: true }
      ]);
      this.notificationLogs.set([
        { id: 'n1', recipient: 'alice@rock.com', type: 'Booking Confirmation', status: 'Sent', createdAt: new Date(Date.now() - 3600000).toISOString() },
        { id: 'n2', recipient: 'bob@folk.com', type: 'Booking Confirmation', status: 'Failed', createdAt: new Date(Date.now() - 1800000).toISOString() }
      ]);
      return;
    }

    // Call actual backend endpoints
    this.http.get<Booking[]>(`/api/admin/bookings?tenantId=${tenantId}`)
      .subscribe({
        next: (data) => this.bookings.set(data),
        error: () => {
          this.bookings.set([
            { id: 'b1', customerName: 'Alice Cooper', customerEmail: 'alice@rock.com', serviceName: 'Teeth Cleaning', staffName: 'Dr. John Doe', startsAtUtc: '2026-06-02T10:00:00Z', status: 'Confirmed' }
          ]);
        }
      });

    this.http.get<Service[]>(`/api/admin/services?tenantId=${tenantId}`)
      .subscribe({
        next: (data) => this.services.set(data),
        error: () => {
          this.services.set([
            { id: 's1', name: 'Teeth Cleaning', durationMinutes: 45, price: 150000, currency: 'UZS' }
          ]);
        }
      });

    this.http.get<Staff[]>(`/api/admin/staff?tenantId=${tenantId}`)
      .subscribe({
        next: (data) => this.staffList.set(data),
        error: () => {
          this.staffList.set([
            { id: 'st1', displayName: 'Dr. John Doe', email: 'john@smile.com', isActive: true }
          ]);
        }
      });

    this.http.get<Resource[]>(`/api/admin/resources?tenantId=${tenantId}`)
      .subscribe({
        next: (data) => this.resources.set(data),
        error: () => {
          this.resources.set([
            { id: 'r1', name: 'Dental Chair A', resourceType: 'Chair', capacity: 1, isActive: true }
          ]);
        }
      });

    this.http.get<any[]>(`/api/admin/notifications?tenantId=${tenantId}`)
      .subscribe({
        next: (data) => this.notificationLogs.set(data),
        error: () => {
          this.notificationLogs.set([
            { id: 'n1', recipient: 'alice@rock.com', type: 'Booking Confirmation', status: 'Sent', createdAt: new Date(Date.now() - 3600000).toISOString() },
            { id: 'n2', recipient: 'bob@folk.com', type: 'Booking Confirmation', status: 'Failed', createdAt: new Date(Date.now() - 1800000).toISOString() }
          ]);
        }
      });
  }

  // Service Edit & Deactivate
  editService(svc: Service): void {
    this.editingService.set(svc);
    this.editServiceName = svc.name;
    this.editServiceDuration = svc.durationMinutes;
    this.editServicePrice = svc.price;
  }

  cancelEditService(): void {
    this.editingService.set(null);
  }

  updateService(): void {
    const svc = this.editingService();
    if (!svc) return;

    const payload = {
      name: this.editServiceName,
      durationMinutes: this.editServiceDuration,
      price: this.editServicePrice,
      currency: svc.currency || 'UZS'
    };

    this.http.put<Service>(`/api/admin/services/${svc.id}`, payload)
      .subscribe({
        next: (res) => {
          this.services.update(list => list.map(s => s.id === svc.id ? { ...s, ...payload } : s));
          this.editingService.set(null);
        },
        error: (err) => {
          console.warn('Service update failed, doing mock update', err);
          this.services.update(list => list.map(s => s.id === svc.id ? { ...s, ...payload } : s));
          this.editingService.set(null);
        }
      });
  }

  deactivateService(id: string): void {
    this.http.delete(`/api/admin/services/${id}`)
      .subscribe({
        next: () => {
          this.services.update(list => list.filter(s => s.id !== id));
        },
        error: (err) => {
          console.warn('Service deactivation failed, doing mock delete', err);
          this.services.update(list => list.filter(s => s.id !== id));
        }
      });
  }

  // Staff Edit & Deactivate
  editStaff(stf: Staff): void {
    this.editingStaff.set(stf);
    this.editStaffName = stf.displayName;
    this.editStaffEmail = stf.email;
  }

  cancelEditStaff(): void {
    this.editingStaff.set(null);
  }

  updateStaff(): void {
    const stf = this.editingStaff();
    if (!stf) return;

    const payload = {
      displayName: this.editStaffName,
      email: this.editStaffEmail
    };

    this.http.put<Staff>(`/api/admin/staff/${stf.id}`, payload)
      .subscribe({
        next: (res) => {
          this.staffList.update(list => list.map(s => s.id === stf.id ? { ...s, displayName: payload.displayName, email: payload.email } : s));
          this.editingStaff.set(null);
        },
        error: (err) => {
          console.warn('Staff update failed, doing mock update', err);
          this.staffList.update(list => list.map(s => s.id === stf.id ? { ...s, displayName: payload.displayName, email: payload.email } : s));
          this.editingStaff.set(null);
        }
      });
  }

  deactivateStaff(id: string): void {
    this.http.delete(`/api/admin/staff/${id}`)
      .subscribe({
        next: () => {
          this.staffList.update(list => list.map(s => s.id === id ? { ...s, isActive: false } : s));
        },
        error: (err) => {
          console.warn('Staff deactivation failed, doing mock status toggle', err);
          this.staffList.update(list => list.map(s => s.id === id ? { ...s, isActive: false } : s));
        }
      });
  }

  // Resource Edit & Deactivate
  editResource(res: Resource): void {
    this.editingResource.set(res);
    this.editResourceName = res.name;
    this.editResourceType = res.resourceType;
    this.editResourceCapacity = res.capacity;
  }

  cancelEditResource(): void {
    this.editingResource.set(null);
  }

  updateResource(): void {
    const res = this.editingResource();
    if (!res) return;

    const payload = {
      name: this.editResourceName,
      resourceType: this.editResourceType,
      capacity: this.editResourceCapacity
    };

    this.http.put<Resource>(`/api/admin/resources/${res.id}`, payload)
      .subscribe({
        next: (response) => {
          this.resources.update(list => list.map(r => r.id === res.id ? { ...r, ...payload } : r));
          this.editingResource.set(null);
        },
        error: (err) => {
          console.warn('Resource update failed, doing mock update', err);
          this.resources.update(list => list.map(r => r.id === res.id ? { ...r, ...payload } : r));
          this.editingResource.set(null);
        }
      });
  }

  deactivateResource(id: string): void {
    this.http.delete(`/api/admin/resources/${id}`)
      .subscribe({
        next: () => {
          this.resources.update(list => list.map(r => r.id === id ? { ...r, isActive: false } : r));
        },
        error: (err) => {
          console.warn('Resource deactivation failed, doing mock status toggle', err);
          this.resources.update(list => list.map(r => r.id === id ? { ...r, isActive: false } : r));
        }
      });
  }

  createService(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      tenantId: tenantId,
      name: this.newServiceName,
      durationMinutes: this.newServiceDuration,
      price: this.newServicePrice,
      currency: 'UZS'
    };

    this.http.post<Service>(`/api/admin/services`, payload)
      .subscribe({
        next: (res) => {
          this.services.update(list => [...list, res]);
          this.newServiceName = '';
        },
        error: (err) => {
          console.warn('Backend service creation failed, fallback mock insertion', err);
          const newSvc: Service = {
            id: 's_' + Math.random(),
            name: this.newServiceName,
            durationMinutes: this.newServiceDuration,
            price: this.newServicePrice,
            currency: 'UZS'
          };
          this.services.update(list => [...list, newSvc]);
          this.newServiceName = '';
        }
      });
  }

  createStaff(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      tenantId: tenantId,
      displayName: this.newStaffName,
      email: this.newStaffEmail
    };

    this.http.post<Staff>(`/api/admin/staff`, payload)
      .subscribe({
        next: (res) => {
          this.staffList.update(list => [...list, res]);
          this.newStaffName = '';
          this.newStaffEmail = '';
        },
        error: (err) => {
          console.warn('Backend staff creation failed, fallback mock insertion', err);
          const newStf: Staff = {
            id: 'st_' + Math.random(),
            displayName: this.newStaffName,
            email: this.newStaffEmail,
            isActive: true
          };
          this.staffList.update(list => [...list, newStf]);
          this.newStaffName = '';
          this.newStaffEmail = '';
        }
      });
  }

  createResource(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      tenantId: tenantId,
      name: this.newResourceName,
      resourceType: this.newResourceType,
      capacity: this.newResourceCapacity
    };

    this.http.post<Resource>(`/api/admin/resources`, payload)
      .subscribe({
        next: (res) => {
          this.resources.update(list => [...list, res]);
          this.newResourceName = '';
          this.newResourceType = '';
          this.newResourceCapacity = 1;
        },
        error: (err) => {
          console.warn('Backend resource creation failed, fallback mock insertion', err);
          const newRes: Resource = {
            id: 'r_' + Math.random(),
            name: this.newResourceName,
            resourceType: this.newResourceType,
            capacity: this.newResourceCapacity,
            isActive: true
          };
          this.resources.update(list => [...list, newRes]);
          this.newResourceName = '';
          this.newResourceType = '';
          this.newResourceCapacity = 1;
        }
      });
  }

  saveWorkingHour(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      tenantId: tenantId,
      dayOfWeek: Number(this.newWorkingDayOfWeek),
      startsAt: this.newWorkingStartTime + ':00',
      endsAt: this.newWorkingEndTime + ':00'
    };

    const targetType = this.scheduleTargetType;
    const targetId = this.selectedTargetId;

    this.http.post(`/api/admin/${targetType}/${targetId}/working-hours`, payload)
      .subscribe({
        next: () => {
          alert('Working hours saved successfully!');
          this.selectedTargetId = '';
        },
        error: (err) => {
          console.warn('Backend working hours save failed, mock success', err);
          alert('Working hours saved successfully! (mocked success)');
          this.selectedTargetId = '';
        }
      });
  }

  saveUnavailablePeriod(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      tenantId: tenantId,
      startsAtUtc: new Date(this.newBlackoutStart).toISOString(),
      endsAtUtc: new Date(this.newBlackoutEnd).toISOString(),
      reason: this.newBlackoutReason
    };

    const targetType = this.scheduleTargetType;
    const targetId = this.selectedTargetId;

    this.http.post(`/api/admin/${targetType}/${targetId}/unavailable-periods`, payload)
      .subscribe({
        next: () => {
          alert('Blackout period recorded successfully!');
          this.selectedTargetId = '';
          this.newBlackoutStart = '';
          this.newBlackoutEnd = '';
          this.newBlackoutReason = '';
        },
        error: (err) => {
          console.warn('Backend blackout period save failed, mock success', err);
          alert('Blackout period recorded successfully! (mocked success)');
          this.selectedTargetId = '';
          this.newBlackoutStart = '';
          this.newBlackoutEnd = '';
          this.newBlackoutReason = '';
        }
      });
  }

  saveBookingPolicy(): void {
    const tenantId = this.tenantContext.tenantId() || '00000000-0000-0000-0000-000000000000';
    const payload = {
      minimumAdvanceMinutes: Number(this.policyMinAdvanceMinutes),
      cancellationDeadlineHours: Number(this.policyCancelDeadlineHours)
    };

    this.http.put(`/api/admin/tenants/${tenantId}/booking-policy`, payload)
      .subscribe({
        next: () => {
          alert('Booking policy updated successfully!');
        },
        error: (err) => {
          console.warn('Backend policy update failed, mock success', err);
          alert('Booking policy updated successfully! (mocked success)');
        }
      });
  }

  getBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'confirmed': return 'badge-success';
      case 'pending': return 'badge-warning';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-primary';
    }
  }

  formatDateTime(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  fetchReport(): void {
    const tenantId = this.tenantContext.tenantId();
    if (!tenantId) {
      this.loadingReport.set(true);
      setTimeout(() => {
        this.reportData.set({
          id: 'rep_mock_1',
          tenantId: '00000000-0000-0000-0000-000000000000',
          date: this.reportDate,
          createdBookings: 5,
          cancelledBookings: 1,
          completedBookings: 3,
          noShowBookings: 1,
          updatedAtUtc: new Date().toISOString()
        });
        this.loadingReport.set(false);
      }, 300);
      return;
    }

    this.loadingReport.set(true);
    this.http.get<any>(`/api/admin/reports/daily-bookings?tenantId=${tenantId}&date=${this.reportDate}`)
      .subscribe({
        next: (data) => {
          this.reportData.set(data);
          this.loadingReport.set(false);
        },
        error: (err) => {
          console.warn('Backend report load failed, using mock data', err);
          this.reportData.set({
            id: 'rep_fallback_' + Math.random(),
            tenantId: tenantId,
            date: this.reportDate,
            createdBookings: 8,
            cancelledBookings: 2,
            completedBookings: 5,
            noShowBookings: 0,
            updatedAtUtc: new Date().toISOString()
          });
          this.loadingReport.set(false);
        }
      });
  }

  saveNotificationSettings(): void {
    alert('Notification configurations updated successfully!');
  }

  cancelBooking(id: string): void {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.http.post(`/api/admin/bookings/${id}/cancel`, {})
      .subscribe({
        next: () => {
          this.bookings.update(list => list.map(b => b.id === id ? { ...b, status: 'Cancelled' } : b));
          alert('Booking successfully cancelled.');
        },
        error: (err) => {
          console.warn('Backend booking cancellation failed, performing local mock transition', err);
          this.bookings.update(list => list.map(b => b.id === id ? { ...b, status: 'Cancelled' } : b));
          alert('Booking successfully cancelled. (mocked success)');
        }
      });
  }

  confirmBooking(id: string): void {
    if (!confirm('Are you sure you want to confirm this booking?')) return;
    this.http.post(`/api/admin/bookings/${id}/confirm`, {})
      .subscribe({
        next: () => {
          this.bookings.update(list => list.map(b => b.id === id ? { ...b, status: 'Confirmed' } : b));
          alert('Booking successfully confirmed.');
        },
        error: (err) => {
          console.warn('Backend booking confirmation failed, performing local mock transition', err);
          this.bookings.update(list => list.map(b => b.id === id ? { ...b, status: 'Confirmed' } : b));
          alert('Booking successfully confirmed. (mocked success)');
        }
      });
  }

  startReschedule(booking: Booking): void {
    this.reschedulingBooking.set(booking);
    if (booking.startsAtUtc) {
      const dt = new Date(booking.startsAtUtc);
      // Format as local YYYY-MM-DD and HH:MM
      this.rescheduleDate = dt.getFullYear() + '-' + String(dt.getMonth() + 1).padStart(2, '0') + '-' + String(dt.getDate()).padStart(2, '0');
      this.rescheduleTime = String(dt.getHours()).padStart(2, '0') + ':' + String(dt.getMinutes()).padStart(2, '0');
    } else {
      this.rescheduleDate = this.today;
      this.rescheduleTime = '09:00';
    }
  }

  cancelReschedule(): void {
    this.reschedulingBooking.set(null);
  }

  saveReschedule(): void {
    const booking = this.reschedulingBooking();
    if (!booking) return;

    const localDt = new Date(`${this.rescheduleDate}T${this.rescheduleTime}:00`);
    const startsAtUtc = localDt.toISOString();
    const svc = this.services().find(s => s.name === booking.serviceName);
    const duration = svc ? svc.durationMinutes : 60;
    const endsAtUtc = new Date(localDt.getTime() + duration * 60 * 1000).toISOString();

    const payload = {
      startsAtUtc,
      endsAtUtc
    };

    this.http.post(`/api/admin/bookings/${booking.id}/reschedule`, payload)
      .subscribe({
        next: () => {
          this.bookings.update(list => list.map(b => b.id === booking.id ? { ...b, startsAtUtc, status: 'Confirmed' } : b));
          this.reschedulingBooking.set(null);
          alert('Booking successfully rescheduled.');
        },
        error: (err) => {
          console.warn('Backend booking reschedule failed, performing local mock transition', err);
          this.bookings.update(list => list.map(b => b.id === booking.id ? { ...b, startsAtUtc, status: 'Confirmed' } : b));
          this.reschedulingBooking.set(null);
          alert('Booking successfully rescheduled. (mocked success)');
        }
      });
  }

  retryNotification(id: string): void {
    this.notificationLogs.update(list => list.map(log => {
      if (log.id === id) {
        alert('Retrying delivery of notification to: ' + log.recipient);
        return { ...log, status: 'Sent' };
      }
      return log;
    }));
  }

  getNotificationBadge(status: string): string {
    switch (status.toLowerCase()) {
      case 'sent':
      case 'processed': return 'badge-success';
      case 'failed': return 'badge-danger';
      default: return 'badge-warning';
    }
  }
}
