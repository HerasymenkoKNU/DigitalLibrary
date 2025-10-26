# === Етап 1: "Build" (Компіляція) ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish "DigitalLibrary.csproj" -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef



FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY --from=build /root/.dotnet/tools /root/.dotnet/tools


ENV PATH="${PATH}:/root/.dotnet/tools"


ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DigitalLibrary.dll"]