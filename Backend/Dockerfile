# =====================================================================
# Imagen de la API (ASP.NET Core .NET 10)
# Se construye en dos etapas: una para compilar y otra, más liviana, para ejecutar.
# =====================================================================

# --- Etapa 1: compilar (imagen con el SDK completo de .NET) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacion
WORKDIR /src

# Primero solo el .csproj: así Docker reutiliza los paquetes descargados
# si el código cambia pero las dependencias no
COPY Backend.csproj .
RUN dotnet restore

# Luego el resto del código, y se publica en modo Release
COPY . .
RUN dotnet publish -c Release -o /app/publicado --no-restore

# --- Etapa 2: ejecutar (imagen liviana, solo con el runtime de ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=compilacion /app/publicado .

# Carpeta donde vive la base SQLite (se conecta a un volumen en docker-compose.yml)
RUN mkdir -p /app/datos

# La API escucha en el puerto 8080 dentro del contenedor
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Backend.dll"]
