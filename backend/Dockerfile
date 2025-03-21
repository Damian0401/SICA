FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SICA.Api/SICA.Api.csproj", "SICA.Api/"]
COPY ["SICA.Common/SICA.Common.csproj", "SICA.Common/"]
COPY ["SICA.Tools/SICA.Tools.csproj", "SICA.Tools/"]
RUN dotnet restore "SICA.Api/SICA.Api.csproj"
COPY . .
WORKDIR /src/SICA.Api
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "SICA.Api.csproj" --no-restore -c $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SICA.Api.csproj" --no-build -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y \
    libleptonica-dev \
    libtesseract-dev \
    && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.85.0.dll.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract54.dll.so

COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "SICA.Api.dll" ]