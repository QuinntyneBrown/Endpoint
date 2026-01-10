# Design 3: Angular Frontend Integration Architecture

## Overview

This design defines the architecture for generating Angular frontend applications that integrate seamlessly with the generated .NET backend solutions. The system creates a complete full-stack development experience from PlantUML diagrams.

## Goals

1. Generate Angular workspace from PlantUML models
2. Create type-safe API client services
3. Generate NgRx state management
4. Scaffold feature modules with components
5. Support multiple Angular project configurations
6. Enable shared libraries for mono-repo development

## Angular Generation Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    PlantUML Semantic Model                               │
├─────────────────────────────────────────────────────────────────────────┤
│  Entities  │  DTOs  │  Commands  │  Queries  │  Relationships           │
└──────────────┬───────────────────────────────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                   Angular Model Transformer                              │
├─────────────────────────────────────────────────────────────────────────┤
│  TypeScriptTypeMapper  │  InterfaceGenerator  │  ServiceGenerator       │
└──────────────────────────────────────────────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Angular Workspace Model                               │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Angular Application                            │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐             │   │
│  │  │ App     │  │ Core    │  │ Shared  │  │Features │             │   │
│  │  │ Module  │  │ Module  │  │ Module  │  │ Modules │             │   │
│  │  └─────────┘  └─────────┘  └─────────┘  └─────────┘             │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Shared Libraries                               │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐             │   │
│  │  │ API     │  │ Models  │  │ State   │  │   UI    │             │   │
│  │  │ Client  │  │ Library │  │ Library │  │Components│             │   │
│  │  └─────────┘  └─────────┘  └─────────┘  └─────────┘             │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

## TypeScript Type Mapping

```csharp
namespace Endpoint.Angular.TypeMapping
{
    public interface ITypeScriptTypeMapper
    {
        string MapCSharpType(string csharpType);
        TypeScriptType CreateComplexType(PlantUmlClass plantUmlClass);
    }

    public class TypeScriptTypeMapper : ITypeScriptTypeMapper
    {
        private static readonly Dictionary<string, string> PrimitiveTypeMap = new()
        {
            ["string"] = "string",
            ["int"] = "number",
            ["long"] = "number",
            ["decimal"] = "number",
            ["double"] = "number",
            ["float"] = "number",
            ["bool"] = "boolean",
            ["boolean"] = "boolean",
            ["DateTime"] = "Date",
            ["DateTimeOffset"] = "Date",
            ["DateOnly"] = "string",
            ["TimeOnly"] = "string",
            ["Guid"] = "string",
            ["byte[]"] = "Blob",
            ["object"] = "unknown",
            ["dynamic"] = "any"
        };

        public string MapCSharpType(string csharpType)
        {
            // Handle nullable types
            if (csharpType.EndsWith("?"))
            {
                var baseType = csharpType.TrimEnd('?');
                return $"{MapCSharpType(baseType)} | null";
            }

            // Handle collections
            if (csharpType.StartsWith("List<") || csharpType.StartsWith("IEnumerable<") ||
                csharpType.StartsWith("ICollection<") || csharpType.StartsWith("IList<"))
            {
                var innerType = ExtractGenericType(csharpType);
                return $"{MapCSharpType(innerType)}[]";
            }

            // Handle dictionaries
            if (csharpType.StartsWith("Dictionary<") || csharpType.StartsWith("IDictionary<"))
            {
                var (keyType, valueType) = ExtractDictionaryTypes(csharpType);
                return $"{{ [key: {MapCSharpType(keyType)}]: {MapCSharpType(valueType)} }}";
            }

            // Primitive types
            if (PrimitiveTypeMap.TryGetValue(csharpType, out var tsType))
                return tsType;

            // Complex types - assume interface exists
            return csharpType;
        }
    }
}
```

## Angular Model Definitions

```csharp
namespace Endpoint.Angular.Models
{
    public class AngularWorkspaceModel
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public AngularApplicationModel MainApplication { get; set; }
        public List<AngularLibraryModel> Libraries { get; set; } = new();
        public AngularStyleConfiguration Styles { get; set; }
        public Dictionary<string, object> AngularJson { get; set; } = new();
    }

    public class AngularApplicationModel
    {
        public string Name { get; set; }
        public string Prefix { get; set; } = "app";
        public bool Standalone { get; set; } = true;
        public bool UseSignals { get; set; } = true;
        public RoutingConfiguration Routing { get; set; }
        public List<AngularFeatureModule> Features { get; set; } = new();
        public CoreModuleConfiguration Core { get; set; }
        public SharedModuleConfiguration Shared { get; set; }
    }

    public class AngularLibraryModel
    {
        public string Name { get; set; }
        public LibraryType Type { get; set; }
        public string ImportPath { get; set; }
        public List<TypeScriptFileModel> Files { get; set; } = new();
        public List<string> Exports { get; set; } = new();
    }

    public enum LibraryType
    {
        ApiClient,
        Models,
        State,
        UiComponents,
        Utilities
    }

    public class AngularFeatureModule
    {
        public string Name { get; set; }
        public string RoutePath { get; set; }
        public bool LazyLoaded { get; set; } = true;
        public List<AngularComponentModel> Components { get; set; } = new();
        public List<AngularServiceModel> Services { get; set; } = new();
        public NgRxFeatureState State { get; set; }
    }

    public class AngularComponentModel
    {
        public string Name { get; set; }
        public ComponentType Type { get; set; }
        public bool Standalone { get; set; } = true;
        public string TemplatePath { get; set; }
        public string StylePath { get; set; }
        public List<ComponentInput> Inputs { get; set; } = new();
        public List<ComponentOutput> Outputs { get; set; } = new();
        public List<string> Imports { get; set; } = new();
    }

    public enum ComponentType
    {
        Page,
        Smart,
        Presentational,
        Layout
    }

    public class AngularServiceModel
    {
        public string Name { get; set; }
        public ServiceType Type { get; set; }
        public string BaseUrl { get; set; }
        public List<ServiceMethod> Methods { get; set; } = new();
    }

    public enum ServiceType
    {
        Api,
        State,
        Utility
    }

    public class ServiceMethod
    {
        public string Name { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public string Endpoint { get; set; }
        public string RequestType { get; set; }
        public string ResponseType { get; set; }
        public List<ServiceParameter> Parameters { get; set; } = new();
    }
}
```

## API Client Generation

### Generated Service Example

```typescript
// libs/api-client/src/lib/services/orders.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Order,
  CreateOrderRequest,
  UpdateOrderRequest,
  PagedResult
} from '@ecommerce/models';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getAll(params?: { page?: number; pageSize?: number }): Observable<PagedResult<Order>> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<Order>>(`${this.baseUrl}/api/orders`, { params: httpParams });
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/api/orders/${id}`);
  }

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/api/orders`, request);
  }

  update(id: string, request: UpdateOrderRequest): Observable<Order> {
    return this.http.put<Order>(`${this.baseUrl}/api/orders/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/orders/${id}`);
  }
}
```

### Service Generator

```csharp
namespace Endpoint.Angular.Generation
{
    public interface IApiServiceGenerator
    {
        TypeScriptFileModel GenerateApiService(ControllerModel controller);
        TypeScriptFileModel GenerateApiIndex(List<ControllerModel> controllers);
    }

    public class ApiServiceGenerator : IApiServiceGenerator
    {
        private readonly ITypeScriptTypeMapper _typeMapper;
        private readonly ITypeScriptSyntaxGenerator _syntaxGenerator;

        public TypeScriptFileModel GenerateApiService(ControllerModel controller)
        {
            var entityName = controller.Name.Replace("Controller", "");
            var serviceName = $"{entityName}Service";
            var fileName = $"{entityName.ToKebabCase()}.service.ts";

            var imports = new List<ImportModel>
            {
                new() { Names = new[] { "Injectable", "inject" }, From = "@angular/core" },
                new() { Names = new[] { "HttpClient", "HttpParams" }, From = "@angular/common/http" },
                new() { Names = new[] { "Observable" }, From = "rxjs" }
            };

            var methods = controller.Actions.Select(GenerateServiceMethod).ToList();

            var serviceClass = new TypeScriptClassModel
            {
                Name = serviceName,
                Decorators = new[] { "@Injectable({ providedIn: 'root' })" },
                Properties = GenerateServiceProperties(),
                Methods = methods
            };

            return new TypeScriptFileModel
            {
                FileName = fileName,
                Imports = imports,
                Classes = new[] { serviceClass }
            };
        }

        private TypeScriptMethodModel GenerateServiceMethod(ActionModel action)
        {
            var httpMethod = action.HttpMethod.ToString().ToLower();
            var returnType = $"Observable<{_typeMapper.MapCSharpType(action.ReturnType)}>";

            return new TypeScriptMethodModel
            {
                Name = ToCamelCase(action.Name),
                Parameters = action.Parameters.Select(p => new TypeScriptParameterModel
                {
                    Name = ToCamelCase(p.Name),
                    Type = _typeMapper.MapCSharpType(p.Type),
                    IsOptional = !p.IsRequired
                }).ToList(),
                ReturnType = returnType,
                Body = GenerateMethodBody(action)
            };
        }
    }
}
```

## NgRx State Management Generation

### State Structure

```typescript
// libs/state/src/lib/orders/orders.state.ts
import { createFeature, createReducer, createSelector, on } from '@ngrx/store';
import { createEntityAdapter, EntityState } from '@ngrx/entity';
import { Order } from '@ecommerce/models';
import { OrdersActions } from './orders.actions';

export interface OrdersState extends EntityState<Order> {
  selectedId: string | null;
  loading: boolean;
  error: string | null;
}

const adapter = createEntityAdapter<Order>();

const initialState: OrdersState = adapter.getInitialState({
  selectedId: null,
  loading: false,
  error: null,
});

export const ordersFeature = createFeature({
  name: 'orders',
  reducer: createReducer(
    initialState,
    on(OrdersActions.loadOrders, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OrdersActions.loadOrdersSuccess, (state, { orders }) =>
      adapter.setAll(orders, { ...state, loading: false })
    ),
    on(OrdersActions.loadOrdersFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    on(OrdersActions.selectOrder, (state, { id }) => ({
      ...state,
      selectedId: id,
    })),
    on(OrdersActions.createOrderSuccess, (state, { order }) =>
      adapter.addOne(order, state)
    ),
    on(OrdersActions.updateOrderSuccess, (state, { order }) =>
      adapter.updateOne({ id: order.id, changes: order }, state)
    ),
    on(OrdersActions.deleteOrderSuccess, (state, { id }) =>
      adapter.removeOne(id, state)
    )
  ),
  extraSelectors: ({ selectOrdersState, selectEntities, selectSelectedId }) => ({
    ...adapter.getSelectors(selectOrdersState),
    selectSelectedOrder: createSelector(
      selectEntities,
      selectSelectedId,
      (entities, selectedId) => (selectedId ? entities[selectedId] : null)
    ),
  }),
});
```

### NgRx Generator

```csharp
namespace Endpoint.Angular.Generation
{
    public interface INgRxStateGenerator
    {
        NgRxFeatureFiles GenerateFeatureState(FeatureModel feature);
    }

    public class NgRxFeatureFiles
    {
        public TypeScriptFileModel ActionsFile { get; set; }
        public TypeScriptFileModel ReducerFile { get; set; }
        public TypeScriptFileModel EffectsFile { get; set; }
        public TypeScriptFileModel SelectorsFile { get; set; }
        public TypeScriptFileModel FacadeFile { get; set; }
        public TypeScriptFileModel IndexFile { get; set; }
    }

    public class NgRxStateGenerator : INgRxStateGenerator
    {
        public NgRxFeatureFiles GenerateFeatureState(FeatureModel feature)
        {
            var featureName = feature.Name.ToLower();

            return new NgRxFeatureFiles
            {
                ActionsFile = GenerateActions(feature),
                ReducerFile = GenerateReducer(feature),
                EffectsFile = GenerateEffects(feature),
                SelectorsFile = GenerateSelectors(feature),
                FacadeFile = GenerateFacade(feature),
                IndexFile = GenerateIndex(feature)
            };
        }

        private TypeScriptFileModel GenerateActions(FeatureModel feature)
        {
            // Generate actions using createActionGroup
            var actionGroup = $@"
import {{ createActionGroup, emptyProps, props }} from '@ngrx/store';
import {{ {feature.EntityName}, Create{feature.EntityName}Request, Update{feature.EntityName}Request }} from '@ecommerce/models';

export const {feature.Name}Actions = createActionGroup({{
  source: '{feature.Name}',
  events: {{
    'Load {feature.Name}': emptyProps(),
    'Load {feature.Name} Success': props<{{ {feature.EntityName.ToCamelCase()}s: {feature.EntityName}[] }}>(),
    'Load {feature.Name} Failure': props<{{ error: string }}>(),
    'Select {feature.EntityName}': props<{{ id: string }}>(),
    'Create {feature.EntityName}': props<{{ request: Create{feature.EntityName}Request }}>(),
    'Create {feature.EntityName} Success': props<{{ {feature.EntityName.ToCamelCase()}: {feature.EntityName} }}>(),
    'Create {feature.EntityName} Failure': props<{{ error: string }}>(),
    'Update {feature.EntityName}': props<{{ id: string; request: Update{feature.EntityName}Request }}>(),
    'Update {feature.EntityName} Success': props<{{ {feature.EntityName.ToCamelCase()}: {feature.EntityName} }}>(),
    'Update {feature.EntityName} Failure': props<{{ error: string }}>(),
    'Delete {feature.EntityName}': props<{{ id: string }}>(),
    'Delete {feature.EntityName} Success': props<{{ id: string }}>(),
    'Delete {feature.EntityName} Failure': props<{{ error: string }}>(),
  }},
}});";

            return new TypeScriptFileModel
            {
                FileName = $"{feature.Name.ToKebabCase()}.actions.ts",
                Content = actionGroup
            };
        }
    }
}
```

## Component Generation

### Smart Component (Container)

```typescript
// apps/ecommerce/src/app/features/orders/orders-list/orders-list.component.ts
import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OrdersFacade } from '@ecommerce/state';
import { OrderCardComponent } from '../components/order-card/order-card.component';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [CommonModule, RouterModule, OrderCardComponent],
  template: `
    <div class="orders-container">
      <header class="orders-header">
        <h1>Orders</h1>
        <button routerLink="new" class="btn-primary">Create Order</button>
      </header>

      @if (loading()) {
        <div class="loading-spinner">Loading...</div>
      }

      @if (error()) {
        <div class="error-message">{{ error() }}</div>
      }

      <div class="orders-grid">
        @for (order of orders(); track order.id) {
          <app-order-card
            [order]="order"
            (edit)="onEdit($event)"
            (delete)="onDelete($event)"
          />
        }
      </div>
    </div>
  `,
  styleUrls: ['./orders-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersListComponent {
  private readonly facade = inject(OrdersFacade);

  orders = this.facade.orders;
  loading = this.facade.loading;
  error = this.facade.error;

  onEdit(id: string): void {
    this.facade.selectOrder(id);
  }

  onDelete(id: string): void {
    if (confirm('Are you sure you want to delete this order?')) {
      this.facade.deleteOrder(id);
    }
  }
}
```

### Component Generator

```csharp
namespace Endpoint.Angular.Generation
{
    public interface IAngularComponentGenerator
    {
        ComponentFiles GenerateListComponent(FeatureModel feature);
        ComponentFiles GenerateDetailComponent(FeatureModel feature);
        ComponentFiles GenerateFormComponent(FeatureModel feature);
        ComponentFiles GenerateCardComponent(FeatureModel feature);
    }

    public class ComponentFiles
    {
        public TypeScriptFileModel Component { get; set; }
        public string Template { get; set; }
        public string Styles { get; set; }
        public TypeScriptFileModel Spec { get; set; }
    }

    public class AngularComponentGenerator : IAngularComponentGenerator
    {
        public ComponentFiles GenerateListComponent(FeatureModel feature)
        {
            var componentName = $"{feature.Name}ListComponent";
            var selector = $"app-{feature.Name.ToKebabCase()}-list";

            var template = GenerateListTemplate(feature);
            var styles = GenerateListStyles(feature);
            var component = GenerateListComponentClass(feature, componentName, selector);

            return new ComponentFiles
            {
                Component = component,
                Template = template,
                Styles = styles,
                Spec = GenerateComponentSpec(componentName)
            };
        }

        private string GenerateListTemplate(FeatureModel feature)
        {
            return $@"
<div class=""{feature.Name.ToKebabCase()}-container"">
  <header class=""page-header"">
    <h1>{feature.Name}</h1>
    <button routerLink=""new"" class=""btn-primary"">Create {feature.EntityName}</button>
  </header>

  @if (loading()) {{
    <div class=""loading-spinner"">Loading...</div>
  }}

  @if (error()) {{
    <div class=""error-message"">{{{{ error() }}}}</div>
  }}

  <div class=""{feature.Name.ToKebabCase()}-grid"">
    @for (item of {feature.Name.ToCamelCase()}(); track item.id) {{
      <app-{feature.EntityName.ToKebabCase()}-card
        [{feature.EntityName.ToCamelCase()}]=""item""
        (edit)=""onEdit($event)""
        (delete)=""onDelete($event)""
      />
    }}
  </div>
</div>";
        }
    }
}
```

## Generated Angular Structure

```
apps/
├── ecommerce/
│   ├── src/
│   │   ├── app/
│   │   │   ├── app.component.ts
│   │   │   ├── app.config.ts
│   │   │   ├── app.routes.ts
│   │   │   ├── core/
│   │   │   │   ├── interceptors/
│   │   │   │   │   ├── auth.interceptor.ts
│   │   │   │   │   └── error.interceptor.ts
│   │   │   │   ├── guards/
│   │   │   │   │   └── auth.guard.ts
│   │   │   │   └── services/
│   │   │   │       └── auth.service.ts
│   │   │   ├── shared/
│   │   │   │   ├── components/
│   │   │   │   │   ├── header/
│   │   │   │   │   ├── footer/
│   │   │   │   │   └── sidebar/
│   │   │   │   └── pipes/
│   │   │   └── features/
│   │   │       ├── orders/
│   │   │       │   ├── orders.routes.ts
│   │   │       │   ├── orders-list/
│   │   │       │   ├── order-detail/
│   │   │       │   ├── order-form/
│   │   │       │   └── components/
│   │   │       │       └── order-card/
│   │   │       └── products/
│   │   │           └── ...
│   │   ├── environments/
│   │   │   ├── environment.ts
│   │   │   └── environment.prod.ts
│   │   └── styles/
│   │       └── styles.scss
│   └── project.json
│
libs/
├── api-client/
│   └── src/
│       ├── lib/
│       │   ├── services/
│       │   │   ├── orders.service.ts
│       │   │   └── products.service.ts
│       │   ├── interceptors/
│       │   └── tokens.ts
│       └── index.ts
│
├── models/
│   └── src/
│       ├── lib/
│       │   ├── order.model.ts
│       │   ├── product.model.ts
│       │   ├── requests/
│       │   │   ├── create-order.request.ts
│       │   │   └── update-order.request.ts
│       │   └── responses/
│       │       └── paged-result.ts
│       └── index.ts
│
├── state/
│   └── src/
│       ├── lib/
│       │   ├── orders/
│       │   │   ├── orders.actions.ts
│       │   │   ├── orders.reducer.ts
│       │   │   ├── orders.effects.ts
│       │   │   ├── orders.facade.ts
│       │   │   └── index.ts
│       │   └── products/
│       │       └── ...
│       └── index.ts
│
└── ui-components/
    └── src/
        ├── lib/
        │   ├── button/
        │   ├── card/
        │   ├── table/
        │   └── form-controls/
        └── index.ts
```

## Command Integration

```csharp
[Verb("angular-from-plantuml", HelpText = "Generate Angular frontend from PlantUML")]
public class AngularFromPlantUmlRequest : IRequest
{
    [Option('f', "file", Required = true, HelpText = "Path to PlantUML file")]
    public string FilePath { get; set; }

    [Option('n', "name", Required = true, HelpText = "Application name")]
    public string Name { get; set; }

    [Option('o', "output", HelpText = "Output directory")]
    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

    [Option("api-url", Default = "http://localhost:5000", HelpText = "API base URL")]
    public string ApiBaseUrl { get; set; }

    [Option("standalone", Default = true, HelpText = "Use standalone components")]
    public bool UseStandalone { get; set; }

    [Option("signals", Default = true, HelpText = "Use Angular signals")]
    public bool UseSignals { get; set; }

    [Option("state", Default = "ngrx", HelpText = "State management (ngrx, component-store, signals)")]
    public string StateManagement { get; set; }

    [Option("css", Default = "scss", HelpText = "CSS preprocessor")]
    public string CssPreprocessor { get; set; }
}
```

## Configuration

### Angular Generation Options

```json
{
  "angular": {
    "version": "17",
    "workspace": {
      "name": "ecommerce",
      "prefix": "app"
    },
    "application": {
      "standalone": true,
      "signals": true,
      "routing": true,
      "lazyLoading": true
    },
    "state": {
      "type": "ngrx",
      "entityAdapter": true,
      "facade": true
    },
    "styling": {
      "preprocessor": "scss",
      "theme": "material"
    },
    "libraries": {
      "apiClient": true,
      "models": true,
      "state": true,
      "uiComponents": true
    },
    "features": {
      "authentication": true,
      "errorHandling": true,
      "loading": true
    }
  }
}
```
