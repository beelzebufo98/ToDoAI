FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ToDoAI.API/ToDoAI.API.csproj", "ToDoAI.API/"]
COPY ["ToDoAI.Application/ToDoAI.Application.csproj", "ToDoAI.Application/"]
COPY ["ToDoAI.Domain/ToDoAI.Domain.csproj", "ToDoAI.Domain/"]
COPY ["ToDoAI.Infrastructure/ToDoAI.Infrastructure.csproj", "ToDoAI.Infrastructure/"]
RUN dotnet restore "./ToDoAI.API/ToDoAI.API.csproj"

COPY . .
RUN dotnet build "./ToDoAI.API/ToDoAI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./ToDoAI.API/ToDoAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5114
EXPOSE 5114

ENTRYPOINT ["dotnet", "ToDoAI.dll"]
