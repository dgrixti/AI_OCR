FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src

# Added resources
COPY ["Dataset/cw2DataSet1.csv", "dataset/cw2DataSet1.csv"]
COPY ["Dataset/cw2DataSet2.csv", "dataset/cw2DataSet2.csv"]
COPY ["Dataset/mlpstate.json", "dataset/mlpstate.json"]
COPY ["Dataset/test2tsp.json", "dataset/test2tsp.json"]
COPY ["Dataset/test2tsp.txt", "dataset/test2tsp.txt"]

COPY ["AI_OCR.csproj", ""]
RUN dotnet restore "./AI_OCR.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AI_OCR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AI_OCR.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AI_OCR.dll"]