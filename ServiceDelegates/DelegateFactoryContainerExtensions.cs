using Autofac;
using Autofac.Builder;
using Autofac.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceDelegates;

public static class DelegateFactoryContainerExtensions {
    private static readonly MethodInfo ResolveMethodInfo = typeof(ResolutionExtensions).GetMethod(
        "Resolve",
        new[] { typeof(IComponentContext), typeof(Type) });

    /// <summary>
    /// Registers all methods of <typeparamref name="T"/> that are decorated with the <see 
    /// cref="DelegateFactoryAttribute"/> delegate factories.
    /// </summary>
    /// <remarks>
    /// Given the following delegates:
    /// <code>
    /// public delegate string GetGreetings(params string[] names);
    /// public delegate string GetGreeting(string name);
    /// </code>
    /// And the follwing delegate factory:
    /// <code>
    /// public class PersonServices {
    ///	    [DelegateFactory]
    ///     public static GetGreetings Compose(GetGreeting getGreeting /* other dependencies */) {
    ///         return names => string.Join(' ', names.Select(x => getGreeting(x)));
    ///     }
    /// }
    /// </code>
    /// Then one could resolve the 'GetGreetings' delegate and services required for composing the 
    /// 'GetGreetings' delegate are automatically injected when the 'Compose' method is called:
    /// <code><![CDATA[
    /// ContainerBuilder b = new();
    /// b.RegisterServiceDelegateFactory<PersonServices>();
    /// b.RegisterInstance(new GetGreeting(name => $"Hello {name}!"));
    /// return c = b.Build();
    /// 
    /// Console.WriteLine(c.Resolve<GetGreetings>()("Max", "Pete")); // Writes "Hello Max! Hello Pete!"
    /// ]]></code>
    /// </remarks>
    public static void RegisterServiceDelegateFactory<T>(this ContainerBuilder b) {
        IEnumerable<MethodInfo> factoryMethods = typeof(T)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.GetCustomAttribute<DelegateFactoryAttribute>() != null);

        foreach (MethodInfo m in factoryMethods) {
            b.RegisterComponent(
                RegistrationBuilder
                    .ForDelegate(m.ReturnType, CreateDelegateFactory(m))
                    .CreateRegistration());
        }
    }

    private static Func<IComponentContext, IEnumerable<Parameter>, object> CreateDelegateFactory(MethodInfo method) {
        ParameterExpression contextParam = Expression.Parameter(typeof(IComponentContext));

        IEnumerable<Expression> parameterResolutionCalls = method
            .GetParameters()
            .Select(p =>
                Expression.Convert(
                    Expression.Call(
                        ResolveMethodInfo,
                        contextParam,
                        Expression.Constant(p.ParameterType)),
                    p.ParameterType));

        return Expression
            .Lambda<Func<IComponentContext, IEnumerable<Parameter>, object>>(
                body: Expression.Call(method, parameterResolutionCalls),
                contextParam,
                Expression.Parameter(typeof(IEnumerable<Parameter>)))
            .Compile();
    }
}
