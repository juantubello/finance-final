# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinanzasApp.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

VOLUME ["/data"]

ENV DatabasePath=/data/finance_db
ENV ASPNETCORE_URLS=http://+:6097

EXPOSE 6097

ENTRYPOINT ["dotnet", "FinanzasApp.dll"]
