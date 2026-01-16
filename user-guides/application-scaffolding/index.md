# Application Scaffolding Commands

Commands for creating complete application templates, microservices, and full-stack applications.

## Available Commands

| Command | Description |
|---------|-------------|
| [mwa-create](./mwa-create.user-guide.md) | Create a Modern Web Application |
| [ddd-app-create](./ddd-app-create.user-guide.md) | Create a DDD application |
| [react-app-create](./react-app-create.user-guide.md) | Create a React application |
| [event-driven-microservices-create](./event-driven-microservices-create.user-guide.md) | Create event-driven microservices |
| [worker-create](./worker-create.user-guide.md) | Create a worker/console application |

## Quick Examples

```bash
# Create a DDD application with a ToDo aggregate
endpoint ddd-app-create -n MyApp -a ToDo -p "ToDoId:Guid,Title:string,IsComplete:bool"

# Create a Modern Web Application
endpoint mwa-create -n MyWebApp

# Create event-driven microservices
endpoint event-driven-microservices-create -n MyPlatform -s "Orders,Inventory,Shipping"
```

[Back to Index](../index.md)
