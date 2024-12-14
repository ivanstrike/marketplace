FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ProductCartMicroservice.csproj ./  
RUN dotnet restore ProductCartMicroservice.csproj

COPY . .  
RUN dotnet build ProductCartMicroservice.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ProductCartMicroservice.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCartMicroservice.dll"]
