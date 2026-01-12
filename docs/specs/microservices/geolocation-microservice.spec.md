# Geolocation Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Provides mapping, geocoding, location tracking, and spatial queries.

## 2. Microservice Structure

### Core Layer (Geolocation.Core)
- Entities: Location, GeoFence, Route, Place
- Interfaces: ILocationRepository, IGeocodingService, IRoutingService
- Events: LocationUpdatedEvent, GeofenceEnteredEvent, GeofenceExitedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=GeolocationDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: GeolocationDbContext
- SQL Server spatial types (GEOGRAPHY)
- Azure Maps/Google Maps integration

### API Layer
- Endpoints: POST /api/locations, GET /api/locations/geocode, GET /api/locations/distance
- Support spatial queries: nearby, within radius, within polygon

## 3. Platform Tiers
- Basic: Address storage, static map display
- Standard: Geocoding/reverse geocoding, interactive maps, distance calculations
- Enterprise: Geofencing, route optimization, real-time tracking, spatial indexing, offline maps

## 4. Database Schema
- Locations: LocationId PK, Name, Address, Coordinates (GEOGRAPHY), CreatedAt
- GeoFences: GeofenceId PK, Name, Boundary (GEOGRAPHY), TriggerType

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
