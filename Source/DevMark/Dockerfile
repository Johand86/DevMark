# syntax=docker/dockerfile:1
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS installer

RUN apt update
RUN apt install -y curl htop git nodejs npm

FROM installer AS base
WORKDIR /app