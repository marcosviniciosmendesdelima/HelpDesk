FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HelpDesk.Gateway/HelpDesk.Gateway.csproj", "HelpDesk.Gateway/"]
RUN dotnet restore "HelpDesk.Gateway/HelpDesk.Gateway.csproj"
COPY . .
WORKDIR "/src/HelpDesk.Gateway"
RUN dotnet publish "HelpDesk.Gateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HelpDesk.Gateway.dll"]