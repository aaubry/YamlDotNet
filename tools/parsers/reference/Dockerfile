FROM ubuntu:20.04

WORKDIR /app

RUN apt-get update && apt-get install -y cabal-install git
RUN cabal update
RUN git clone https://github.com/orenbenkiki/yamlreference.git .
RUN cabal install --only-dependencies
RUN cabal configure
RUN cabal build
COPY run.sh /app/

ENTRYPOINT [ "/usr/bin/bash", "/app/run.sh" ]
