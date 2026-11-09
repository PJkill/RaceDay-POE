# Use the official ASP.NET Core runtime as a base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"

# Copy the rest of the files and build
COPY . .
WORKDIR "/src/RaceDay.MVC"
RUN dotnet build "RaceDay.MVC.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]