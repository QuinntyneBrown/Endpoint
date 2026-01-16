# react-app-create

Create a React application scaffold.

## Synopsis

```bash
endpoint react-app-create [options]
```

## Description

The `react-app-create` command generates a new React application with a modern project structure. This provides a starting point for building React-based frontend applications.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the React application | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Create a React application

```bash
endpoint react-app-create -n my-react-app
```

### Specify output directory

```bash
endpoint react-app-create -n dashboard -d ./frontend
```

## Generated Structure

```
my-react-app/
├── public/
│   ├── index.html
│   └── favicon.ico
├── src/
│   ├── components/
│   ├── pages/
│   ├── services/
│   ├── hooks/
│   ├── App.tsx
│   ├── index.tsx
│   └── index.css
├── package.json
├── tsconfig.json
└── README.md
```

## Common Use Cases

1. **Frontend Development**: Create React frontends
2. **Full-Stack Applications**: Frontend for .NET backend
3. **SPAs**: Single Page Applications
4. **Dashboards**: Admin dashboards and data visualization

## Getting Started

After creation:

```bash
cd my-react-app
npm install
npm start
```

## Integration with Backend

Combine with backend commands:

```bash
# Create backend
endpoint ddd-app-create -n MyApp -a Product

# Create frontend
endpoint react-app-create -n my-app-frontend
```

## Related Commands

- [ddd-app-create](./ddd-app-create.user-guide.md) - Create backend API
- [mwa-create](./mwa-create.user-guide.md) - Create full-stack application

[Back to Application Scaffolding](./index.md) | [Back to Index](../index.md)
