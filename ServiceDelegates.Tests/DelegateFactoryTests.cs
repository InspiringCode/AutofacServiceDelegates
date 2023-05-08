using Autofac;

namespace ServiceDelegates.Tests;

public class DelegateFactoryTests : FeatureBase {
    [Scenario]
    internal void RegisterDelegateFactory(IContainer c, GetGreetings @delegate) {
        GIVEN["a delegate factory registration"] = () => {
            ContainerBuilder b = new();
            b.RegisterServiceDelegateFactory<PersonServices>();
            b.RegisterInstance(new GetGreeting(name => $"Hello {name}!"));
            b.RegisterInstance(new PersonRepository());
            return c = b.Build();
        };

        WHEN["resolving a delegate type created by a delegate factoy"] = () => @delegate = c.Resolve<GetGreetings>();
        THEN["the factory is called and all parameters are automatically resolved"] = () =>
            @delegate(1, 2).Should().Be("Hello Person 1! Hello Person 2!");
    }


    internal class PersonServices {
        [DelegateFactory]
        public static GetGreetings Compose(GetGreeting getGreeting, PersonRepository persons) {
            return ids => {
                IEnumerable<string> greetings = persons
                    .GetNames(ids)
                    .Select(name => getGreeting(name));
                return string.Join(' ', greetings);
            };
        }
    }

    internal delegate string GetGreetings(params int[] ids);

    internal class PersonRepository {
        public string[] GetNames(int[] ids) => ids
            .Select(id => $"Person {id}")
            .ToArray();
    }

    internal delegate string GetGreeting(string name);
}
