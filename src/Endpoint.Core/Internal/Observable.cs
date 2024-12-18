// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Internal;

public class Observable<T> : IObservable<T>
{
    private readonly object @lock = new object();

    private IList<Subscription> subscriptions = new List<Subscription>();

    public Observable()
    {
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return AddSubscription(observer);
    }

    public void Unsubscribe(IObserver<T> observer)
    {
        RemoveSubscription(observer);
    }

    public void Broadcast(T item)
    {
        var subscriptions = this.subscriptions;
        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnNext(item);
        }
    }

    protected void BroadcastError(Exception error)
    {
        var subscriptions = this.subscriptions;
        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnError(error);
        }
    }

    protected void BroadcastOnCompleted()
    {
        var subscriptions = this.subscriptions;
        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnCompleted();
        }
    }

    private Subscription AddSubscription(IObserver<T> observer)
    {
        lock (@lock)
        {
            var subscriptions = new List<Subscription>(this.subscriptions);
            var subscription = new Subscription() { Observable = this, Observer = observer };
            subscriptions.Add(subscription);
            Interlocked.Exchange(ref this.subscriptions, subscriptions);
            return subscription;
        }
    }

    private void RemoveSubscription(IObserver<T> observer)
    {
        lock (@lock)
        {
            var subscriptions = new List<Subscription>(this.subscriptions);
            var subscription = subscriptions.FirstOrDefault(s => s.Observer == observer);
            if (subscription != null)
            {
                subscriptions.Remove(subscription);
                Interlocked.Exchange(ref this.subscriptions, subscriptions);
            }
        }
    }

    internal class Subscription : IDisposable
    {
        public required Observable<T> Observable;
        public required IObserver<T> Observer;

        public void Dispose()
        {
            Observable.Unsubscribe(Observer);
        }
    }
}
