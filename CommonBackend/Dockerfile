FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

# Копируем собранное приложение
COPY --from=build /app/publish .

# Копируем конфигурацию (если нужно)
COPY appsettings.json ./
COPY appsettings.Docker.json ./

COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

ENV ASPNETCORE_ENVIRONMENT=Docker
ENTRYPOINT ["/wait-for-it.sh", "postgres:5432", "-t", "60", "--", "/wait-for-it.sh", "rabbitmq:5672", "-t", "60", "--", "dotnet", "CommonBackend.dll"]
