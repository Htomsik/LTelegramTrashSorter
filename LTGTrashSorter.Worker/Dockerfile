FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LTGTrashSorter.Worker/LTGTrashSorter.Worker.csproj", "LTGTrashSorter.Worker/"]
COPY ["LTGTrashSorter.Core/LTGTrashSorter.Core.csproj", "LTGTrashSorter.Core/"]
COPY ["LTGTrashSorter.AI/LTGTrashSorter.AI.csproj", "LTGTrashSorter.AI/"]
COPY ["LTGTrashSorter.TG/LTGTrashSorter.TG.csproj", "LTGTrashSorter.TG/"]
RUN dotnet restore "LTGTrashSorter.Worker/LTGTrashSorter.Worker.csproj"
COPY . .
WORKDIR "/src/LTGTrashSorter.Worker"
RUN dotnet build "./LTGTrashSorter.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LTGTrashSorter.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LTGTrashSorter.Worker.dll"]
