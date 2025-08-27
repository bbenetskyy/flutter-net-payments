FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MicroApp.UsersService/MicroApp.UsersService.csproj", "MicroApp.UsersService/"]
RUN dotnet restore "MicroApp.UsersService/MicroApp.UsersService.csproj"
COPY . .
WORKDIR "/src/MicroApp.UsersService"
RUN dotnet build "./MicroApp.UsersService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MicroApp.UsersService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroApp.UsersService.dll"]
