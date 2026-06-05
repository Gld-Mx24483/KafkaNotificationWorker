FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NotificationWorker/NotificationWorker.csproj", "NotificationWorker/"]
RUN dotnet restore "NotificationWorker/NotificationWorker.csproj"
COPY . .
WORKDIR "/src/NotificationWorker"
RUN dotnet build "NotificationWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationWorker.dll"]
