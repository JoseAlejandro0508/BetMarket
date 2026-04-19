FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ApuestasDeportivas.Api/ApuestasDeportivas.Api.csproj", "ApuestasDeportivas.Api/"]
COPY ["ApuestasDeportivas.Application/ApuestasDeportivas.Application.csproj", "ApuestasDeportivas.Application/"]
COPY ["ApuestasDeportivas.Contracts/ApuestasDeportivas.Contracts.csproj", "ApuestasDeportivas.Contracts/"]
COPY ["ApuestasDeportivas.Domain/ApuestasDeportivas.Domain.csproj", "ApuestasDeportivas.Domain/"]
COPY ["ApuestasDeportivas.Infrastructure/ApuestasDeportivas.Infrastructure.csproj", "ApuestasDeportivas.Infrastructure/"]
RUN dotnet restore "ApuestasDeportivas.Api/ApuestasDeportivas.Api.csproj"

COPY . .
WORKDIR "/src/ApuestasDeportivas.Api"
RUN dotnet publish "ApuestasDeportivas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApuestasDeportivas.Api.dll"]
