FROM ubuntu:20.04

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" apt-get install -y cabal-install git wget apt-transport-https software-properties-common python3-pip
RUN cabal update

WORKDIR /opt/yamlreference
RUN git clone https://github.com/orenbenkiki/yamlreference.git .
RUN cabal install --only-dependencies
RUN cabal configure
RUN cabal build

WORKDIR /tmp
RUN wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN add-apt-repository universe
RUN apt-get install -y powershell

RUN pip3 install rundoc

RUN apt-get clean

WORKDIR /app
COPY check-syntax.ps1 /app

ENTRYPOINT [ "/opt/microsoft/powershell/7/pwsh", "/app/check-syntax.ps1" ]
