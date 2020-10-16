#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TrafficLights.Api/TrafficLights.Api.csproj", "TrafficLights.Api/"]
COPY ["TrafficLight.Model/TrafficLights.Model.csproj", "TrafficLight.Model/"]
COPY ["TrafficLight.Core/TrafficLights.Core.csproj", "TrafficLight.Core/"]
COPY ["TrafficLight.Data/TrafficLights.Data.csproj", "TrafficLight.Data/"]
COPY ["TrafficLights.WorkerService/TrafficLights.WorkerService.csproj", "TrafficLights.WorkerService/"]
RUN dotnet restore "TrafficLights.Api/TrafficLights.Api.csproj"
COPY . .
WORKDIR "/src/TrafficLights.Api"
RUN dotnet build "TrafficLights.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrafficLights.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrafficLights.Api.dll"]
