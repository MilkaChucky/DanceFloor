FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /Docker

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /Docker
COPY --from=build /Docker/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "DanceFloor.Api.dll"]


