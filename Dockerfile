
FROM mcr.microsoft.com/dotnet/runtime:7.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0  AS build
WORKDIR /src
COPY . .
WORKDIR "/src/src/ProjectIvy.Jobs.DistanceCovered"
RUN dotnet build "ProjectIvy.Jobs.DistanceCovered.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectIvy.Jobs.DistanceCovered.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectIvy.Jobs.DistanceCovered.dll"]