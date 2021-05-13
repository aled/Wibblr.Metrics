#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Wibblr.Metrics.RestApi/Wibblr.Metrics.RestApi.csproj", "Wibblr.Metrics.RestApi/"]
RUN dotnet restore "Wibblr.Metrics.RestApi/Wibblr.Metrics.RestApi.csproj"
COPY . .
WORKDIR "/src/Wibblr.Metrics.RestApi"
RUN dotnet build "Wibblr.Metrics.RestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wibblr.Metrics.RestApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wibblr.Metrics.RestApi.dll"]