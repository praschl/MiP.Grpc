# MiP.Grpc
Extensions for Grpc Services which simplifies separating implementations into command/query like implementation classes.

## The Problem
You have defined a big Grpc Service in protobuff in Visual Studio and now, you have to implement it.

You start by overriding method after method of the Service.ServiceBase, and the class is growing and growing, requiring more and more dependencies.

### Disadvantages of this approach
* The implementation of Service.ServiceBase gets very long when the service declares many methods
* Each time a method is invoked, all dependencies the service constructor requires are injected, even if they are not used by the method to be invoked.
* Implementation may become hard to test with all the dependencies

## The Solution
By using this extension, an automatically generated class deriving from the Service.ServiceBase forwards each call to it's own handler class.

So, each service method gets its own class where it's implemented.

### Advantages
* no huge class with lots of methods
* constructor is slim and doesn't get tens of dependencies injected
* each of your handler classes can be small
* and only get injected what is required for this method.

## Usage and example
Lets start our example with the greeter service that is generated when you create the project.

### Basics
Add the dependency to the nuget package
```powershell
Install-Package MiP.Grpc
```
Navigate to the Startup class and add
```csharp
services.AddDispatchedGrpcHandlers();
```
right under `services.AddGrpc();` in the `ConfigureServices()` method.
This will add required dependencies and (optionally) register handlers - we will come to that later.

Next, go to the `Configure()` method and replace
```csharp
endpoints.MapGrpcService<GreeterService>();
```
with
```csharp
endpoints.CompileAndMapGrpcServiceDispatcher(typeof(Greeter.GreeterBase));
```
or whatever base class you would normally derive your service of. This will create an implementation for the service and map it.

### Create Handlers
Let's continue with the Greeter service example, since it also comes with the template.

You can now implement a handler for the SayHello() method
```csharp
public class SayHelloHandler : IHandler<HelloRequest, HelloReply>
{
    public Task<HelloReply> RunAsync(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        }); ;
    }
}
```
The handler needs to be registered in the `ConfigureServices()` method, which can be done in two ways. The method is passed an action which gets a `IDispatcherMapBuilder` as parameter. This builder can then be used to add types manually or all implementations of `IHandler<,>` of an assembly automatically.

#### Add assembly types
```csharp
services.AddDispatchedGrpcHandlers(builder =>
  builder.Add(Assembly.GetExecutingAssembly()));
```
The method will scan for types implementing `IHandler<TRequest, TResponse>` and register them with a transient lifetime.

Per convention, a found implementation thats called "SayHelloHandler" will handle the "SayHello" method. However you can handle a method with another name, if you add the `HandlesAttribute` to the class:
```csharp
[Handles("SayHello")] // class will handle the "SayHello" method regardless of its name.
public class CanHandleSay : IHandler<CanHandleSomeRequest, CanHandleSomeResponse>
{
  // ...
}
```
Priority of finding the name is
1. `HandlesAttribute`s MethodName property
2. Name of class without "Handler"
3. Name of class

#### Add types manually
```csharp
services.AddDispatchedGrpcHandlers(builder => 
  {
    builder.Add(typeof(MyHandler), "MyMethod");
    // or
    builder.Add<MyHandler>("MyMethod");
  });
```
Passing the name is optional, priorities for chosing the name are
1. name Parameter
2. `HandlesAttribute`s MethodName property
3. Name of class without "Handler"
4. Name of class

And you can also combine them in any order:
```csharp
services.AddDispatchedGrpcHandlers(builder => 
{
  builder.Add(assembly1);
  builder.Add(typeof(Handler1), "method1");
  builder.Add<Handler3>("method3");
  builder.Add(typeof(Handler2), "method2");
  builder.Add(assembly2);
  builder.Add<Handler4>("method4");
});
```
although I would suggest to use assemblies first, and then override with custom implementations.

Types are added in the same order they are added, if there is already a Handler for a methodName that has the same `TRequest` and `TResponse` types, the older one is discarded and the new one takes its place. This makes it possible to add all types from assemblies, and then overwrite some specific handlers.

### Gotcha
It's important that there is only one call to `AddDispatchedGrpcHandlers`, but you can register as many assemblies and types as you want in this one call. Calling `AddDispatchedGrpcHandlers` would completely discard the first call.

Types registered directly on the service collection are not recognised as handlers and can currently not be used.

## Special Types
The `Google.Protobuf.WellKnownTypes.Empty` class can be used, if you do not require parameters or return types, but a handler still has to use the class as parameter and/or return type:
```csharp
public class IHandler<Empty, Empty>
{
  Task<Empty> RunAsync(Empty request) 
  {
    return Task.FromResult(new Empty());
  }
}
```

## Authorization
Authorization is supported by the use of the `Microsoft.AspNetCore.Authorization.AuthorizeAttribute` attribute.

That's the same attribute you would place on a method when implementing the service manually. Just place
the attribute on the handling method and it will be copied to the handled method when the code is generated.
If your handler implements more than just one `IHandler<,>` you can also place the attribute on the class to 
activate authorization for all methods of this handler class.

There is currently no way to add authorization to all methods of the service.

## Extension
Each method of the implemented service does one very simple thing, it calls 
```csharp
dispatcher.Dispatch<TRequest, TResponse>(request, context);
```
and returns the result. The dispatcher is an implementation of the IDispatcher interface:
```csharp
public interface IDispatcher
{
    Task<TResponse> Dispatch<TRequest, TResponse>(TRequest request, ServerCallContext context);
}
```
The default dispatcher is registered with transient lifetime scope when calling `AddDispatchedGrpcHandlers()` and it does nothing more than resolve `IHandler<TRequest, TResult` call it, and return the result.

You can always overwrite the default dispatcher by creating your own implementation (to do logging or whatever you want) and register it, just make sure, you overwrite the default dispatcher by calling
```csharp
services.AddTransient<IDispatcher, MyAwsomeDispatcher>();
```
after the call to `AddDispatchedGrpcHandlers()`.
