# Localization Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Manages translations, regional settings, and internationalization across applications.

## 2. Microservice Structure

### Core Layer (Localization.Core)
- Entities: Translation, Language, LocalizedResource, TranslationKey
- Interfaces: ITranslationRepository, ILocalizationService
- Events: TranslationAddedEvent, LanguageEnabledEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=LocalizationDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: LocalizationDbContext
- Support for RESX, JSON, and database-driven translations

### API Layer
- Endpoints: GET /api/translations/{languageCode}, POST /api/translations, PUT /api/translations/{key}
- Support locale detection from Accept-Language header

## 3. Platform Tiers
- Basic: Single language support with hardcoded strings
- Standard: Multi-language support, translation management, locale detection
- Enterprise: Translation workflow, machine translation integration, RTL support, regional formatting, translation memory

## 4. Database Schema
- Languages: LanguageCode (PK), Name, IsEnabled, IsDefault, CreatedAt
- Translations: TranslationId PK, Key, LanguageCode (FK), Value, CreatedAt
- TranslationKeys: KeyId PK, Key (unique), Category, Description

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
