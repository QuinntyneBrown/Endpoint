# Endpoint.Lit

The Lit web components code generation module for the Endpoint framework. This library provides generators for creating Lit-based web components with TypeScript support.

## Overview

Endpoint.Lit enables the generation of Lit web components that can be used across any frontend framework or vanilla JavaScript applications. Lit provides a lightweight, fast foundation for building web components.

## Key Features

- **Web Component Generation**: Standards-based custom elements
- **TypeScript Support**: Full TypeScript decorators and types
- **Reactive Properties**: Lit's reactive property system
- **Shadow DOM**: Encapsulated styles and markup
- **Template Literals**: Efficient lit-html templates

## Usage

Endpoint.Lit is typically used through the Endpoint.Cli:

```bash
# Generate a Lit component
endpoint lit component --name CustomerCard

# Generate a Lit component with properties
endpoint lit component --name ProductList --properties "items:array,loading:boolean"
```

## Generated Artifacts

Example generated Lit component:

```typescript
import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('customer-card')
export class CustomerCard extends LitElement {
  static styles = css`
    :host {
      display: block;
    }
  `;

  @property({ type: String })
  name = '';

  render() {
    return html`
      <div class="card">
        <h2>${this.name}</h2>
      </div>
    `;
  }
}
```

## Why Lit?

- **Framework Agnostic**: Use generated components anywhere
- **Lightweight**: Small bundle size (~5KB)
- **Standards-Based**: Built on web standards
- **Fast**: Efficient rendering with lit-html

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
