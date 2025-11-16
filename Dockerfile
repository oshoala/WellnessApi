# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copy solution file (if you have one)
COPY *.sln ./

# Copy project file
COPY WellnessApis/WellnessApis.csproj ./WellnessApis/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Build and publish
WORKDIR /app/WellnessApis
RUN dotnet publish -c Release -o /app/out

# Use the runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/out .

# Expose the port (Render uses 10000, but you can change this)
EXPOSE 10000

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:10000

# Run the application
ENTRYPOINT ["dotnet", "WellnessApis.dll"]