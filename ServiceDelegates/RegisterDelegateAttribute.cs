namespace ServiceDelegates;

public abstract class RegisterDelegateAttribute : Attribute {
    public Type DelegateType { get; }

    protected RegisterDelegateAttribute(Type delegateType)
        => DelegateType = delegateType;
}

/// <summary>
/// Registers a delegate of type <typeparamref name="TDelegate"/> for the decorated method. See 
/// <see cref="ServiceDelegateModule"/> for a full example.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterDelegateAttribute<TDelegate> : RegisterDelegateAttribute where TDelegate : Delegate {
    public RegisterDelegateAttribute() : base(typeof(TDelegate)) { }
}
