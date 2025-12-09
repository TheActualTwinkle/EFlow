FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EFlow/Web/EFlow.WebApi/EFlow.WebApi.csproj", "EFlow/Web/EFlow.WebApi/"]
COPY ["EFlow/Infrastructure/EFlow.Persistence/EFlow.Persistence.csproj", "EFlow/Infrastructure/EFlow.Persistence/"]
COPY ["EFlow/Core/EFlow.Domain/EFlow.Domain.csproj", "EFlow/Core/EFlow.Domain/"]
COPY ["EFlow/Core/EFlow.Application/EFlow.Application.csproj", "EFlow/Core/EFlow.Application/"]
COPY ["EFlow.Common/EFlow.Common.csproj", "EFlow.Common/"]
COPY ["EFlow/Infrastructure/EFlow.Messaging/EFlow.Messaging.csproj", "EFlow/Infrastructure/EFlow.Messaging/"]
RUN dotnet restore "EFlow/Web/EFlow.WebApi/EFlow.WebApi.csproj"
COPY . .
WORKDIR "/src/EFlow/Web/EFlow.WebApi"
RUN dotnet build "./EFlow.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EFlow.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EFlow.WebApi.dll"]
