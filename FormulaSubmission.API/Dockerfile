#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["FormulaSubmission.API/FormulaSubmission.API.csproj", "FormulaSubmission.API/"]
RUN dotnet restore "FormulaSubmission.API/FormulaSubmission.API.csproj"
COPY . .
WORKDIR "/src/FormulaSubmission.API"
RUN dotnet build "FormulaSubmission.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FormulaSubmission.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FormulaSubmission.API.dll"]