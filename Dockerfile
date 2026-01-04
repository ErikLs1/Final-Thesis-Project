# Build - stage 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + props + csproj files first (better layer caching)
COPY FinalThesisProject.sln ./
COPY Directory.build.props ./

COPY App.Common/App.Common.csproj App.Common/
COPY App.Data/App.Data.csproj App.Data/
COPY App.Domain/App.Domain.csproj App.Domain/
COPY App.EF/App.EF.csproj App.EF/
COPY App.Repository/App.Repository.csproj App.Repository/
COPY App.Service/App.Service.csproj App.Service/
COPY WebApp/WebApp.csproj WebApp/

RUN dotnet restore "FinalThesisProject.sln"

# Copy everything else and publish
COPY . .

# Runtime files
WORKDIR /src/WebApp
RUN dotnet publish "WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Runtime -  stage 2
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# copy build output to runtime image
COPY --from=build /app/publish .

# copy resources
COPY --from=build /src/App.Data/Resources /app/local_resources

ENTRYPOINT ["dotnet", "WebApp.dll"]


