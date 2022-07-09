FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
RUN apt update && \
    apt install -y \
        apt-transport-https \
        gnupg \
        ca-certificates \
        curl
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
# yes, we're using the bullseye (debian 11) image, however mono only has buster, but it still works for bullseye
RUN echo "deb https://download.mono-project.com/repo/debian stable-buster main" > /etc/apt/sources.list.d/mono-official-stable.list
RUN apt update
RUN apt install -y mono-complete

# install dot net 3.1 for running netstandard2.1 tests
RUN apt install -y wget
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt update
RUN apt install -y dotnet-sdk-3.1

FROM builder AS build
WORKDIR /src

COPY YamlDotNet.sln YamlDotNet.sln
COPY YamlDotNet/YamlDotNet.csproj YamlDotNet/YamlDotNet.csproj
COPY YamlDotNet.AotTest/YamlDotNet.AotTest.csproj YamlDotNet.AotTest/YamlDotNet.AotTest.csproj
COPY YamlDotNet.Benchmark/YamlDotNet.Benchmark.csproj YamlDotNet.Benchmark/YamlDotNet.Benchmark.csproj
COPY YamlDotNet.Samples/YamlDotNet.Samples.csproj YamlDotNet.Samples/YamlDotNet.Samples.csproj
COPY YamlDotNet.Test/YamlDotNet.Test.csproj YamlDotNet.Test/YamlDotNet.Test.csproj

RUN dotnet restore YamlDotNet.sln

COPY . .

RUN dotnet build -c Release --framework net35 YamlDotNet/YamlDotNet.csproj -o /output/net35
RUN dotnet build -c Release --framework net47 YamlDotNet/YamlDotNet.csproj -o /output/net47
RUN dotnet build -c Release --framework netstandard2.1 YamlDotNet/YamlDotNet.csproj -o /output/netstandard2.1

RUN dotnet test YamlDotNet.sln

FROM alpine
VOLUME /output
COPY --from=build /output /libraries
CMD [ "cp", "-r", "/libraries", "/output" ]