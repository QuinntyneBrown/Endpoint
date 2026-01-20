# Architecture Guide

## Overview

This shared library follows a layered architecture designed for maximum flexibility and maintainability in microservices environments.

## Design Principles

1. **Abstraction over Implementation**: Core interfaces define contracts; implementations can be swapped
2. **Protocol Agnostic**: Events and commands are serialized using pluggable serializers
3. **Domain-Driven Design**: Strongly-typed IDs and value objects enforce domain rules
4. **Dependency Injection**: All services are registered via extension methods

## Layer Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                    Aggregator Project                        │
│                (MissionControlShared.Shared)                │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│   Shared.Domain   │ │  Shared.Contracts │ │ Protocol Projects │
│ (IDs, Results)  │ │    (Events)     │ │  (Event Buses)  │
└─────────────────┘ └─────────────────┘ └─────────────────┘
              │               │               │
              └───────────────┼───────────────┘
                              │
                              ▼
              ┌─────────────────────────────────┐
              │  Shared.Messaging.Abstractions  │
              │     (Core Interfaces)           │
              └─────────────────────────────────┘
```

## Projects

- [Project Structure](./project-structure.md) - Detailed breakdown of each project

