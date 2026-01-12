# Calculation Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Performs complex financial calculations, projections, and simulations.

## 2. Microservice Structure

### Core Layer (Calculation.Core)
- Entities: Calculation, Formula, CalculationResult, Scenario
- Interfaces: ICalculationRepository, IFormulaEngine, ISimulationService
- Events: CalculationCompletedEvent, ScenarioCreatedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=CalculationDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: CalculationDbContext
- Financial formula libraries

### API Layer
- Endpoints: POST /api/calculations/compute, GET /api/calculations/{id}, POST /api/calculations/scenarios
- Support formulas: amortization, compound interest, NPV, IRR

## 3. Platform Tiers
- Basic: Simple arithmetic in application code
- Standard: Financial formulas, what-if scenarios
- Enterprise: Monte Carlo simulations, custom formula engine, batch calculations, audit trail, regulatory compliance

## 4. Database Schema
- Calculations: CalculationId PK, FormulaType, Inputs (JSON), Result (DECIMAL), CreatedAt
- Scenarios: ScenarioId PK, Name, Variables (JSON), CreatedAt
- CalculationHistory: HistoryId PK, CalculationId FK, Result, Timestamp

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
