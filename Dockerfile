FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build -c Release
EXPOSE 8080
ENTRYPOINT ["dotnet", "run", "--urls", "http://0.0.0.0:8080"]