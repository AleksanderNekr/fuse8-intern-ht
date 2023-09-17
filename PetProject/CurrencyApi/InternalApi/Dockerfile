FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS prepare-restore-files
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset --version 0.3.2
WORKDIR app/src/
COPY . .
RUN ls
RUN dotnet subset restore "InternalApi/InternalApi.csproj" --root-directory /app/src --output restore_subset/
RUN ls

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
RUN ls
COPY --from=prepare-restore-files app/src/restore_subset .
RUN ls
RUN dotnet restore "InternalApi/InternalApi.csproj"
COPY . .
RUN dotnet build "InternalApi/InternalApi.csproj" -c Release -o /app/build
# For healthchecks
RUN apt-get update && apt-get install -y curl

FROM build AS publish
RUN dotnet publish "InternalApi/InternalApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternalApi.dll"]
