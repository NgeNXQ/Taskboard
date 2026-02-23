FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EC.TaskBoard.Web.csproj .
RUN dotnet restore EC.TaskBoard.Web.csproj

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

COPY --from=build /src/etc/ etc/

EXPOSE 8080
ENTRYPOINT ["dotnet", "EC.TaskBoard.Web.dll"]
