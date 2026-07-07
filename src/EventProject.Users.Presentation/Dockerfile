FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/EventProject.Users.Presentation/EventProject.Users.Presentation.csproj", "src/EventProject.Users.Presentation/"]
COPY ["src/EventProject.Users.Application/EventProject.Users.Application.csproj", "src/EventProject.Users.Application/"]
COPY ["src/EventProject.Users.Domain/EventProject.Users.Domain.csproj", "src/EventProject.Users.Domain/"]
COPY ["src/EventProject.Users.Infrastructure/EventProject.Users.Infrastructure.csproj", "src/EventProject.Users.Infrastructure/"]
COPY ["src/EventProject.Domain/EventProject.Domain.csproj", "src/EventProject.Domain/"]
RUN dotnet restore "src/EventProject.Users.Presentation/EventProject.Users.Presentation.csproj"
COPY . .
WORKDIR "/src/src/EventProject.Users.Presentation"
RUN dotnet build "./EventProject.Users.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventProject.Users.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventProject.Users.Presentation.dll"]
