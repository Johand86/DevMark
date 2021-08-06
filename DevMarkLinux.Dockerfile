# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS installer

RUN curl -sL https://deb.nodesource.com/setup_14.x -o setup_14.sh
RUN sh ./setup_14.sh

RUN curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list

RUN apt update
RUN apt install -y curl git nodejs yarn gcc g++ make

FROM installer AS config
        
RUN dotnet tool install --tool-path /DevMark --version 1.0.0 DevMark

FROM config AS final

ENV NUGET_PACKAGES=/root/.nuget/fallbackpackages

WORKDIR /DevMark

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';$ProgressPreference='silentlyContinue';"]

VOLUME /dmshare

ENTRYPOINT ["./devmark"]
CMD [""]
