# Custom Serializers

## Overview

The `IMessageSerializer` interface allows you to implement custom serialization formats for your messages.

## Interface Definition

```csharp
public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);
    T Deserialize<T>(byte[] data);
    object Deserialize(byte[] data, Type type);
}
```

## Example: Protocol Buffers Serializer

```csharp
using Google.Protobuf;

public class ProtobufMessageSerializer : IMessageSerializer
{
    public byte[] Serialize<T>(T message)
    {
        if (message is IMessage protoMessage)
        {
            return protoMessage.ToByteArray();
        }
        throw new ArgumentException("Message must implement IMessage");
    }

    public T Deserialize<T>(byte[] data)
    {
        var parser = (MessageParser<T>)typeof(T)
            .GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null);
        return parser!.ParseFrom(data);
    }

    public object Deserialize(byte[] data, Type type)
    {
        var method = GetType().GetMethod(nameof(Deserialize), new[] { typeof(byte[]) });
        var generic = method!.MakeGenericMethod(type);
        return generic.Invoke(this, new object[] { data })!;
    }
}
```

## Registering Custom Serializers

Register your serializer in the DI container:

```csharp
services.AddSingleton<IMessageSerializer, ProtobufMessageSerializer>();
```

## Considerations

When implementing custom serializers, consider:

- **Performance**: Serialization/deserialization is on the hot path
- **Compatibility**: Ensure all services use compatible serializer versions
- **Schema Evolution**: Plan for how to handle message schema changes
- **Error Handling**: Provide clear error messages for serialization failures
