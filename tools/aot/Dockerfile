FROM mono:6.12-slim

RUN mono --aot=full /usr/lib/mono/4.5/mscorlib.dll && \
    mono --aot=full /usr/lib/mono/gac/System/4.0.0.0__b77a5c561934e089/System.dll && \
    mono --aot=full /usr/lib/mono/gac/System.Xml/4.0.0.0__b77a5c561934e089/System.Xml.dll && \
    mono --aot=full /usr/lib/mono/gac/Mono.Security/4.0.0.0__0738eb9f132ed756/Mono.Security.dll && \
    mono --aot=full /usr/lib/mono/gac/System.Configuration/4.0.0.0__b03f5f7f11d50a3a/System.Configuration.dll && \
    mono --aot=full /usr/lib/mono/gac/System.Security/4.0.0.0__b03f5f7f11d50a3a/System.Security.dll && \
    mono --aot=full /usr/lib/mono/gac/System.Core/4.0.0.0__b77a5c561934e089/System.Core.dll && \
    mono --aot=full /usr/lib/mono/gac/System.Numerics/4.0.0.0__b77a5c561934e089/System.Numerics.dll
