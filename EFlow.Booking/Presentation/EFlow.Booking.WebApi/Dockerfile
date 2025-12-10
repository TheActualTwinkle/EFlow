FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj", "EFlow.Booking/Presentation/EFlow.Booking.WebApi/"]
COPY ["EFlow.Booking/Core/EFlow.Booking.Application/EFlow.Booking.Application.csproj", "EFlow.Booking/Core/EFlow.Booking.Application/"]
COPY ["EFlow.Common/EFlow.Common.csproj", "EFlow.Common/"]
COPY ["EFlow.Booking/Core/EFlow.Booking.Domain/EFlow.Booking.Domain.csproj", "EFlow.Booking/Core/EFlow.Booking.Domain/"]
COPY ["EFlow.Booking/Infrastructure/EFlow.Booking.Persistence/EFlow.Booking.Persistence.csproj", "EFlow.Booking/Infrastructure/EFlow.Booking.Persistence/"]
COPY ["EFlow.Booking/Infrastructure/EFlow.Booking.Messaging/EFlow.Booking.Messaging.csproj", "EFlow.Booking/Infrastructure/EFlow.Booking.Messaging/"]
RUN dotnet restore "EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj"
COPY . .
WORKDIR "/src/EFlow.Booking/Presentation/EFlow.Booking.WebApi"
RUN dotnet build "./EFlow.Booking.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EFlow.Booking.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EFlow.Booking.WebApi.dll"]
