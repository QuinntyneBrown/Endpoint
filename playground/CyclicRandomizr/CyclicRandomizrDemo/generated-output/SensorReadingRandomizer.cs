// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CyclicRandomizrDemo;

namespace CyclicRandomizrDemo;

public class SensorReadingRandomizer
{
    private readonly Random _random = new();

    private CancellationTokenSource _cts;

    private bool _isRunning;

    private Task _runningTask;

    public double Hz { get; set; } = 1.0;
    public HashSet<string> ExcludedProperties { get; set; } = new();
    public bool IsRunning { get; set; }
    public async Task StartAsync(ChannelWriter<SensorReading> writer,CancellationToken cancellationToken)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Randomizer is already running.");
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        _runningTask = Task.Run(async () =>
        {
            try
            {
                var delayMs = (int)(1000.0 / Hz);
                while (!_cts.Token.IsCancellationRequested)
                {
                    var instance = CreateRandomInstance();
                    await writer.WriteAsync(instance, _cts.Token);
                    await Task.Delay(delayMs, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            finally
            {
                _isRunning = false;
            }
        }, _cts.Token);

        await Task.CompletedTask;

    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _cts?.Cancel();

    }

    public async Task StopAsync()
    {
        Stop();

        if (_runningTask != null)
        {
            try
            {
                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

    }

    public SensorReading CreateRandomInstance()
    {
        var instance = Activator.CreateInstance<SensorReading>();
        var properties = typeof(SensorReading).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (ExcludedProperties.Contains(property.Name))
            {
                continue;
            }

            if (!property.CanWrite)
            {
                continue;
            }

            var randomValue = GenerateRandomValue(property.PropertyType);
            if (randomValue != null)
            {
                property.SetValue(instance, randomValue);
            }
        }

        return instance;

    }

    private object GenerateRandomValue(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(int))
        {
            return _random.Next();
        }
        else if (underlyingType == typeof(long))
        {
            return (long)_random.Next() << 32 | (uint)_random.Next();
        }
        else if (underlyingType == typeof(short))
        {
            return (short)_random.Next(short.MinValue, short.MaxValue);
        }
        else if (underlyingType == typeof(byte))
        {
            return (byte)_random.Next(0, 256);
        }
        else if (underlyingType == typeof(double))
        {
            return _random.NextDouble() * 1000;
        }
        else if (underlyingType == typeof(float))
        {
            return (float)(_random.NextDouble() * 1000);
        }
        else if (underlyingType == typeof(decimal))
        {
            return (decimal)(_random.NextDouble() * 1000);
        }
        else if (underlyingType == typeof(bool))
        {
            return _random.Next(2) == 1;
        }
        else if (underlyingType == typeof(string))
        {
            return Guid.NewGuid().ToString("N")[..8];
        }
        else if (underlyingType == typeof(Guid))
        {
            return Guid.NewGuid();
        }
        else if (underlyingType == typeof(DateTime))
        {
            var range = (DateTime.MaxValue - DateTime.MinValue).Days;
            return DateTime.MinValue.AddDays(_random.Next(range));
        }
        else if (underlyingType == typeof(DateTimeOffset))
        {
            var range = (DateTimeOffset.MaxValue - DateTimeOffset.MinValue).Days;
            return DateTimeOffset.MinValue.AddDays(_random.Next(range));
        }
        else if (underlyingType == typeof(TimeSpan))
        {
            return TimeSpan.FromMilliseconds(_random.Next(0, 86400000));
        }
        else if (underlyingType.IsEnum)
        {
            var values = Enum.GetValues(underlyingType);
            return values.GetValue(_random.Next(values.Length));
        }

        return null;

    }

}

