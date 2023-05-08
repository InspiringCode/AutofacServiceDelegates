using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using System.Reflection;

namespace ServiceDelegates;

/// <summary>
/// Automatically registers service delegates for all methods of registered types that have been
/// decorated with <see cref="RegisterDelegateAttribute{T}"/>.
/// </summary>
/// <remarks>
/// Given the following delegate type and service:
/// <code><![CDATA[
/// internal delegate string GetGreeting(string name);
/// 
/// internal class GreetingService {
///     [RegisterDelegate<GetGreeting>]
///     public string GetGreeting(string name)
///         => $"Hello {name}!";
/// }
/// ]]></code>
/// 
/// The GetGreeting delegate is automatically registered and can be resolved:
/// <code><![CDATA[
/// ContainerBuilder b = new();
/// b.RegisterModule<ServiceDelegateModule>();
/// b.RegisterType<GreetingService>();
/// IContainer c = b.Build();
/// GetGreeting g = c.Resolve<GetGreeting>();
/// Console.WriteLine(d("Max")) // prints "Hello Max!";
/// ]]></code>
/// </remarks>
public class ServiceDelegateModule : Autofac.Module {
    protected override void AttachToComponentRegistration(IComponentRegistryBuilder cr, IComponentRegistration reg) {
        Type serviceType = reg.Activator.LimitType;

        IEnumerable<DelegateFactory> delegates = serviceType
            .GetMethods()
            .SelectMany(m => m
                .GetCustomAttributes<RegisterDelegateAttribute>()
                .Select(attr => new DelegateFactory(m, attr.DelegateType,serviceType)));

        foreach (DelegateFactory d in delegates) {
            cr.Register(RegistrationBuilder
                .ForDelegate(d.DelegateType, d.GetDelegateFactory())
                .CreateRegistration());
        }
    }

    private record DelegateFactory(MethodInfo Method, Type DelegateType, Type ServiceType) {
        public Func<IComponentContext, IEnumerable<Parameter>, Delegate> GetDelegateFactory() {
            return (ctx, ps) => CreateServiceDelegate(ctx);
        }

        private Delegate CreateServiceDelegate(IComponentContext ctx) { 
            object targetService = ctx.Resolve(ServiceType);
            return Method.CreateDelegate(DelegateType, targetService);
        }
    }
}
