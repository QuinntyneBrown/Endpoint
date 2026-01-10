// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.UnitTests;

public class ObservableTests
{
    #region Subscribe Tests

    [Fact]
    public void Subscribe_ReturnsDisposable()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        var subscription = observable.Subscribe(observer);

        Assert.NotNull(subscription);
        Assert.IsAssignableFrom<IDisposable>(subscription);
    }

    [Fact]
    public void Subscribe_AllowsReceivingBroadcasts()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        observable.Subscribe(observer);
        observable.Broadcast("Test Message");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("Test Message", observer.ReceivedItems[0]);
    }

    [Fact]
    public void Subscribe_MultipleObservers_AllReceiveBroadcasts()
    {
        var observable = new Observable<int>();
        var observer1 = new TestObserver<int>();
        var observer2 = new TestObserver<int>();
        var observer3 = new TestObserver<int>();

        observable.Subscribe(observer1);
        observable.Subscribe(observer2);
        observable.Subscribe(observer3);
        observable.Broadcast(42);

        Assert.Single(observer1.ReceivedItems);
        Assert.Single(observer2.ReceivedItems);
        Assert.Single(observer3.ReceivedItems);
        Assert.Equal(42, observer1.ReceivedItems[0]);
        Assert.Equal(42, observer2.ReceivedItems[0]);
        Assert.Equal(42, observer3.ReceivedItems[0]);
    }

    #endregion

    #region Broadcast Tests

    [Fact]
    public void Broadcast_WithNoSubscribers_DoesNotThrow()
    {
        var observable = new Observable<string>();

        var exception = Record.Exception(() => observable.Broadcast("Test"));

        Assert.Null(exception);
    }

    [Fact]
    public void Broadcast_MultipleTimes_AllItemsReceived()
    {
        var observable = new Observable<int>();
        var observer = new TestObserver<int>();

        observable.Subscribe(observer);
        observable.Broadcast(1);
        observable.Broadcast(2);
        observable.Broadcast(3);

        Assert.Equal(3, observer.ReceivedItems.Count);
        Assert.Equal(new[] { 1, 2, 3 }, observer.ReceivedItems);
    }

    [Fact]
    public void Broadcast_ToMultipleObservers_MaintainsOrder()
    {
        var observable = new Observable<string>();
        var observer1 = new TestObserver<string>();
        var observer2 = new TestObserver<string>();

        observable.Subscribe(observer1);
        observable.Subscribe(observer2);
        observable.Broadcast("First");
        observable.Broadcast("Second");

        Assert.Equal(new[] { "First", "Second" }, observer1.ReceivedItems);
        Assert.Equal(new[] { "First", "Second" }, observer2.ReceivedItems);
    }

    [Fact]
    public void Broadcast_WithComplexObject_DeliversCorrectly()
    {
        var observable = new Observable<TestData>();
        var observer = new TestObserver<TestData>();
        var testData = new TestData { Id = 1, Name = "Test" };

        observable.Subscribe(observer);
        observable.Broadcast(testData);

        Assert.Single(observer.ReceivedItems);
        Assert.Same(testData, observer.ReceivedItems[0]);
    }

    #endregion

    #region Unsubscribe Tests

    [Fact]
    public void Unsubscribe_StopsReceivingBroadcasts()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        observable.Subscribe(observer);
        observable.Broadcast("Before");
        observable.Unsubscribe(observer);
        observable.Broadcast("After");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("Before", observer.ReceivedItems[0]);
    }

    [Fact]
    public void Unsubscribe_NonSubscribedObserver_DoesNotThrow()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        var exception = Record.Exception(() => observable.Unsubscribe(observer));

        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_OneOfMultiple_OthersStillReceive()
    {
        var observable = new Observable<int>();
        var observer1 = new TestObserver<int>();
        var observer2 = new TestObserver<int>();

        observable.Subscribe(observer1);
        observable.Subscribe(observer2);
        observable.Broadcast(1);
        observable.Unsubscribe(observer1);
        observable.Broadcast(2);

        Assert.Single(observer1.ReceivedItems);
        Assert.Equal(2, observer2.ReceivedItems.Count);
    }

    [Fact]
    public void Unsubscribe_TwiceSameObserver_DoesNotThrow()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        observable.Subscribe(observer);
        observable.Unsubscribe(observer);

        var exception = Record.Exception(() => observable.Unsubscribe(observer));

        Assert.Null(exception);
    }

    #endregion

    #region Dispose Subscription Tests

    [Fact]
    public void DisposeSubscription_StopsReceivingBroadcasts()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        var subscription = observable.Subscribe(observer);
        observable.Broadcast("Before");
        subscription.Dispose();
        observable.Broadcast("After");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("Before", observer.ReceivedItems[0]);
    }

    [Fact]
    public void DisposeSubscription_CalledMultipleTimes_DoesNotThrow()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        var subscription = observable.Subscribe(observer);
        subscription.Dispose();

        var exception = Record.Exception(() => subscription.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void DisposeSubscription_OnlyAffectsOwnSubscription()
    {
        var observable = new Observable<int>();
        var observer1 = new TestObserver<int>();
        var observer2 = new TestObserver<int>();

        var subscription1 = observable.Subscribe(observer1);
        var subscription2 = observable.Subscribe(observer2);

        observable.Broadcast(1);
        subscription1.Dispose();
        observable.Broadcast(2);

        Assert.Single(observer1.ReceivedItems);
        Assert.Equal(2, observer2.ReceivedItems.Count);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Observable_WithNullBroadcast_DeliversNull()
    {
        var observable = new Observable<string?>();
        var observer = new TestObserver<string?>();

        observable.Subscribe(observer);
        observable.Broadcast(null);

        Assert.Single(observer.ReceivedItems);
        Assert.Null(observer.ReceivedItems[0]);
    }

    [Fact]
    public void Observable_SubscribeSameObserverTwice_ReceivesBroadcastTwice()
    {
        var observable = new Observable<int>();
        var observer = new TestObserver<int>();

        observable.Subscribe(observer);
        observable.Subscribe(observer);
        observable.Broadcast(42);

        // Observer subscribed twice, so it receives broadcast twice
        Assert.Equal(2, observer.ReceivedItems.Count);
    }

    [Fact]
    public void Observable_WithValueType_WorksCorrectly()
    {
        var observable = new Observable<int>();
        var observer = new TestObserver<int>();

        observable.Subscribe(observer);
        observable.Broadcast(0);
        observable.Broadcast(int.MinValue);
        observable.Broadcast(int.MaxValue);

        Assert.Equal(3, observer.ReceivedItems.Count);
        Assert.Equal(new[] { 0, int.MinValue, int.MaxValue }, observer.ReceivedItems);
    }

    [Fact]
    public void Observable_LargeNumberOfSubscribers_HandlesCorrectly()
    {
        var observable = new Observable<int>();
        var observers = new List<TestObserver<int>>();

        for (int i = 0; i < 100; i++)
        {
            var observer = new TestObserver<int>();
            observers.Add(observer);
            observable.Subscribe(observer);
        }

        observable.Broadcast(42);

        foreach (var observer in observers)
        {
            Assert.Single(observer.ReceivedItems);
            Assert.Equal(42, observer.ReceivedItems[0]);
        }
    }

    #endregion

    #region Test Helpers

    private class TestObserver<T> : IObserver<T>
    {
        public List<T> ReceivedItems { get; } = new List<T>();
        public List<Exception> Errors { get; } = new List<Exception>();
        public bool IsCompleted { get; private set; }

        public void OnCompleted()
        {
            IsCompleted = true;
        }

        public void OnError(Exception error)
        {
            Errors.Add(error);
        }

        public void OnNext(T value)
        {
            ReceivedItems.Add(value);
        }
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
