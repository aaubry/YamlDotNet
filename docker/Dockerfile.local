FROM aaubry/yamldotnet

# Add local user 
ARG groupId=1001
RUN groupadd -g $groupId build

ARG userId=1001
RUN useradd -u $userId -g $groupId -ms /bin/bash build
RUN mv /root/.nuget /home/build/.nuget
RUN chown -R build:build /home/build/.nuget
USER $userId

CMD [ "bash" ]
