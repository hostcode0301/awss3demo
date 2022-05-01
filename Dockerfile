FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 5268

ENV ASPNETCORE_URLS=http://+:5268

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["awss3demo.csproj", "./"]
RUN dotnet restore "awss3demo.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "awss3demo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "awss3demo.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Local development testing
# ENTRYPOINT ["dotnet", "awss3demo.dll"]

# Heroku production
CMD ASPNETCORE_URLS=http://*:$PORT dotnet awss3demo.dll
