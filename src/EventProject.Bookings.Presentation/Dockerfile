FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/EventProject.Bookings.Presentation/EventProject.Bookings.Presentation.csproj", "src/EventProject.Bookings.Presentation/"]
COPY ["src/EventProject.Bookings.Application/EventProject.Bookings.Application.csproj", "src/EventProject.Bookings.Application/"]
COPY ["src/EventProject.Bookings.Domain/EventProject.Bookings.Domain.csproj", "src/EventProject.Bookings.Domain/"]
COPY ["src/EventProject.Bookings.Infrastructure/EventProject.Bookings.Infrastructure.csproj", "src/EventProject.Bookings.Infrastructure/"]
COPY ["src/EventProject.Shared/EventProject.Shared.csproj", "src/EventProject.Shared/"]
RUN dotnet restore "src/EventProject.Bookings.Presentation/EventProject.Bookings.Presentation.csproj"
COPY . .
WORKDIR "/src/src/EventProject.Bookings.Presentation"
RUN dotnet build "./EventProject.Bookings.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventProject.Bookings.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventProject.Bookings.Presentation.dll"]
