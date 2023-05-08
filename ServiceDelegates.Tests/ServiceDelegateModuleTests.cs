using Autofac;

namespace ServiceDelegates.Tests;

public class ServiceDelegateTests : FeatureBase {
    [Scenario]
    internal void ResolveDelegate(IContainer c, GetGreeting @delegate) {
        GIVEN["a container with a registered service type"] = () => {
            ContainerBuilder b = new();
            b.RegisterModule<ServiceDelegateModule>();
            b.RegisterType<GreetingService>();
            return c = b.Build();
        };
        WHEN["resolving a service delegate"] = () => @delegate = c.Resolve<GetGreeting>();
        THEN["the service delegate invokes the service"] = () => @delegate("Max").Should().Be("Hello Max!");
    }

    internal delegate string GetGreeting(string name);

    internal class GreetingService {
        [RegisterDelegate<GetGreeting>]
        public string GetGreeting(string name)
            => $"Hello {name}!";
    }
}