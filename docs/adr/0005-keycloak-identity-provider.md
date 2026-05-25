# ADR 0005: Use Keycloak As Identity Provider

## Status

Accepted

## Context

ReserveFlow needs authentication, roles, tenant-aware access, and user management. Building password storage, registration, password reset, MFA, account security, and SSO directly inside the MVP would distract from the core booking domain.

The project still needs application-specific authorization: tenant memberships, permissions, staff assignment checks, booking ownership, and platform admin access.

## Decision

Use Keycloak as the identity provider.

Keycloak owns:

- user login
- user registration when enabled
- password reset
- MFA and required actions
- SSO
- access token and refresh token issuance
- realm/client roles where useful

ReserveFlow owns:

- local app user profile mapped to Keycloak `sub`
- tenant membership
- application permissions
- resource-based authorization
- platform admin and tenant admin business rules
- audit logs for sensitive app actions

The ASP.NET Core 8 API validates Keycloak-issued access tokens using JWT bearer authentication. The Angular SPA uses a public Keycloak/OIDC client with Authorization Code + PKCE.

## Implementation Notes

- Local development runs Keycloak through Docker Compose.
- A development realm file should define `reserveflow`, SPA client, API audience/client, demo users, and demo roles.
- The SPA must not store a client secret.
- The backend may use Keycloak Admin REST API from server-side code for user provisioning/invites. Frontend code must not call the Admin REST API directly.
- `/api/auth/me` returns ReserveFlow application context: local user id, Keycloak subject, tenant memberships, roles, and permissions.
- ReserveFlow does not store password hashes or refresh tokens.

## Consequences

Positive:

- avoids custom password/security implementation
- supports SSO and MFA paths later
- keeps booking domain work focused
- uses standard OpenID Connect/OAuth2 integration

Tradeoffs:

- local development needs Keycloak container and realm setup
- auth troubleshooting now includes an external component
- user provisioning needs clear app-to-Keycloak boundaries
- application permissions must be synchronized or mapped carefully

## References

- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
- [Keycloak JavaScript adapter](https://www.keycloak.org/securing-apps/javascript-adapter)
- [Keycloak Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/index.html)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)

