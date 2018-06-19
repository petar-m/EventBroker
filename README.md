# EventBroker
[![NuGet](https://img.shields.io/nuget/v/M.EventBroker.svg)](https://www.nuget.org/packages/M.EventBroker)  

EventBroker is an implementation of publish/subscribe pattern.  

## Specifics

- in-memory, in-process scope
- publishing is 'Fire and Forget' style  
- event handlers are runned on background threads
- can use different strategies for running event handlers
- provides different ways for subscribing that can be used side by side
- subscriptions are based on the event type
- event handlers can provide filter for the events they want to handle  
- events don't need to implement specific interface

## Usage

check out the wiki

## Breaking Changes  

version 2.x is not backward compatible with [version 1.x](https://github.com/petar-m/EventBroker/blob/master/README_v1.md)    

 - `IEventHandler` has new method `OnError`  
 - `EventBroker` now takes an `IEventHandlerRunner` parameter instead of thread count  
 - instead of factory delegate `EventBroker` now has optional `IEventHandlerFactory` parameter  
 - global error handling in `EventBroker` is now moved to `IEventHandler.OnError`
