# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copy solution file
COPY WellnessApi.sln ./

# Copy project file
COPY StanbicApi/StanbicApi.csproj ./StanbicApi/

# Restore dependencies
WORKDIR /app
RUN dotnet restore WellnessApi.sln

# Copy everything else
COPY . ./

# Build and publish
RUN dotnet publish WellnessApi.sln -c Release -o out

# Use the runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port Render will use
EXPOSE 10000

# Run the application
ENTRYPOINT ["dotnet", "StanbicApi.dll"]