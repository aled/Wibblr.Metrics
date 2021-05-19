#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Wibblr.Metrics.Web.BlazorServer/Wibblr.Metrics.Web.BlazorServer.csproj", "Wibblr.Metrics.Web.BlazorServer/"]
RUN dotnet restore "Wibblr.Metrics.Web.BlazorServer/Wibblr.Metrics.Web.BlazorServer.csproj"
COPY . .
WORKDIR "/src/Wibblr.Metrics.Web.BlazorServer"
RUN dotnet build "Wibblr.Metrics.Web.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wibblr.Metrics.Web.BlazorServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wibblr.Metrics.Web.BlazorServer.dll"]