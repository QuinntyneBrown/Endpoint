# Messaging Commands

Commands for adding messaging infrastructure and event-driven patterns to your applications.

## Available Commands

| Command | Description |
|---------|-------------|
| [messaging-add](./messaging-add.user-guide.md) | Add messaging to a solution |
| [messaging-project-add](./messaging-project-add.user-guide.md) | Add a messaging project |

## Quick Examples

```bash
# Add messaging support to an existing solution
endpoint messaging-add -n MySolution --lz4

# Add messaging project with Redis support
endpoint messaging-project-add --include-redis
```

[Back to Index](../index.md)
