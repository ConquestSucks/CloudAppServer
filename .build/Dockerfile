# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CloudAppServer.sln ./

COPY CloudAppServer.Api/CloudAppServer.Api.csproj                  CloudAppServer.Api/
COPY CloudAppServer.Application/CloudAppServer.Application.csproj  CloudAppServer.Application/
COPY CloudAppServer.Domain/CloudAppServer.Domain.csproj            CloudAppServer.Domain/
COPY CloudAppServer.Infrastructure/CloudAppServer.Infrastructure.csproj CloudAppServer.Infrastructure/
COPY CloudAppServer.SharedKernel/CloudAppServer.SharedKernel.csproj    CloudAppServer.SharedKernel/

RUN dotnet restore CloudAppServer.sln

COPY . .

RUN dotnet publish CloudAppServer.Api/CloudAppServer.Api.csproj \
       -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CloudAppServer.Api.dll"]
