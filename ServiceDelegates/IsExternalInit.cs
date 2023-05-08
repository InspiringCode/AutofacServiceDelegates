#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

// this class is needed for init properties.
// It was added to .NET 5.0 but for earlier versions we need to specify it manually
internal static class IsExternalInit { }
#endif