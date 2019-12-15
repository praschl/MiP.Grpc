# MiP.Grpc
Extensions for Grpc Services

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
Add the dependency to the nuget package (TBD)
```
Install-Package MiP.Grpc
```
Navigate to the Startup class and add
```
services.AddDispatchedGrpcHandlers();
```
right under `services.AddGrpc();` in the `ConfigureServices()` method.
This will add required dependencies and (optionally) register handlers - we will come to that later.

Next, go to the `Configure()` method and replace
```
endpoints.MapGrpcService<GreeterService>();
```
with
```
endpoints.CompileAndMapGrpcServiceDispatcher(typeof(Greeter.GreeterBase));
```
or whatever base class you would normally derive your service of. This will create an implementation for the service and map it.

### Create Handlers
Let's continue with the Greeter service example, since it also comes with the template.

You can not implement a handler for the SayHello() method
```
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
The handler needs to be registered in the `ConfigureServices()` method, which can be done in two ways, you can either do it explicitly, by calling
```
services.AddTransient<IHandler<HelloRequest, HelloReply>>();
```
or by passing the assembly where all your handlers are implemented to the `AddDispatchedGrpcHandlers()` call
```
services.AddDispatchedGrpcHandlers(new[] { Assembly.GetExecutingAssembly() });
```
In this example, the handlers would have to be in the same assembly as the grpc service host. The method will scan for types implementing `IHandler<TRequest, TResponse>` and register them with a transient lifetime.

### Gotcha
Currently it's not possible to have two service methods share the `TRequest` and `TResponse` type, since net.core does not directly support registering named services. 

If a service has two methods sharing both `TRequest` and `TResponse`, both would be handled by the same handler.

This also means there is futile to implement two handlers for the same `TRequest` and `TResponse` as only one of them will ever be called.

## Extension
Each method of the implemented service does one very simple thing, it calls 
```
dispatcher.Dispatch<TRequest, TResponse>(request, context);
```
and returns the result. The dispatcher is an implementation of the IDispatcher interface:
```
public interface IDispatcher
{
    Task<TResponse> Dispatch<TRequest, TResponse>(TRequest request, ServerCallContext context);
}
```
The default dispatcher is registered with transient lifetime scope when calling `AddDispatchedGrpcHandlers()` and it does nothing more than resolve `IHandler<TRequest, TResult` call it, and return the result.

You can always overwrite the default dispatcher by creating your own implementation (to do logging or whatever you want) and register it, just make sure, you overwrite the default dispatcher by calling
```
services.AddTransient<IDispatcher, MyAwsomeDispatcher>();
```
after the call to `AddDispatchedGrpcHandlers()`.
