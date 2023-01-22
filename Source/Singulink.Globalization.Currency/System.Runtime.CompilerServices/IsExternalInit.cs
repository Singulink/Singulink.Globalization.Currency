using System.ComponentModel;

namespace System.Runtime.CompilerServices;

#if !NET

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }

#endif