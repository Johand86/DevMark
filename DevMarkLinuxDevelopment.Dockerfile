# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS installer

RUN curl -sL https://deb.nodesource.com/setup_14.x -o setup_14.sh
RUN sh ./setup_14.sh

RUN curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list

RUN apt update
RUN apt install -y curl git nodejs yarn gcc g++ make

FROM installer AS base
WORKDIR /app

FROM base AS build

WORKDIR /src/
COPY ["DevMark.sln", "LICENSE.txt", "DevMarkWin.Dockerfile", "DevMarkLinux.Dockerfile", "DevMarkWinDevelopment.Dockerfile", "DevMarkLinuxDevelopment.Dockerfile", "./"]
COPY ["Source", "Source"]
RUN dotnet restore Source/DevMark/DevMark.csproj && \
    dotnet build Source/DevMark/DevMark.csproj -c Release -o /DevMark

FROM build AS dev

ENV NUGET_PACKAGES=/root/.nuget/fallbackpackages

WORKDIR /DevMark
COPY --from=build /DevMark .

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';$ProgressPreference='silentlyContinue';"]

VOLUME /dmshare

ENTRYPOINT ["./DevMark"]
CMD [""]
