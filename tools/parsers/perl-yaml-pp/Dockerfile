FROM ubuntu:20.04

RUN apt-get update && apt-get install -y libyaml-pp-perl
COPY parse.pl /app/
WORKDIR /app

ENTRYPOINT [ "/usr/bin/perl", "-w", "/app/parse.pl" ]
