# Overview

The Async package provides primitives for working with asynchronous code. Many projects within the `CreateAR.Common` namespace use the `IAsyncToken<T>` interface, so it is best to get well acquainted with it. This object may be considered either a very simplified Promise, or just a wrapper around a callback.

### AsyncToken

##### Callbacks

In many codebases, callbacks are used to manage asynchronous code:

```
_controller.Foo(() => {
	...
});
```

Callbacks are very nice, but lacking in a few areas. Most notably, callbacks provide a great method for subscribing, but provide no out-of-the-box method for unsubscribing. Consider a UI class:

```csharp
public Resource MyResource;

public void Open()
{
	_controller.Foo(Callback);
}

public void Close()
{
	MyResource.Destroy();
}

private void Callback()
{
	MyResource.Bar();
}
```

This is a simple example, but demonstrates the problem well. Here we have some asynchronous method `Foo` which is given a callback. That callback may be called at any future time. Now the problem: what if `Close` is called before `Callback`? We would have an error, because `Callback` is making an assumption on the order of execution. We could add a Boolean to tell us we're closed, but that's easy to forget. What we'd really like to be able to do is prevent the callback from ever being called.

##### Events

C# has a nice primitive called `event`, which allows for subscription and unsubscription. Thus, we can get around the earlier problem we had with callbacks:

```csharp
public void Open()
{
	_controller.OnEvent += Callback;
	_controller.Foo();
}

public void Close()
{
	_controller.OnEvent -= Callback;
}
```

This is great because it allows us to prevent the callback from ever being called, meaning we don't have to set a Boolean and check it in our handler. However, it has a whole different problem. Consider:

```csharp
public MyClass(Dependency dependency)
{
	dependency.OnReady += Initialize;
}
```

In the above example, we have an object that needs something to happen before it can initialize. However, what if `Dependency` has already called `OnReady`? This object will be waiting forever. The problem with events is that you may subscribe to late.

Thus, the need arises for a primitive that will fix these two problems.

##### IAsyncToken<T>

Our answer is `IAsyncToken<T>`. This object has acts much like an asynchronous counterpart to `try/catch/finally` blocks. At a high level, this object represents an asynchronous action being performed. This object accepts exactly one resolution. This is easiest to see by example:

```csharp
// execute an asynchronous operation
var token = _controller.Operation();

// called if the operation was a success
token.OnSuccess(value => ...);

// called if the operation was a failure
token.OnFailure(exception => ...);

// called regardless
token.OnFinally(_ => ...);
```

In this example, internally to `Operation`, the token is being resolved with either a success or a failure. It cannot be resolved with both. Once a token has been resolved, its resolution cannot be changed.

Additionally, `Operation` *may* be asynchronous, but it may not be. Consider the interface for a lazy cache-- the first call may be asynchronous, the second may be synchronous. For `IAsyncToken<T>`, it doesn't matter. Unlike an event, the callback will be called immediately if already resolved.

```csharp
var token = _controller.Operation();

// Operation() is already finished!

// Callback is called immediately
token.OnSuccess(value => ...);
```

For syntactic sugar familiar to Promise-users, you can also combine all of these calls in a chain:

```csharp
_controller
	.Foo()
	.OnSuccess(value => ...)
	.OnFailure(exception => ...)
	.OnFinally(token => ...);
```

Much like an event, multiple callbacks may be added:

```csharp
var token = _controller.OnSuccess(...);

...

token.OnSuccess(...);
```

These two callbacks will be called in order.

##### AsyncToken<T>

Much like the Promise/Deferred pattern, the methods for resolving a token and triggering the handlers are not present on the `IAsyncToken<T>` interface. Instead, these methods are present only on the `AsyncToken<T>` implementation. This allows us to very simply protect a token from being tampered with externally.

```csharp
public IAsyncToken<MyClass> Foo()
{
  var token = new AsyncToken<MyClass>();
  
  // do something
  InternalMethod(value => {
    if (value.IsValid()) {
      token.Succeed(value);
    } else {
      token.Fail(new Exception("Could not retrieve instance."));
    }
  });
  
  return token;
}
```

In this example, the `Foo` method needs to do some sort of asynchronous work. `InternalMethod` happens to take a callback, but it could listen for a load event, wait on a message from a thread, use a coroutine, or any other number of asynchronous structures. `Foo` then returns the token, but it is cast to the interface, `IAsyncToken<T>`. This means that the `Succeed` method is not visible to the object calling `Foo`, thus that object cannot resolve the token.

##### Resolving

Resolving tokens is a one time deal. A single token can be resolved only once. Any resolution after the first resolution is discarded.

```csharp
token
	.OnSuccess(value => ...)		// called
	.OnFailure(exception => ...);	// never called
	
...

token.Succeed(myValue);			// calls OnSuccess
token.Fail(new Exception());	// discarded
```

For loads of other edge cases, check out the [unit test suite](https://github.com/create-ar/commons-unity-async/blob/master/CreateAR.Commons.Unity.Async.Test/AsyncToken_Tests.cs).

##### Abort

Revisiting the problem we had with callbacks, let's do this again with tokens:

```csharp
private IAsyncToken<MyValue> _fooToken;

public void Foo()
{
	_fooToken = _controller
		.Foo()
		.OnSuccess(Callback);
}

public void Close()
{
	_fooToken.Abort();
{
```

After `Abort` is called, the token is dead. It cannot be resolved and it will call no future callbacks. In this case, if `Close` is called before the token calls `Callback`, then `Callback` will not be called at all. This gets around the limitations of both callbacks and events.

##### Exception Handling

If a callback assigned to a token throws an exception, what should happen? There are a number of ways to answer this. The easiest way is to let exceptions propogate naturally, i.e. do nothing at all.

```csharp
_token.OnSuccess(myValue => throw Exception());

_token.Succeed(3);	// throws exception
```

This can lead to inconsistencies and hard to track bugs, unfortunately.

```csharp
_token.OnSuccess(myValue => throw Exception());

...

_token.OnSuccess(myValue => Log.Info(this, "Received!"));
```

Our second callback would not be called at all! There is the potential these callbacks may be in separate classes. Instead, if a callback is added to a token, it is _guaranteed_ to be called. Internally, the token simply catches any exceptions that occur during execution, and throws them again after all callbacks have been called.

```csharp
_token.OnSuccess(myValue => throw Exception());

...

_token.OnSuccess(myValue => Log.Info(this, "Received!"));

...

_token.Succeed(value);	// the exception is thrown here, after both callbacks are called
```