# EventBroker
[![NuGet](https://img.shields.io/nuget/v/M.EventBroker.svg)](https://www.nuget.org/packages/M.EventBroker/1.0.0)  

EventBroker is an implementation of publish/subscribe pattern.  

## Specifics

- in-memory, in-process scope
- publishing is 'fire and forget' style  
- when published event handlers are queued and invoked asynchronously
- doesn't use ThreadPool, uses preconfigured count of 'dedicated' threads to execute event handlers
- provides two ways for subscribing that can be used side by side
- subscriptions are based on the event type
- event handlers can provide filter for the events they want to handle  
- events don't need to implement specific interface

## Usage

### Creating an EventBroker  

	IEventBroker broker = new EventBroker(3);
  
The only required paramerer for the EventBroker constructor is the count of threads that will be used for event handling.  
There are two optional parameters:  
*Action&lt;Exception&gt;* - a delegate invoked if exception is thrown from event handler. EventBroker mutes exceptions.  
*Func&lt;Type, IEnumerable&lt;object&gt;&gt;* - a factory delegate expected to return instance(s) of IEventHandler of the given type.  

### Subscribing for Events

Having event
  
    public class MyEvent
    {
        public MyEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

and event handler  

    public class MyEventHandler : IEventHandler<MyEvent>
    {
        public void Handle(MyEvent @event)
        {
            Console.WriteLine(@event.Message);
        }

        public bool ShouldHandle(MyEvent @event)
        {
            return true;
        }
    }

There are two ways for subscribing.  

**First one** is to use `Subscribe/Unsubscribe` methods of `IEventBroker` passing instance of IEventHadler
	
	var handler = new MyEventHandler();
	broker.Subscribe<MyEventHandler>(handler);

To stop receiving events use the same handler instance to unsubscribe

	broker.Unsubscribe<MyEventHandler>(handler);

`Subscribe/Unsubscribe` methods have overloads taking delegates  

	Action<MyEvent> handler = x => Console.WriteLine(x.Message);
	Func<MyEvent, bool> filter = x => true;
	
	broker.Subscribe(handler, filter);
	...
	broker.Unsubscribe(handler);
 
**Second one** is to pass a factory delegate in the constructor of the EventBroker. It can be hooked to an IoC container or a custom implementation. It will be called every time every time an event is published.

	var broker = new EventBroker(2, null, type => type == typeof(MyEvent) ? new[] { new MyEventHandler() } : null);	
Both ways can be used with the same instance of the EventBroker.

### Publishing Events

	broker.Publish(new MyEvent("Hello"));
 

### Event Filters

Event filters are defined by `IEventHandler<TEvent>.ShouldHandle` or `Func<TEvent, bool>`.  
If the filter returns `false` the handler will not be called. 