FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MyProject.WebAPI/MyProject.WebAPI.csproj", "src/MyProject.WebAPI/"]
COPY ["src/MyProject.Application/MyProject.Application.csproj", "src/MyProject.Application/"]
COPY ["src/MyProject.Infrastructure/MyProject.Infrastructure.csproj", "src/MyProject.Infrastructure/"]
COPY ["src/MyProject.Domain/MyProject.Domain.csproj", "src/MyProject.Domain/"]
RUN dotnet restore "src/MyProject.WebAPI/MyProject.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/MyProject.WebAPI"
RUN dotnet build "MyProject.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyProject.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyProject.WebAPI.dll"]
