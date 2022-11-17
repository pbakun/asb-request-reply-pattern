#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RequestReplyPattern.Consumer/RequestReplyPattern.Consumer.csproj", "RequestReplyPattern.Consumer/"]
RUN dotnet restore "RequestReplyPattern.Consumer/RequestReplyPattern.Consumer.csproj"
COPY . .
WORKDIR "/src/RequestReplyPattern.Consumer"
RUN dotnet build "RequestReplyPattern.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RequestReplyPattern.Consumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RequestReplyPattern.Consumer.dll"]