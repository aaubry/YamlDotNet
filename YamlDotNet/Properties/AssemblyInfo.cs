using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

#if !SIGNED
#if PORTABLE
[assembly: InternalsVisibleTo("YamlDotNet.Test.Portable")]
#else
[assembly: InternalsVisibleTo("YamlDotNet.Test")]
#endif
#endif