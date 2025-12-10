FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EFlow.Notifications/Web/EFlow.Notifications.WebApi/EFlow.Notifications.WebApi.csproj", "EFlow.Notifications/Web/EFlow.Notifications.WebApi/"]
COPY ["EFlow.Notifications/Infrastructure/EFlow.Notifications.Messaging/EFlow.Notifications.Messaging.csproj", "EFlow.Notifications/Infrastructure/EFlow.Notifications.Messaging/"]
COPY ["EFlow.Common/EFlow.Common.csproj", "EFlow.Common/"]
COPY ["EFlow.Notifications/Core/EFlow.Notifications.Services/EFlow.Notifications.Services.csproj", "EFlow.Notifications/Core/EFlow.Notifications.Services/"]
RUN dotnet restore "EFlow.Notifications/Web/EFlow.Notifications.WebApi/EFlow.Notifications.WebApi.csproj"
COPY . .
WORKDIR "/src/EFlow.Notifications/Web/EFlow.Notifications.WebApi"
RUN dotnet build "./EFlow.Notifications.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EFlow.Notifications.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EFlow.Notifications.WebApi.dll"]
