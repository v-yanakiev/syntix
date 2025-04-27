# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["aiExecBackend/aiExecBackend.csproj", "aiExecBackend/"]
RUN dotnet restore "aiExecBackend/aiExecBackend.csproj"
COPY . .
WORKDIR "/src/aiExecBackend"
RUN dotnet build "aiExecBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "aiExecBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl, use it for fly.io install, then remove
RUN apt update && apt install -y curl && \
    curl -L https://fly.io/install.sh | sh && \
    apt remove -y curl
    
# Adding shell form to see if it helps with command recognition
ENTRYPOINT dotnet aiExecBackend.dll
# Adding a fallback CMD in case ENTRYPOINT isn't recognized
CMD ["dotnet", "aiExecBackend.dll"]