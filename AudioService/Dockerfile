FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

WORKDIR /app

COPY . ./


RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/out ./

COPY --from=build /app/libs ./libs

COPY appsettings.json ./
COPY appsettings.Docker.json ./

CMD ["dotnet", "AudioService.dll"]
