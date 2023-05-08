A simple Autofac module to automatically register delegates for certain service methods.

# Registering methods as service delegates

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

# Registering delegate factories

One can even take this idea one step further and provide methods the compose multiple service delegates and other to higher level delegates.

## Example

Given the following delegates:
```csharp
public delegate string GetGreetings(params string[] names);
public delegate string GetGreeting(string name);
```
And the follwing delegate factory:
```csharp
public class PersonServices {
    [DelegateFactory]
    public static GetGreetings Compose(GetGreeting getGreeting /* other dependencies */) {
        return names => string.Join(' ', names.Select(x => getGreeting(x)));
    }
}
```
Then one could resolve the 'GetGreetings' delegate and services required for composing the 'GetGreetings' delegate are automatically injected when the 'Compose' method is called:
```csharp
ContainerBuilder b = new();
b.RegisterServiceDelegateFactory<PersonServices>();
b.RegisterInstance(new GetGreeting(name => $"Hello {name}!"));
return c = b.Build();

Console.WriteLine(c.Resolve<GetGreetings>()("Max", "Pete")); // Writes "Hello Max! Hello Pete!"
```
</remarks>