FROM ubuntu:18.04

RUN apt-get update
RUN apt-get install -y gnupg ca-certificates apt-transport-https wget curl git libgit2-26

# Configure Mono
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list

# Configure dotnet
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb

# Install mono and dotnet
ARG NETSDK=3.0
RUN apt-get update
RUN apt-get install -y mono-complete dotnet-sdk-${NETSDK}

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Prepare for the AOT test
RUN mono --aot=full /usr/lib/mono/4.5/mscorlib.dll
RUN mono --aot=full /usr/lib/mono/gac/System/4.0.0.0__b77a5c561934e089/System.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Xml/4.0.0.0__b77a5c561934e089/System.Xml.dll
RUN mono --aot=full /usr/lib/mono/gac/Mono.Security/4.0.0.0__0738eb9f132ed756/Mono.Security.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Configuration/4.0.0.0__b03f5f7f11d50a3a/System.Configuration.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Security/4.0.0.0__b03f5f7f11d50a3a/System.Security.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Core/4.0.0.0__b77a5c561934e089/System.Core.dll
RUN mono --aot=full /usr/lib/mono/gac/Mono.Posix/4.0.0.0__0738eb9f132ed756/Mono.Posix.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Numerics/4.0.0.0__b77a5c561934e089/System.Numerics.dll

CMD [ "bash" ]
