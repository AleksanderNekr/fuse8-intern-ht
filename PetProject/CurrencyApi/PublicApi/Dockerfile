FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS prepare-restore-files
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset --version 0.3.2
WORKDIR src/
COPY . .
RUN dotnet subset restore "PublicApi/PublicApi.csproj" --root-directory /src --output restore_subset/

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY --from=prepare-restore-files "/src/restore_subset" .
RUN dotnet restore "PublicApi/PublicApi.csproj"
COPY . .
WORKDIR "/src/PublicApi"
RUN dotnet build "PublicApi.csproj" -c Release -o /app/build
# For healthchecks
RUN apt-get update && apt-get install -y curl

FROM build AS publish
RUN dotnet publish "PublicApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PublicApi.dll"]
