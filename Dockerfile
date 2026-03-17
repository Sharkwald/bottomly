FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /src

RUN dotnet workload install aspire

COPY bottomly.net.slnx ./
COPY Bottomly/Bottomly.csproj Bottomly/
COPY Bottomly.ServiceDefaults/Bottomly.ServiceDefaults.csproj Bottomly.ServiceDefaults/

RUN dotnet restore Bottomly/Bottomly.csproj

COPY Bottomly/ Bottomly/
COPY Bottomly.ServiceDefaults/ Bottomly.ServiceDefaults/

RUN dotnet publish Bottomly/Bottomly.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/nightly/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Bottomly.dll"]
