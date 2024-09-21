FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TelegramQueueBot/TelegramQueueBot.csproj", "TelegramQueueBot/"]
COPY ["Data/Data.csproj", "Data/"]
RUN dotnet restore "./TelegramQueueBot/TelegramQueueBot.csproj"
COPY . .
WORKDIR "/src/TelegramQueueBot"
RUN dotnet build "./TelegramQueueBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TelegramQueueBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TelegramQueueBot.dll"]