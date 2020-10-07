#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TrafficLights.Auth/TrafficLights.Auth.csproj", "TrafficLights.Auth/"]
COPY ["TrafficLight.Model/TrafficLights.Model.csproj", "TrafficLight.Model/"]
COPY ["TrafficLight.Core/TrafficLights.Core.csproj", "TrafficLight.Core/"]
COPY ["TrafficLight.Data/TrafficLights.Data.csproj", "TrafficLight.Data/"]
RUN dotnet restore "TrafficLights.Auth/TrafficLights.Auth.csproj"
COPY . .
WORKDIR "/src/TrafficLights.Auth"
RUN dotnet build "TrafficLights.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrafficLights.Auth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficLights.Auth.dll"]
