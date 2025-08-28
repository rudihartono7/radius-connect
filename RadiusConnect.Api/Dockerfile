FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

RUN apt update
RUN apt install -y unzip

RUN curl -fsSL https://bun.sh/install | BUN_INSTALL=/usr bash -s "bun-v1.2.7"

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y libgdiplus

COPY --from=build /publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "RadiusConnect.Api.dll"]
