FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ToDoAI.csproj", "./"]
RUN dotnet restore "./ToDoAI.csproj"

COPY . .
RUN dotnet build "./ToDoAI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./ToDoAI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5114
EXPOSE 5114

ENTRYPOINT ["dotnet", "ToDoAI.dll"]