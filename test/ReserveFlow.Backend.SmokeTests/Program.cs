using System.Security.Claims;
using ReserveFlow.Common.Domain;
using ReserveFlow.Common.Infrastructure;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Application.AuditLogs.RecordAuditLog;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;
using ReserveFlow.Modules.Audit.Infrastructure.Database;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;
using ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;
using ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Infrastructure.Database;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Domain.Notifications;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Infrastructure.Sending;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;
using ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;
using ReserveFlow.Modules.Reporting.Domain.Reports;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Domain.Resources;
using ReserveFlow.Modules.Resources.Infrastructure.Database;
using ReserveFlow.Modules.Scheduling.Application.Availability;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.Availability;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.Tenants;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;

var checks = new List<(string Name, bool Passed)>
{
    ("identity presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Identity.Presentation.AssemblyReference.Assembly)),
    ("tenants presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly)),
    ("catalog presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly)),
    ("staffing presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly)),
    ("resources presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly)),
    ("scheduling presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly)),
    ("bookings presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly)),
    ("notifications presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Notifications.Presentation.AssemblyReference.Assembly)),
    ("audit presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Audit.Presentation.AssemblyReference.Assembly)),
    ("reporting presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Reporting.Presentation.AssemblyReference.Assembly)),
    ("integrations presentation has endpoint", HasEndpoint(ReserveFlow.Modules.Integrations.Presentation.AssemblyReference.Assembly)),
    ("common application defines rate limit policies", HasTypeNamed(
        typeof(ReserveFlow.Common.Application.Messaging.ICommand).Assembly,
        "RateLimitPolicies")),
    ("common infrastructure configures rate limiter", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "AddRateLimiter")),
    ("api pipeline uses rate limiter", SourceContains(
        "src/API/ReserveFlow.Api/Program.cs",
        "UseRateLimiter")),
    ("public booking create endpoint is rate limited", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.PublicBooking")),
    ("public availability endpoint is rate limited", SourceContains(
        "src/Modules/Scheduling/ReserveFlow.Modules.Scheduling.Presentation/GetAvailableSlotsEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.PublicAvailability")),
    ("auth me endpoint is rate limited", SourceContains(
        "src/Modules/Identity/ReserveFlow.Modules.Identity.Presentation/CurrentUserEndpoint.cs",
        "RequireRateLimiting(RateLimitPolicies.AuthContext")),
    ("authorization policies require keycloak role assertions", SourceContains(
        "src/Common/ReserveFlow.Common.Infrastructure/InfrastructureConfiguration.cs",
        "KeycloakRoleClaims.HasAnyRole")),
    ("keycloak role parser accepts realm access roles", KeycloakRoleParserAcceptsRealmAccessRoles()),
    ("keycloak role parser accepts resource access roles", KeycloakRoleParserAcceptsResourceAccessRoles()),
    ("keycloak role parser accepts simple roles claims", KeycloakRoleParserAcceptsSimpleRoleClaims()),
    ("identity module has db context", typeof(IdentityDbContext).Name == nameof(IdentityDbContext)),
    ("tenants module has db context", typeof(TenantsDbContext).Name == nameof(TenantsDbContext)),
    ("tenants db context exposes tenant categories", typeof(TenantsDbContext).GetProperty("TenantCategories") is not null),
    ("catalog module has db context", typeof(CatalogDbContext).Name == nameof(CatalogDbContext)),
    ("staffing module has db context", typeof(StaffingDbContext).Name == nameof(StaffingDbContext)),
    ("resources module has db context", typeof(ResourcesDbContext).Name == nameof(ResourcesDbContext)),
    ("scheduling module has db context", typeof(SchedulingDbContext).Name == nameof(SchedulingDbContext)),
    ("scheduling db context exposes unavailable periods", typeof(SchedulingDbContext).GetProperty("UnavailablePeriods") is not null),
    ("bookings module has db context", typeof(BookingsDbContext).Name == nameof(BookingsDbContext)),
    ("notifications module has db context", typeof(NotificationsDbContext).Name == nameof(NotificationsDbContext)),
    ("audit module has db context", typeof(AuditDbContext).Name == nameof(AuditDbContext)),
    ("reporting module has db context", typeof(ReportingDbContext).Name == nameof(ReportingDbContext)),
    ("integrations module has db context", typeof(IntegrationsDbContext).Name == nameof(IntegrationsDbContext)),
    ("bookings db context exposes customers", typeof(BookingsDbContext).GetProperty("Customers") is not null),
    ("bookings db context exposes booking history", typeof(BookingsDbContext).GetProperty("BookingHistoryEntries") is not null),
    ("bookings db context exposes booking policies", typeof(BookingsDbContext).GetProperty("BookingPolicies") is not null),
    ("bookings db context exposes outbox", typeof(BookingsDbContext).GetProperty("OutboxMessages") is not null),
    ("common outbox message captures domain event", OutboxMessageCapturesDomainEvent()),
    ("common outbox has dispatcher", typeof(IOutboxMessageDispatcher).Name == nameof(IOutboxMessageDispatcher)),
    ("common outbox has logging dispatcher", typeof(LoggingOutboxMessageDispatcher).Name == nameof(LoggingOutboxMessageDispatcher)),
    ("bookings infrastructure has outbox processor", HasTypeNamed(typeof(BookingsDbContext).Assembly, "BookingsOutboxProcessorHostedService")),
    ("outbox message records processing outcome", OutboxMessageRecordsProcessingOutcome()),
    ("bookings module has create command", typeof(CreateBookingCommand).Name == nameof(CreateBookingCommand)),
    ("booking create command captures customer contact", HasPublicProperty(typeof(CreateBookingCommand), "CustomerEmail")),
    ("booking create command captures idempotency key", HasPublicProperty(typeof(CreateBookingCommand), "IdempotencyKey")),
    ("bookings module has create handler", typeof(CreateBookingCommandHandler).Name == nameof(CreateBookingCommandHandler)),
    ("booking repository can lookup idempotency key", typeof(IBookingRepository).GetMethod("FindByIdempotencyKeyAsync") is not null),
    ("bookings module has policy repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingPolicyRepository")),
    ("booking create handler checks policy", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingPolicyRepository")),
    ("bookings module has availability checker contract", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingAvailabilityChecker")),
    ("booking create handler checks configured availability", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingAvailabilityChecker")),
    ("booking cancel handler checks policy", HasConstructorParameterNamed(typeof(CancelBookingCommandHandler), "IBookingPolicyRepository")),
    ("booking cancel command captures policy enforcement", HasPublicProperty(typeof(CancelBookingCommand), "EnforcePolicy")),
    ("bookings module has configure policy command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfigureBookingPolicyCommand")),
    ("bookings module has configure policy handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfigureBookingPolicyCommandHandler")),
    ("bookings module has customer repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ICustomerRepository")),
    ("bookings module has history repository", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "IBookingHistoryRepository")),
    ("booking create handler records history", HasConstructorParameterNamed(typeof(CreateBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking cancel handler records history", HasConstructorParameterNamed(typeof(CancelBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking reschedule handler records history", HasConstructorParameterNamed(typeof(RescheduleBookingCommandHandler), "IBookingHistoryRepository")),
    ("booking reschedule handler checks configured availability", HasConstructorParameterNamed(typeof(RescheduleBookingCommandHandler), "IBookingAvailabilityChecker")),
    ("bookings infrastructure has scheduling-backed availability checker", HasTypeNamed(typeof(BookingsDbContext).Assembly, "SchedulingBookingAvailabilityChecker")),
    ("bookings module has cancel command", typeof(CancelBookingCommand).Name == nameof(CancelBookingCommand)),
    ("bookings module has cancel handler", typeof(CancelBookingCommandHandler).Name == nameof(CancelBookingCommandHandler)),
    ("bookings module has reschedule command", typeof(RescheduleBookingCommand).Name == nameof(RescheduleBookingCommand)),
    ("bookings module has reschedule handler", typeof(RescheduleBookingCommandHandler).Name == nameof(RescheduleBookingCommandHandler)),
    ("bookings module has confirm command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfirmBookingCommand")),
    ("bookings module has confirm handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ConfirmBookingCommandHandler")),
    ("booking confirm handler records history", HasConstructorParameterNamed(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking.ConfirmBookingCommandHandler),
        "IBookingHistoryRepository")),
    ("bookings module has expire command", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ExpirePendingBookingCommand")),
    ("bookings module has expire handler", HasTypeNamed(typeof(CreateBookingCommand).Assembly, "ExpirePendingBookingCommandHandler")),
    ("booking expire handler records history", HasConstructorParameterNamed(
        typeof(ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking.ExpirePendingBookingCommandHandler),
        "IBookingHistoryRepository")),
    ("bookings module has complete command", typeof(CompleteBookingCommand).Name == nameof(CompleteBookingCommand)),
    ("bookings module has complete handler", typeof(CompleteBookingCommandHandler).Name == nameof(CompleteBookingCommandHandler)),
    ("booking complete handler records history", HasConstructorParameterNamed(typeof(CompleteBookingCommandHandler), "IBookingHistoryRepository")),
    ("bookings module has no-show command", typeof(MarkBookingAsNoShowCommand).Name == nameof(MarkBookingAsNoShowCommand)),
    ("bookings module has no-show handler", typeof(MarkBookingAsNoShowCommandHandler).Name == nameof(MarkBookingAsNoShowCommandHandler)),
    ("booking no-show handler records history", HasConstructorParameterNamed(typeof(MarkBookingAsNoShowCommandHandler), "IBookingHistoryRepository")),
    ("bookings module has response dto", typeof(BookingResponse).Name == nameof(BookingResponse)),
    ("booking response exposes concurrency token", HasPublicProperty(typeof(BookingResponse), "ConcurrencyToken")),
    ("bookings presentation has create endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CreateBookingEndpoint")),
    ("public booking request captures customer contact", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "CreateBookingRequest",
        "CustomerEmail")),
    ("public booking request captures idempotency key", HasTypeWithPublicProperty(
        ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly,
        "CreateBookingRequest",
        "IdempotencyKey")),
    ("bookings presentation has configure policy endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ConfigureBookingPolicyEndpoint")),
    ("bookings presentation has cancel endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CancelBookingEndpoint")),
    ("bookings presentation has public cancel endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "PublicCancelBookingEndpoint")),
    ("bookings presentation has reschedule endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "RescheduleBookingEndpoint")),
    ("bookings presentation has confirm endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ConfirmBookingEndpoint")),
    ("bookings presentation has expire endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "ExpirePendingBookingEndpoint")),
    ("bookings presentation has complete endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "CompleteBookingEndpoint")),
    ("bookings presentation has no-show endpoint", HasEndpointNamed(ReserveFlow.Modules.Bookings.Presentation.AssemblyReference.Assembly, "MarkBookingAsNoShowEndpoint")),
    ("notifications module has queue command", typeof(QueueNotificationCommand).Name == nameof(QueueNotificationCommand)),
    ("notifications module has queue handler", typeof(QueueNotificationCommandHandler).Name == nameof(QueueNotificationCommandHandler)),
    ("notifications module has response dto", typeof(NotificationResponse).Name == nameof(NotificationResponse)),
    ("notifications module has fake sender", typeof(FakeNotificationSender).Name == nameof(FakeNotificationSender)),
    ("notification queue normalizes data and raises domain event", NotificationQueueNormalizesDataAndRaisesDomainEvent()),
    ("audit module has record command", typeof(RecordAuditLogCommand).Name == nameof(RecordAuditLogCommand)),
    ("audit module has record handler", typeof(RecordAuditLogCommandHandler).Name == nameof(RecordAuditLogCommandHandler)),
    ("audit module has response dto", typeof(AuditLogResponse).Name == nameof(AuditLogResponse)),
    ("audit log normalizes data and raises domain event", AuditLogNormalizesDataAndRaisesDomainEvent()),
    ("reporting module has record command", typeof(RecordDailyBookingReportCommand).Name == nameof(RecordDailyBookingReportCommand)),
    ("reporting module has record handler", typeof(RecordDailyBookingReportCommandHandler).Name == nameof(RecordDailyBookingReportCommandHandler)),
    ("reporting module has daily query", typeof(GetDailyBookingReportQuery).Name == nameof(GetDailyBookingReportQuery)),
    ("reporting module has daily query handler", typeof(GetDailyBookingReportQueryHandler).Name == nameof(GetDailyBookingReportQueryHandler)),
    ("reporting module has response dto", typeof(DailyBookingReportResponse).Name == nameof(DailyBookingReportResponse)),
    ("daily booking report validates counts and raises domain event", DailyBookingReportValidatesCountsAndRaisesDomainEvent()),
    ("integrations module has accept webhook command", typeof(AcceptWebhookCommand).Name == nameof(AcceptWebhookCommand)),
    ("integrations module has accept webhook handler", typeof(AcceptWebhookCommandHandler).Name == nameof(AcceptWebhookCommandHandler)),
    ("integrations module has webhook inbox response dto", typeof(WebhookInboxMessageResponse).Name == nameof(WebhookInboxMessageResponse)),
    ("webhook inbox message normalizes data and raises domain event", WebhookInboxMessageNormalizesDataAndRaisesDomainEvent()),
    ("tenant create normalizes data and raises domain event", TenantCreateNormalizesDataAndRaisesDomainEvent()),
    ("tenant create can assign category", TenantCreateCanAssignCategory()),
    ("tenant response exposes category id", HasPublicProperty(typeof(TenantResponse), "CategoryId")),
    ("tenant category normalizes data and raises domain event", TenantCategoryCreateNormalizesDataAndRaisesDomainEvent()),
    ("tenants module has category repository", HasTypeNamed(typeof(TenantResponse).Assembly, "ITenantCategoryRepository")),
    ("tenants module has create category command", HasTypeNamed(typeof(TenantResponse).Assembly, "CreateTenantCategoryCommand")),
    ("tenants module has create category handler", HasTypeNamed(typeof(TenantResponse).Assembly, "CreateTenantCategoryCommandHandler")),
    ("tenants module has public category query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPublicTenantCategoriesQuery")),
    ("tenants module has public tenant list query", HasTypeNamed(typeof(TenantResponse).Assembly, "GetPublicTenantsQuery")),
    ("tenants module has activate tenant command", HasTypeNamed(typeof(TenantResponse).Assembly, "ActivateTenantCommand")),
    ("tenants module has activate tenant handler", HasTypeNamed(typeof(TenantResponse).Assembly, "ActivateTenantCommandHandler")),
    ("tenants module has suspend tenant command", HasTypeNamed(typeof(TenantResponse).Assembly, "SuspendTenantCommand")),
    ("tenants module has suspend tenant handler", HasTypeNamed(typeof(TenantResponse).Assembly, "SuspendTenantCommandHandler")),
    ("tenants presentation has public categories endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPublicCategoriesEndpoint")),
    ("tenants presentation has public tenant list endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "GetPublicTenantsEndpoint")),
    ("tenants presentation has create category endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "CreateTenantCategoryEndpoint")),
    ("tenants presentation has activate tenant endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "ActivateTenantEndpoint")),
    ("tenants presentation has suspend tenant endpoint", HasEndpointNamed(ReserveFlow.Modules.Tenants.Presentation.AssemblyReference.Assembly, "SuspendTenantEndpoint")),
    ("tenant activate changes status and raises domain event", TenantActivateChangesStatusAndRaisesDomainEvent()),
    ("tenant suspend changes status and raises domain event", TenantSuspendChangesStatusAndRaisesDomainEvent()),
    ("service create normalizes data and raises domain event", ServiceCreateNormalizesDataAndRaisesDomainEvent()),
    ("catalog module has update service command", HasTypeNamed(typeof(ServiceResponse).Assembly, "UpdateServiceCommand")),
    ("catalog module has update service handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "UpdateServiceCommandHandler")),
    ("catalog module has deactivate service command", HasTypeNamed(typeof(ServiceResponse).Assembly, "DeactivateServiceCommand")),
    ("catalog module has deactivate service handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "DeactivateServiceCommandHandler")),
    ("catalog service repository can lookup service by id", typeof(IServiceRepository).GetMethod("GetByIdAsync") is not null),
    ("catalog service repository can list services by tenant", typeof(IServiceRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("catalog module has admin services query", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServicesQuery")),
    ("catalog module has admin services query handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServicesQueryHandler")),
    ("catalog module has admin service detail query", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServiceQuery")),
    ("catalog module has admin service detail query handler", HasTypeNamed(typeof(ServiceResponse).Assembly, "GetServiceQueryHandler")),
    ("catalog presentation has admin services endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "GetAdminServicesEndpoint")),
    ("catalog presentation has admin service detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "GetAdminServiceEndpoint")),
    ("catalog presentation has update service endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "UpdateServiceEndpoint")),
    ("catalog presentation has deactivate service endpoint", HasEndpointNamed(ReserveFlow.Modules.Catalog.Presentation.AssemblyReference.Assembly, "DeactivateServiceEndpoint")),
    ("service update changes details and raises domain event", ServiceUpdateChangesDetailsAndRaisesDomainEvent()),
    ("service deactivate changes active flag and raises domain event", ServiceDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("staff member create normalizes data and raises domain event", StaffMemberCreateNormalizesDataAndRaisesDomainEvent()),
    ("staffing module has update staff member command", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "UpdateStaffMemberCommand")),
    ("staffing module has update staff member handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "UpdateStaffMemberCommandHandler")),
    ("staffing module has deactivate staff member command", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "DeactivateStaffMemberCommand")),
    ("staffing module has deactivate staff member handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "DeactivateStaffMemberCommandHandler")),
    ("staff member repository can lookup staff member by id", typeof(IStaffMemberRepository).GetMethod("GetByIdAsync") is not null),
    ("staff member repository can list staff members by tenant", typeof(IStaffMemberRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("staffing module has admin staff members query", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMembersQuery")),
    ("staffing module has admin staff members query handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMembersQueryHandler")),
    ("staffing module has admin staff member detail query", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMemberQuery")),
    ("staffing module has admin staff member detail query handler", HasTypeNamed(typeof(StaffMemberResponse).Assembly, "GetStaffMemberQueryHandler")),
    ("staffing presentation has admin staff members endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "GetAdminStaffMembersEndpoint")),
    ("staffing presentation has admin staff member detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "GetAdminStaffMemberEndpoint")),
    ("staffing presentation has update staff member endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "UpdateStaffMemberEndpoint")),
    ("staffing presentation has deactivate staff member endpoint", HasEndpointNamed(ReserveFlow.Modules.Staffing.Presentation.AssemblyReference.Assembly, "DeactivateStaffMemberEndpoint")),
    ("staff member update changes details and raises domain event", StaffMemberUpdateChangesDetailsAndRaisesDomainEvent()),
    ("staff member deactivate changes active flag and raises domain event", StaffMemberDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("resource create normalizes data and raises domain event", ResourceCreateNormalizesDataAndRaisesDomainEvent()),
    ("resources module has update resource command", HasTypeNamed(typeof(ResourceResponse).Assembly, "UpdateResourceCommand")),
    ("resources module has update resource handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "UpdateResourceCommandHandler")),
    ("resources module has deactivate resource command", HasTypeNamed(typeof(ResourceResponse).Assembly, "DeactivateResourceCommand")),
    ("resources module has deactivate resource handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "DeactivateResourceCommandHandler")),
    ("resource repository can lookup resource by id", typeof(IResourceRepository).GetMethod("GetByIdAsync") is not null),
    ("resource repository can list resources by tenant", typeof(IResourceRepository).GetMethod("GetByTenantIdAsync") is not null),
    ("resources module has admin resources query", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourcesQuery")),
    ("resources module has admin resources query handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourcesQueryHandler")),
    ("resources module has admin resource detail query", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourceQuery")),
    ("resources module has admin resource detail query handler", HasTypeNamed(typeof(ResourceResponse).Assembly, "GetResourceQueryHandler")),
    ("resources presentation has admin resources endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "GetAdminResourcesEndpoint")),
    ("resources presentation has admin resource detail endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "GetAdminResourceEndpoint")),
    ("resources presentation has update resource endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "UpdateResourceEndpoint")),
    ("resources presentation has deactivate resource endpoint", HasEndpointNamed(ReserveFlow.Modules.Resources.Presentation.AssemblyReference.Assembly, "DeactivateResourceEndpoint")),
    ("resource update changes details and raises domain event", ResourceUpdateChangesDetailsAndRaisesDomainEvent()),
    ("resource deactivate changes active flag and raises domain event", ResourceDeactivateChangesActiveFlagAndRaisesDomainEvent()),
    ("scheduling module has working hour response", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "WorkingHourResponse")),
    ("scheduling module has create working hour command", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateWorkingHourCommand")),
    ("scheduling module has create working hour handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateWorkingHourCommandHandler")),
    ("working hour repository can query by target and day", typeof(IWorkingHourRepository).GetMethod("GetByTargetAndDayAsync") is not null),
    ("scheduling module has tenant availability query", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "GetTenantAvailableSlotsQuery")),
    ("scheduling module has tenant availability handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "GetTenantAvailableSlotsQueryHandler")),
    ("scheduling module has unavailable period response", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "UnavailablePeriodResponse")),
    ("scheduling module has unavailable period repository", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "IUnavailablePeriodRepository")),
    ("scheduling module has create unavailable period command", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateUnavailablePeriodCommand")),
    ("scheduling module has create unavailable period handler", HasTypeNamed(typeof(AvailableSlotResponse).Assembly, "CreateUnavailablePeriodCommandHandler")),
    ("tenant availability handler subtracts unavailable periods", HasConstructorParameterNamed(typeof(GetTenantAvailableSlotsQueryHandler), "IUnavailablePeriodRepository")),
    ("scheduling presentation has create staff working hour endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateStaffWorkingHourEndpoint")),
    ("scheduling presentation has create resource working hour endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateResourceWorkingHourEndpoint")),
    ("scheduling presentation has create staff unavailable period endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateStaffUnavailablePeriodEndpoint")),
    ("scheduling presentation has create resource unavailable period endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "CreateResourceUnavailablePeriodEndpoint")),
    ("scheduling presentation has tenant availability endpoint", HasEndpointNamed(ReserveFlow.Modules.Scheduling.Presentation.AssemblyReference.Assembly, "GetTenantAvailableSlotsEndpoint")),
    ("working hour create targets staff or resource and raises domain event", WorkingHourCreateTargetsStaffOrResourceAndRaisesDomainEvent()),
    ("unavailable period create targets staff or resource and raises domain event", UnavailablePeriodCreateTargetsStaffOrResourceAndRaisesDomainEvent()),
    ("booking availability checker excludes unavailable periods", SourceContains(
        "src/Modules/Bookings/ReserveFlow.Modules.Bookings.Infrastructure/Bookings/Availability/SchedulingBookingAvailabilityChecker.cs",
        "unavailable_periods")),
    ("availability engine generates fixed-duration slots", AvailabilityEngineGeneratesFixedDurationSlots()),
    ("booking create raises domain event", BookingCreateRaisesDomainEvent()),
    ("booking create stores idempotency key", BookingCreateStoresIdempotencyKey()),
    ("booking exposes optimistic concurrency token", BookingExposesOptimisticConcurrencyToken()),
    ("booking lifecycle mutation changes concurrency token", BookingLifecycleMutationChangesConcurrencyToken()),
    ("customer register normalizes data and raises domain event", CustomerRegisterNormalizesDataAndRaisesDomainEvent()),
    ("booking history records status and raises domain event", BookingHistoryRecordsStatusAndRaisesDomainEvent()),
    ("booking policy configures rules and raises domain event", BookingPolicyConfiguresRulesAndRaisesDomainEvent()),
    ("booking policy blocks booking inside minimum advance window", BookingPolicyBlocksBookingInsideMinimumAdvanceWindow()),
    ("booking policy blocks cancellation after deadline", BookingPolicyBlocksCancellationAfterDeadline()),
    ("booking cancel changes status and raises domain event", BookingCancelChangesStatusAndRaisesDomainEvent()),
    ("booking reschedule changes time and raises domain event", BookingRescheduleChangesTimeAndRaisesDomainEvent()),
    ("booking confirm changes status and raises domain event", BookingConfirmChangesStatusAndRaisesDomainEvent()),
    ("booking expire changes status and raises domain event", BookingExpireChangesStatusAndRaisesDomainEvent()),
    ("booking complete changes status and raises domain event", BookingCompleteChangesStatusAndRaisesDomainEvent()),
    ("booking no-show changes status and raises domain event", BookingNoShowChangesStatusAndRaisesDomainEvent())
};

foreach ((string name, bool passed) in checks)
{
    Console.WriteLine($"{(passed ? "PASS" : "FAIL")} {name}");
}

if (checks.Exists(check => !check.Passed))
{
    Environment.ExitCode = 1;
}

static bool HasEndpoint(System.Reflection.Assembly assembly)
{
    return assembly
        .GetTypes()
        .Any(type => type is { IsAbstract: false, IsInterface: false } &&
                     typeof(IEndpoint).IsAssignableFrom(type));
}

static bool HasEndpointNamed(System.Reflection.Assembly assembly, string typeName)
{
    return assembly
        .GetTypes()
        .Any(type => type.Name == typeName &&
                     type is { IsAbstract: false, IsInterface: false } &&
                     typeof(IEndpoint).IsAssignableFrom(type));
}

static bool HasTypeNamed(System.Reflection.Assembly assembly, string typeName)
{
    return assembly.GetTypes().Any(type => type.Name == typeName);
}

static bool HasTypeWithPublicProperty(
    System.Reflection.Assembly assembly,
    string typeName,
    string propertyName)
{
    return assembly
        .GetTypes()
        .Any(type => type.Name == typeName && HasPublicProperty(type, propertyName));
}

static bool HasPublicProperty(Type type, string propertyName)
{
    return type.GetProperty(propertyName) is not null;
}

static bool SourceContains(string relativePath, string expectedText)
{
    string path = Path.Combine(Directory.GetCurrentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    return File.Exists(path) &&
           File.ReadAllText(path).Contains(expectedText, StringComparison.Ordinal);
}

static bool HasConstructorParameterNamed(Type type, string parameterTypeName)
{
    return type.GetConstructors()
        .SelectMany(constructor => constructor.GetParameters())
        .Any(parameter => parameter.ParameterType.Name == parameterTypeName);
}

static bool BookingCreateRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: null,
        resourceId: Guid.NewGuid(),
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    return booking.Status == BookingStatus.Pending &&
           booking.DomainEvents.OfType<BookingCreatedDomainEvent>().Any();
}

static bool BookingCreateStoresIdempotencyKey()
{
    System.Reflection.MethodInfo? createMethod = typeof(Booking).GetMethod(
        "Create",
        [
            typeof(Guid),
            typeof(Guid),
            typeof(Guid),
            typeof(Guid?),
            typeof(Guid?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset),
            typeof(string)
        ]);

    if (createMethod is null)
    {
        return false;
    }

    object? booking = createMethod.Invoke(
        null,
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            "  public-retry-key  "
        ]);

    return booking is Booking typedBooking &&
           typeof(Booking).GetProperty("IdempotencyKey")?.GetValue(typedBooking) as string == "public-retry-key";
}

static bool BookingExposesOptimisticConcurrencyToken()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    object? concurrencyToken = typeof(Booking).GetProperty("ConcurrencyToken")?.GetValue(booking);

    return concurrencyToken is Guid token && token != Guid.Empty;
}

static bool BookingLifecycleMutationChangesConcurrencyToken()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.PropertyInfo? property = typeof(Booking).GetProperty("ConcurrencyToken");

    if (property?.GetValue(booking) is not Guid originalToken)
    {
        return false;
    }

    booking.ClearDomainEvents();
    booking.Cancel(DateTimeOffset.UtcNow);

    return property.GetValue(booking) is Guid changedToken &&
           changedToken != Guid.Empty &&
           changedToken != originalToken;
}

static bool CustomerRegisterNormalizesDataAndRaisesDomainEvent()
{
    Type? customerType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Customers.Customer");
    Type? registeredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Customers.CustomerRegisteredDomainEvent");

    if (customerType is null || registeredEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? registerMethod = customerType.GetMethod(
        "Register",
        [typeof(Guid), typeof(string), typeof(string), typeof(string)]);

    if (registerMethod is null)
    {
        return false;
    }

    object? customer = registerMethod.Invoke(
        null,
        [Guid.NewGuid(), "  Ali Valiyev  ", "  CUSTOMER@EXAMPLE.COM  ", " +998 90 123 45 67 "]);

    if (customer is not Entity entity)
    {
        return false;
    }

    return customerType.GetProperty("FullName")?.GetValue(customer) as string == "Ali Valiyev" &&
           customerType.GetProperty("Email")?.GetValue(customer) as string == "customer@example.com" &&
           customerType.GetProperty("PhoneNumber")?.GetValue(customer) as string == "+998 90 123 45 67" &&
           entity.DomainEvents.Any(registeredEventType.IsInstanceOfType);
}

static bool BookingHistoryRecordsStatusAndRaisesDomainEvent()
{
    Type? historyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingHistory.BookingHistoryEntry");
    Type? recordedEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingHistory.BookingHistoryRecordedDomainEvent");

    if (historyType is null || recordedEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? recordMethod = historyType.GetMethod(
        "Record",
        [typeof(Guid), typeof(Guid), typeof(BookingStatus), typeof(DateTimeOffset), typeof(string)]);

    if (recordMethod is null)
    {
        return false;
    }

    Guid tenantId = Guid.NewGuid();
    Guid bookingId = Guid.NewGuid();
    DateTimeOffset changedAtUtc = DateTimeOffset.UtcNow;

    object? history = recordMethod.Invoke(
        null,
        [tenantId, bookingId, BookingStatus.Confirmed, changedAtUtc, "  Confirmed by admin  "]);

    if (history is not Entity entity)
    {
        return false;
    }

    return historyType.GetProperty("TenantId")?.GetValue(history) is Guid returnedTenantId &&
           returnedTenantId == tenantId &&
           historyType.GetProperty("BookingId")?.GetValue(history) is Guid returnedBookingId &&
           returnedBookingId == bookingId &&
           historyType.GetProperty("Status")?.GetValue(history) is BookingStatus status &&
           status == BookingStatus.Confirmed &&
           historyType.GetProperty("Reason")?.GetValue(history) as string == "Confirmed by admin" &&
           entity.DomainEvents.Any(recordedEventType.IsInstanceOfType);
}

static bool BookingPolicyConfiguresRulesAndRaisesDomainEvent()
{
    Type? policyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicy");
    Type? configuredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicyConfiguredDomainEvent");

    if (policyType is null || configuredEventType is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? configureMethod = policyType.GetMethod(
        "Configure",
        [typeof(Guid), typeof(int), typeof(int)]);

    if (configureMethod is null)
    {
        return false;
    }

    Guid tenantId = Guid.NewGuid();
    object? policy = configureMethod.Invoke(null, [tenantId, 120, 12]);

    if (policy is not Entity entity)
    {
        return false;
    }

    return policyType.GetProperty("TenantId")?.GetValue(policy) is Guid returnedTenantId &&
           returnedTenantId == tenantId &&
           policyType.GetProperty("MinimumAdvanceMinutes")?.GetValue(policy) is int minimumAdvanceMinutes &&
           minimumAdvanceMinutes == 120 &&
           policyType.GetProperty("CancellationDeadlineHours")?.GetValue(policy) is int cancellationDeadlineHours &&
           cancellationDeadlineHours == 12 &&
           entity.DomainEvents.Any(configuredEventType.IsInstanceOfType);
}

static bool BookingPolicyBlocksBookingInsideMinimumAdvanceWindow()
{
    object? policy = CreateBookingPolicy(minimumAdvanceMinutes: 120, cancellationDeadlineHours: 12);

    if (policy is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? ensureMethod = policy.GetType().GetMethod(
        "EnsureBookingCanStartAt",
        [typeof(DateTimeOffset), typeof(DateTimeOffset)]);

    if (ensureMethod is null)
    {
        return false;
    }

    try
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        ensureMethod.Invoke(policy, [nowUtc.AddMinutes(90), nowUtc]);
        return false;
    }
    catch (System.Reflection.TargetInvocationException exception)
        when (exception.InnerException is InvalidOperationException)
    {
        return true;
    }
}

static bool BookingPolicyBlocksCancellationAfterDeadline()
{
    object? policy = CreateBookingPolicy(minimumAdvanceMinutes: 30, cancellationDeadlineHours: 12);

    if (policy is null)
    {
        return false;
    }

    System.Reflection.MethodInfo? ensureMethod = policy.GetType().GetMethod(
        "EnsureCancellationAllowed",
        [typeof(DateTimeOffset), typeof(DateTimeOffset)]);

    if (ensureMethod is null)
    {
        return false;
    }

    try
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        ensureMethod.Invoke(policy, [nowUtc.AddHours(6), nowUtc]);
        return false;
    }
    catch (System.Reflection.TargetInvocationException exception)
        when (exception.InnerException is InvalidOperationException)
    {
        return true;
    }
}

static object? CreateBookingPolicy(
    int minimumAdvanceMinutes,
    int cancellationDeadlineHours)
{
    Type? policyType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.BookingPolicies.BookingPolicy");
    System.Reflection.MethodInfo? configureMethod = policyType?.GetMethod(
        "Configure",
        [typeof(Guid), typeof(int), typeof(int)]);

    return configureMethod?.Invoke(null, [Guid.NewGuid(), minimumAdvanceMinutes, cancellationDeadlineHours]);
}

static bool OutboxMessageCapturesDomainEvent()
{
    var domainEvent = new BookingCreatedDomainEvent(
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddHours(1));

    OutboxMessage message = OutboxMessage.FromDomainEvent(domainEvent);

    return message.TenantId == domainEvent.TenantId &&
           message.Type.EndsWith(nameof(BookingCreatedDomainEvent), StringComparison.Ordinal) &&
           message.Payload.Contains(nameof(BookingCreatedDomainEvent.BookingId), StringComparison.Ordinal);
}

static bool OutboxMessageRecordsProcessingOutcome()
{
    var domainEvent = new BookingCancelledDomainEvent(
        Guid.NewGuid(),
        Guid.NewGuid(),
        DateTimeOffset.UtcNow);

    OutboxMessage message = OutboxMessage.FromDomainEvent(domainEvent);
    message.RecordFailure("temporary failure");
    bool failed = message.RetryCount == 1 && message.Error == "temporary failure" && message.ProcessedOnUtc is null;

    DateTime processedOnUtc = DateTime.UtcNow;
    message.MarkProcessed(processedOnUtc);

    return failed && message.RetryCount == 1 && message.Error is null && message.ProcessedOnUtc == processedOnUtc;
}

static bool BookingCancelChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    booking.ClearDomainEvents();
    booking.Cancel(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.Cancelled &&
           booking.DomainEvents.OfType<BookingCancelledDomainEvent>().Any();
}

static bool BookingRescheduleChangesTimeAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    DateTimeOffset newStart = DateTimeOffset.UtcNow.AddHours(3);
    DateTimeOffset newEnd = DateTimeOffset.UtcNow.AddHours(4);

    booking.ClearDomainEvents();
    booking.Reschedule(newStart, newEnd);

    return booking.Status == BookingStatus.Rescheduled &&
           booking.StartsAtUtc == newStart &&
           booking.EndsAtUtc == newEnd &&
           booking.DomainEvents.OfType<BookingRescheduledDomainEvent>().Any();
}

static bool BookingConfirmChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.MethodInfo? confirmMethod = typeof(Booking).GetMethod("Confirm", [typeof(DateTimeOffset)]);
    Type? confirmedEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Bookings.BookingConfirmedDomainEvent");

    if (confirmMethod is null || confirmedEventType is null)
    {
        return false;
    }

    booking.ClearDomainEvents();
    confirmMethod.Invoke(booking, [DateTimeOffset.UtcNow]);

    return booking.Status == BookingStatus.Confirmed &&
           booking.DomainEvents.Any(confirmedEventType.IsInstanceOfType);
}

static bool BookingExpireChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(2));

    System.Reflection.MethodInfo? expireMethod = typeof(Booking).GetMethod("Expire", [typeof(DateTimeOffset)]);
    Type? expiredEventType = typeof(Booking).Assembly.GetType(
        "ReserveFlow.Modules.Bookings.Domain.Bookings.BookingExpiredDomainEvent");

    if (expireMethod is null || expiredEventType is null)
    {
        return false;
    }

    booking.ClearDomainEvents();
    expireMethod.Invoke(booking, [DateTimeOffset.UtcNow]);

    return booking.Status == BookingStatus.Expired &&
           booking.DomainEvents.Any(expiredEventType.IsInstanceOfType);
}

static bool BookingCompleteChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(-2),
        DateTimeOffset.UtcNow.AddHours(-1));

    booking.ClearDomainEvents();
    booking.Complete(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.Completed &&
           booking.DomainEvents.OfType<BookingCompletedDomainEvent>().Any();
}

static bool BookingNoShowChangesStatusAndRaisesDomainEvent()
{
    Booking booking = Booking.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        staffMemberId: Guid.NewGuid(),
        resourceId: null,
        DateTimeOffset.UtcNow.AddHours(-2),
        DateTimeOffset.UtcNow.AddHours(-1));

    booking.ClearDomainEvents();
    booking.MarkAsNoShow(DateTimeOffset.UtcNow);

    return booking.Status == BookingStatus.NoShow &&
           booking.DomainEvents.OfType<BookingMarkedAsNoShowDomainEvent>().Any();
}

static bool TenantCreateNormalizesDataAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "  Smile Dental Clinic  ",
        "smile-dental",
        "Asia/Tashkent");

    return tenant.Name == "Smile Dental Clinic" &&
           tenant.Slug == "smile-dental" &&
           tenant.Status == TenantStatus.Pending &&
           tenant.DomainEvents.OfType<TenantProvisionedDomainEvent>().Any();
}

static bool TenantCreateCanAssignCategory()
{
    Guid categoryId = Guid.NewGuid();

    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent",
        categoryId);

    return tenant.CategoryId == categoryId;
}

static bool TenantCategoryCreateNormalizesDataAndRaisesDomainEvent()
{
    TenantCategory category = TenantCategory.Create(
        "  Dental Clinic  ",
        " DENTAL-CLINIC ",
        sortOrder: 20);

    return category.Name == "Dental Clinic" &&
           category.Slug == "dental-clinic" &&
           category.SortOrder == 20 &&
           category.IsActive &&
           category.DomainEvents.OfType<TenantCategoryCreatedDomainEvent>().Any();
}

static bool TenantActivateChangesStatusAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent");

    DateTimeOffset activatedAtUtc = DateTimeOffset.UtcNow;

    tenant.ClearDomainEvents();
    tenant.Activate(activatedAtUtc);

    return tenant.Status == TenantStatus.Active &&
           tenant.DomainEvents.OfType<TenantActivatedDomainEvent>().Any(domainEvent =>
               domainEvent.ActivatedAtUtc == activatedAtUtc);
}

static bool TenantSuspendChangesStatusAndRaisesDomainEvent()
{
    Tenant tenant = Tenant.Create(
        "Smile Dental Clinic",
        "smile-dental",
        "Asia/Tashkent");

    DateTimeOffset suspendedAtUtc = DateTimeOffset.UtcNow;

    tenant.Activate(DateTimeOffset.UtcNow);
    tenant.ClearDomainEvents();
    tenant.Suspend(suspendedAtUtc);

    return tenant.Status == TenantStatus.Suspended &&
           tenant.DomainEvents.OfType<TenantSuspendedDomainEvent>().Any(domainEvent =>
               domainEvent.SuspendedAtUtc == suspendedAtUtc);
}

static bool ServiceCreateNormalizesDataAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "  Dental Consultation  ",
        durationMinutes: 45,
        price: 200000m,
        currency: " uzs ");

    return service.Name == "Dental Consultation" &&
           service.DurationMinutes == 45 &&
           service.Currency == "UZS" &&
           service.IsActive &&
           service.DomainEvents.OfType<ServiceCreatedDomainEvent>().Any();
}

static bool ServiceUpdateChangesDetailsAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "Dental Consultation",
        durationMinutes: 45,
        price: 200000m,
        currency: "UZS");

    service.ClearDomainEvents();
    service.Update(
        "  Teeth Cleaning  ",
        durationMinutes: 30,
        price: 150000m,
        currency: " uzs ");

    return service.Name == "Teeth Cleaning" &&
           service.DurationMinutes == 30 &&
           service.Price == 150000m &&
           service.Currency == "UZS" &&
           service.DomainEvents.OfType<ServiceUpdatedDomainEvent>().Any();
}

static bool ServiceDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    Service service = Service.Create(
        Guid.NewGuid(),
        "Dental Consultation",
        durationMinutes: 45,
        price: 200000m,
        currency: "UZS");

    service.ClearDomainEvents();
    service.Deactivate();

    return !service.IsActive &&
           service.DomainEvents.OfType<ServiceDeactivatedDomainEvent>().Any();
}

static bool StaffMemberCreateNormalizesDataAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "  Dr. Ali  ",
        " ALI@SMILE.EXAMPLE ");

    return staffMember.DisplayName == "Dr. Ali" &&
           staffMember.Email == "ali@smile.example" &&
           staffMember.IsActive &&
           staffMember.DomainEvents.OfType<StaffMemberCreatedDomainEvent>().Any();
}

static bool StaffMemberUpdateChangesDetailsAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "Dr. Ali",
        "ali@smile.example");

    staffMember.ClearDomainEvents();
    staffMember.Update(
        "  Dr. Madina  ",
        " MADINA@SMILE.EXAMPLE ");

    return staffMember.DisplayName == "Dr. Madina" &&
           staffMember.Email == "madina@smile.example" &&
           staffMember.DomainEvents.OfType<StaffMemberUpdatedDomainEvent>().Any();
}

static bool StaffMemberDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    StaffMember staffMember = StaffMember.Create(
        Guid.NewGuid(),
        "Dr. Ali",
        "ali@smile.example");

    staffMember.ClearDomainEvents();
    staffMember.Deactivate();

    return !staffMember.IsActive &&
           staffMember.DomainEvents.OfType<StaffMemberDeactivatedDomainEvent>().Any();
}

static bool ResourceCreateNormalizesDataAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "  Room 2  ",
        " treatment-room ",
        capacity: 1);

    return resource.Name == "Room 2" &&
           resource.ResourceType == "treatment-room" &&
           resource.Capacity == 1 &&
           resource.IsActive &&
           resource.DomainEvents.OfType<ResourceCreatedDomainEvent>().Any();
}

static bool ResourceUpdateChangesDetailsAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "Room 2",
        "treatment-room",
        capacity: 1);

    resource.ClearDomainEvents();
    resource.Update(
        "  Room 3  ",
        " X-Ray-Room ",
        capacity: 2);

    return resource.Name == "Room 3" &&
           resource.ResourceType == "x-ray-room" &&
           resource.Capacity == 2 &&
           resource.DomainEvents.OfType<ResourceUpdatedDomainEvent>().Any();
}

static bool ResourceDeactivateChangesActiveFlagAndRaisesDomainEvent()
{
    Resource resource = Resource.Create(
        Guid.NewGuid(),
        "Room 2",
        "treatment-room",
        capacity: 1);

    resource.ClearDomainEvents();
    resource.Deactivate();

    return !resource.IsActive &&
           resource.DomainEvents.OfType<ResourceDeactivatedDomainEvent>().Any();
}

static bool WorkingHourCreateTargetsStaffOrResourceAndRaisesDomainEvent()
{
    Guid tenantId = Guid.NewGuid();
    Guid staffMemberId = Guid.NewGuid();

    WorkingHour workingHour = WorkingHour.Create(
        tenantId,
        staffMemberId,
        resourceId: null,
        DayOfWeek.Monday,
        new TimeOnly(9, 0),
        new TimeOnly(18, 0));

    return workingHour.TenantId == tenantId &&
           workingHour.StaffMemberId == staffMemberId &&
           workingHour.ResourceId is null &&
           workingHour.DomainEvents.OfType<WorkingHourCreatedDomainEvent>().Any();
}

static bool UnavailablePeriodCreateTargetsStaffOrResourceAndRaisesDomainEvent()
{
    Guid tenantId = Guid.NewGuid();
    Guid resourceId = Guid.NewGuid();
    DateTimeOffset startsAtUtc = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endsAtUtc = startsAtUtc.AddHours(2);

    UnavailablePeriod unavailablePeriod = UnavailablePeriod.Create(
        tenantId,
        staffMemberId: null,
        resourceId,
        startsAtUtc,
        endsAtUtc,
        "  Maintenance  ");

    return unavailablePeriod.TenantId == tenantId &&
           unavailablePeriod.ResourceId == resourceId &&
           unavailablePeriod.Reason == "Maintenance" &&
           unavailablePeriod.DomainEvents.OfType<UnavailablePeriodCreatedDomainEvent>().Any();
}

static bool NotificationQueueNormalizesDataAndRaisesDomainEvent()
{
    NotificationMessage message = NotificationMessage.Queue(
        Guid.NewGuid(),
        NotificationChannel.Email,
        "  CUSTOMER@EXAMPLE.COM  ",
        "  Booking confirmed  ",
        "  Your booking is confirmed.  ");

    return message.Recipient == "customer@example.com" &&
           message.Subject == "Booking confirmed" &&
           message.Body == "Your booking is confirmed." &&
           message.Status == NotificationStatus.Pending &&
           message.DomainEvents.OfType<NotificationQueuedDomainEvent>().Any();
}

static bool AuditLogNormalizesDataAndRaisesDomainEvent()
{
    AuditLog log = AuditLog.Record(
        tenantId: Guid.NewGuid(),
        userId: Guid.NewGuid(),
        action: "  Booking.Cancelled  ",
        entityName: "  Booking  ",
        entityId: Guid.NewGuid(),
        detailsJson: " {\"reason\":\"customer-request\"} ");

    return log.Action == "Booking.Cancelled" &&
           log.EntityName == "Booking" &&
           log.DetailsJson == "{\"reason\":\"customer-request\"}" &&
           log.DomainEvents.OfType<AuditLogRecordedDomainEvent>().Any();
}

static bool DailyBookingReportValidatesCountsAndRaisesDomainEvent()
{
    DailyBookingReport report = DailyBookingReport.Record(
        tenantId: Guid.NewGuid(),
        date: new DateOnly(2026, 6, 1),
        createdBookings: 12,
        cancelledBookings: 2,
        completedBookings: 8,
        noShowBookings: 1);

    return report.Date == new DateOnly(2026, 6, 1) &&
           report.CreatedBookings == 12 &&
           report.CancelledBookings == 2 &&
           report.CompletedBookings == 8 &&
           report.NoShowBookings == 1 &&
           report.DomainEvents.OfType<DailyBookingReportRecordedDomainEvent>().Any();
}

static bool WebhookInboxMessageNormalizesDataAndRaisesDomainEvent()
{
    WebhookInboxMessage message = WebhookInboxMessage.Accept(
        tenantId: Guid.NewGuid(),
        source: "  GoogleCalendar  ",
        externalMessageId: "  evt-123  ",
        eventType: "  CalendarEventChanged  ",
        payloadJson: " {\"id\":\"evt-123\"} ");

    return message.Source == "googlecalendar" &&
           message.ExternalMessageId == "evt-123" &&
           message.EventType == "CalendarEventChanged" &&
           message.PayloadJson == "{\"id\":\"evt-123\"}" &&
           message.Status == WebhookInboxStatus.Received &&
           message.DomainEvents.OfType<WebhookInboxMessageReceivedDomainEvent>().Any();
}

static bool KeycloakRoleParserAcceptsRealmAccessRoles()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("realm_access", "{\"roles\":[\"tenant-admin\"]}")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.TenantAdmin);
}

static bool KeycloakRoleParserAcceptsResourceAccessRoles()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("resource_access", "{\"reserveflow-api\":{\"roles\":[\"staff\"]}}")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.Staff);
}

static bool KeycloakRoleParserAcceptsSimpleRoleClaims()
{
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("roles", "PlatformAdmin")],
        authenticationType: "jwt"));

    return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.PlatformAdmin);
}

static bool AvailabilityEngineGeneratesFixedDurationSlots()
{
    var workingWindow = new AvailabilityWindow(
        new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));

    IReadOnlyList<AvailableSlot> slots = AvailabilityEngine.GenerateSlots(
        [workingWindow],
        serviceDuration: TimeSpan.FromMinutes(60),
        step: TimeSpan.FromMinutes(30));

    return slots.Count == 5 &&
           slots[0].StartsAtUtc == workingWindow.StartsAtUtc &&
           slots[^1].StartsAtUtc == new DateTimeOffset(2026, 6, 1, 11, 0, 0, TimeSpan.Zero);
}
