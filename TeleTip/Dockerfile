FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TeleTip.Domain/TeleTip.Domain.csproj", "src/TeleTip.Domain/"]
COPY ["src/TeleTip.Application/TeleTip.Application.csproj", "src/TeleTip.Application/"]
COPY ["src/TeleTip.Infrastructure/TeleTip.Infrastructure.csproj", "src/TeleTip.Infrastructure/"]
COPY ["src/TeleTip.WebAPI/TeleTip.WebAPI.csproj", "src/TeleTip.WebAPI/"]
RUN dotnet restore "src/TeleTip.WebAPI/TeleTip.WebAPI.csproj"

COPY . .
RUN dotnet publish "src/TeleTip.WebAPI/TeleTip.WebAPI.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
RUN mkdir -p /app/logs && chown -R $APP_UID:$APP_UID /app
USER $APP_UID
ENTRYPOINT ["dotnet", "TeleTip.WebAPI.dll"]
