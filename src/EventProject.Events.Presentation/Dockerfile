FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/EventProject.Events.Presentation/EventProject.Events.Presentation.csproj", "src/EventProject.Events.Presentation/"]
COPY ["src/EventProject.Events.Application/EventProject.Events.Application.csproj", "src/EventProject.Events.Application/"]
COPY ["src/EventProject.Events.Domain/EventProject.Events.Domain.csproj", "src/EventProject.Events.Domain/"]
COPY ["src/EventProject.Events.Infrastructure/EventProject.Events.Infrastructure.csproj", "src/EventProject.Events.Infrastructure/"]
COPY ["src/EventProject.Shared/EventProject.Shared.csproj", "src/EventProject.Shared/"]
RUN dotnet restore "src/EventProject.Events.Presentation/EventProject.Events.Presentation.csproj"
COPY . .
WORKDIR "/src/src/EventProject.Events.Presentation"
RUN dotnet build "./EventProject.Events.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventProject.Events.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventProject.Events.Presentation.dll"]
