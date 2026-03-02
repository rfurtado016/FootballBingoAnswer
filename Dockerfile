# 1) Base image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Render will route traffic to whatever port we expose and listen on
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2) Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and build
COPY . .
RUN dotnet publish -c Release -o /app/out

# 3) Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "FootballBingoWeb.dll"]
