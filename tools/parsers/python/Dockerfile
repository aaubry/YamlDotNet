FROM ubuntu:20.04

RUN apt-get update && apt-get install -y python3 python3-yaml
WORKDIR /app

ENTRYPOINT [ "/usr/bin/python3", "-c", "import sys, yaml, json; print(json.dumps(yaml.load(sys.stdin.read(), Loader=yaml.SafeLoader)))" ]
