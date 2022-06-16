FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SphereTracing/SphereTracing.csproj", "SphereTracing/"]
RUN dotnet restore "SphereTracing/SphereTracing.csproj"
COPY . .
WORKDIR "/src/SphereTracing"
RUN dotnet build "SphereTracing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SphereTracing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SphereTracing.dll"]
