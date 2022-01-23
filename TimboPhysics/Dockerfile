FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TimboPhysics/TimboPhysics.csproj", "TimboPhysics/"]
RUN dotnet restore "TimboPhysics/TimboPhysics.csproj"
COPY . .
WORKDIR "/src/TimboPhysics"
RUN dotnet build "TimboPhysics.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimboPhysics.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimboPhysics.dll"]
