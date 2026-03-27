FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TechWorkFlow.sln", "./"]
COPY ["Frontend/Frontend.csproj", "Frontend/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Frontend/Frontend.csproj"

COPY . .
RUN dotnet publish "Frontend/Frontend.csproj" -c Release -o /app/publish

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
