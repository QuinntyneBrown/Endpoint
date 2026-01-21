# SOLID and Design Violations Audit

Scope
- Reviewed core C# code under `src` with focus on factories/generators and high‑fan‑in classes.
- This is not a full proof of absence for other files; findings below are the violations identified in the reviewed areas.

Violations

1) Interface Segregation Principle (ISP) — oversized factory interface
- Evidence: `src\Endpoint.Engineering\Microservices\IMicroserviceFactory.cs:11` and `src\Endpoint.Engineering\Microservices\IMicroserviceFactory.cs:156` show a single interface with many `Create*MicroserviceAsync` methods.
- Why this is a violation: consumers and implementers are forced to depend on methods they do not use.
- Impact: changes/additions to a single microservice force recompilation and re‑implementation across all implementers/consumers.
- Suggested fix: split into smaller interfaces per microservice or use a registry/strategy model keyed by microservice type.

2) Open/Closed Principle (OCP) + SRP — hard‑coded microservice list and switch
- Evidence: `src\Endpoint.Engineering\Microservices\MicroserviceFactory.cs:80` defines a static list of names; `src\Endpoint.Engineering\Microservices\MicroserviceFactory.cs:184` uses a large switch; constructor dependencies span many artifact factories (`src\Endpoint.Engineering\Microservices\MicroserviceFactory.cs:50`, `src\Endpoint.Engineering\Microservices\MicroserviceFactory.cs:147`).
- Why this is a violation: adding a microservice requires editing the list, switch, and constructor; the class also centralizes knowledge of all microservices.
- Impact: high coupling and frequent edits in a single class; elevated risk of regressions when adding microservices.
- Suggested fix: register microservice factories in DI and resolve by key (dictionary/strategy), or use a plugin registry.

3) Single Responsibility Principle (SRP) — microservice artifact factories own multiple layers
- Evidence: `src\Endpoint.Engineering\Microservices\Analytics\AnalyticsArtifactFactory.cs:36`, `src\Endpoint.Engineering\Microservices\Analytics\AnalyticsArtifactFactory.cs:70`, `src\Endpoint.Engineering\Microservices\Analytics\AnalyticsArtifactFactory.cs:98` show a single class creating Core, Infrastructure, and API artifacts.
- Why this is a violation: the class mixes concerns for multiple layers and dozens of file creation responsibilities.
- Impact: changes to one layer’s structure ripple through a large class; testing and maintenance become harder.
- Suggested fix: split into per‑layer factories (Core/Infrastructure/API) and compose them in a small coordinator.

4) Interface Segregation Principle (ISP) — oversized project factory interface
- Evidence: `src\Endpoint.DotNet\Artifacts\Projects\Factories\IProjectFactory.cs:9` through `src\Endpoint.DotNet\Artifacts\Projects\Factories\IProjectFactory.cs:51` define many unrelated `Create*` methods in a single interface.
- Why this is a violation: implementers/consumers depend on many unrelated project creation methods.
- Impact: higher coupling and unnecessary changes across modules when new project types are added.
- Suggested fix: split into smaller interfaces (e.g., `IWebProjectFactory`, `ITestProjectFactory`, `IFrontendProjectFactory`) or use a keyed registry.

5) Open/Closed Principle (OCP) — stringly‑typed project creation switch
- Evidence: `src\Endpoint.DotNet\Artifacts\Projects\Factories\ProjectFactory.cs:380` shows a `type switch` on string values to choose project type.
- Why this is a violation: new project types require modifying the switch and method.
- Impact: central factory becomes a modification hotspot; no extension point for new types.
- Suggested fix: use a strategy map keyed by type (string/enum) with registration, or replace with separate factories per type.

6) Single Responsibility Principle (SRP) — PlantUML solution factory mixes parsing and multi‑project orchestration
- Evidence: `src\Endpoint.DotNet\Artifacts\PlantUml\Services\PlantUmlSolutionModelFactory.cs:52` has a very large `CreateAsync` method; the same method handles Angular and Worker project creation (`src\Endpoint.DotNet\Artifacts\PlantUml\Services\PlantUmlSolutionModelFactory.cs:61`, `src\Endpoint.DotNet\Artifacts\PlantUml\Services\PlantUmlSolutionModelFactory.cs:88`).
- Why this is a violation: one method manages multiple stereotypes, project types, and orchestration steps.
- Impact: changes to one project type or stereotype require touching a very large method; testing individual behaviors is difficult.
- Suggested fix: split into per‑stereotype/project‑type strategies and a small orchestrator.

Notes
- No explicit LSP‑specific violations were identified in the inspected files.
