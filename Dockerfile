# === Етап 1: Збірка (використовуємо повний SDK) ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Копіюємо .csproj ТА відновлюємо пакети (включаючи Polly)
COPY *.csproj .
RUN dotnet restore

# 2. Копіюємо решту коду
COPY . .

# 3. Публікуємо
RUN dotnet publish "DigitalLibrary.csproj" -c Release -o /app/publish

# === Етап 2: Фінальний образ ===
# Використовуємо "товстий" SDK-образ як фінальний (для C++ libs)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DigitalLibrary.dll"]