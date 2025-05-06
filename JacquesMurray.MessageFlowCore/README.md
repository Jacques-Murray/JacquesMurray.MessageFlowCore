# JacquesMurray.MessageFlowCore

A simple mediator implementation in .NET with no dependencies. Supports request/response, commands, queries, notifications and events, both synchronous and async with intelligent dispatching via C# generic variance.

## Features

- Request/response pattern via `IRequest<TResponse>`
- Command pattern via `ICommand` (returns `Unit` representing void)
- Query pattern via `IQuery<TResult>`
- Notification pattern for 1:N message distribution via `INotification`
- Event pattern as a specialized notification type via `IEvent`
- Synchronous and asynchronous handlers
- Generic variance for intelligent dispatching
- No external dependencies (DI support is optional)
- Complete XML documentation

## Usage

### Installation

```shell
dotnet add package JacquesMurray.MessageFlowCore
```

### Basic Setup

```csharp
// Create a mediator using the builder
var mediator = MediatorBuilderExtensions.CreateMediator()
    .RegisterHandlersFromAssemblyContaining<Program>()
    .Build();
```

### With Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddMessageFlowFromAssemblyContaining<Program>();

// In your application code
public class MyService
{
    private readonly IMediator _mediator;

    public MyService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void DoWork()
    {
        // Send a command
        _mediator.Execute(new MyCommand());
        
        // Query for data
        var result = _mediator.Query(new MyQuery());
        
        // Publish an event
        _mediator.Raise(new MyEvent());
    }
}
```

### Defining Messages and Handlers

```csharp
// Command
public class MyCommand : ICommand { }

public class MyCommandHandler : IRequestHandler<MyCommand, Unit>
{
    public Unit Handle(MyCommand request)
    {
        // Command logic here
        return Unit.Value;
    }
}

// Query
public class MyQuery : IQuery<string> { }

public class MyQueryHandler : IRequestHandler<MyQuery, string>
{
    public string Handle(MyQuery request)
    {
        return "Hello, world!";
    }
}

// Event
public class MyEvent : IEvent { }

public class MyEventHandler : INotificationHandler<MyEvent>
{
    public void Handle(MyEvent notification)
    {
        // Event handling logic
    }
}
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.