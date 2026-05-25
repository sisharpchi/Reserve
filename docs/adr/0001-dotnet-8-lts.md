# ADR 0001: Use .NET 8 LTS

## Status

Accepted

## Context

ReserveFlow is intended as a production-style learning and portfolio project. It should use a stable runtime with broad ecosystem support and predictable package compatibility.

The user explicitly selected a .NET 8 baseline.

## Decision

Use:

- .NET 8 LTS
- ASP.NET Core 8
- C# 12
- EF Core 8.x LTS
- Npgsql 8.x
- Keycloak integration through standard OpenID Connect/OAuth2 libraries

Use latest available patch versions in the selected major line.

## Consequences

Positive:

- stable long-term-support baseline
- strong compatibility with ASP.NET Core, EF Core, Npgsql, Testcontainers, OpenTelemetry, and common CI environments
- avoids preview or newer-runtime churn while the product model is still being built

Tradeoffs:

- newer platform APIs outside the .NET 8 line are not baseline features
- docs and implementation must verify that packages support .NET 8

## References

- [.NET release and support policy](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [EF Core 8 release notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)
