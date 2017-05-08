FROM ubuntu:trusty

RUN apt-get update
RUN apt-get install -y apt-transport-https curl

# Configure Mono 5.0 Beta 2 (5.0.0.94) (on Trusty) http://www.mono-project.com/download/beta/
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN echo "deb http://download.mono-project.com/repo/ubuntu beta-trusty main" | tee /etc/apt/sources.list.d/mono-official-beta.list

# Configure dotnet-dev 1.0.3
RUN sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893

# Install mono and dotnet
RUN apt-get update
RUN apt-get install -y mono-complete dotnet-dev-1.0.3

# Warm-up package cache
WORKDIR /tmp/root
COPY Dummy.csproj /tmp/root/Dummy.csproj
RUN dotnet restore

# Could be installed sooner, but I don't want to push a new 1GB image...
RUN apt-get install -y git

# Prepare for the AOT test
RUN mono --aot=full /usr/lib/mono/4.5/mscorlib.dll
RUN mono --aot=full /usr/lib/mono/gac/System/4.0.0.0__b77a5c561934e089/System.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Xml/4.0.0.0__b77a5c561934e089/System.Xml.dll
RUN mono --aot=full /usr/lib/mono/gac/Mono.Security/4.0.0.0__0738eb9f132ed756/Mono.Security.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Configuration/4.0.0.0__b03f5f7f11d50a3a/System.Configuration.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Security/4.0.0.0__b03f5f7f11d50a3a/System.Security.dll
RUN mono --aot=full /usr/lib/mono/gac/System.Core/4.0.0.0__b77a5c561934e089/System.Core.dll
RUN mono --aot=full /usr/lib/mono/gac/Mono.Posix/4.0.0.0__0738eb9f132ed756/Mono.Posix.dll

# # Add local user 
# ARG groupId=1001
# RUN groupadd -g $groupId build

# ARG userId=1001
# RUN useradd -u $userId -g $groupId -ms /bin/bash build
# RUN ln -s /root/.nuget /home/build/.nuget
# USER $userId

CMD [ "bash" ]
