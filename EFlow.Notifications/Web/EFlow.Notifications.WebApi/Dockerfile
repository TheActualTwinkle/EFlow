FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet publish "EFlow.Notifications/Web/EFlow.Notifications.WebApi/EFlow.Notifications.WebApi.csproj" \
	-c $BUILD_CONFIGURATION \
	-o /app/publish \
	/p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EFlow.Notifications.WebApi.dll"]
