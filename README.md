A simple Autofac module to automatically register delegates for certain service methods.

# Example

Given the following delegate type and service:
```csharp
public delegate string GetGreeting(string name);

public class GreetingService {
    [RegisterDelegate<GetGreeting>]
    public string GetGreeting(string name)
        => $"Hello {name}!";
}
```

The GetGreeting delegate is automatically registered and can be resolved:
```csharp
ContainerBuilder b = new();
b.RegisterModule<ServiceDelegateModule>();
b.RegisterType<GreetingService>();
IContainer c = b.Build();
GetGreeting g = c.Resolve<GetGreeting>();
Console.WriteLine(g("Max")) // prints "Hello Max!";
```