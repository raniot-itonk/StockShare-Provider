FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["StockShare-Provider/StockShareProvider.csproj", "StockShare-Provider/"]
RUN dotnet restore "StockShare-Provider/StockShareProvider.csproj"
COPY . .
WORKDIR "/src/StockShare-Provider"
RUN dotnet build "StockShareProvider.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "StockShareProvider.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StockShareProvider.dll"]