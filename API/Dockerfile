FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /api

# すべてコピーする
COPY . ./

# リストアする
RUN dotnet restore
# パブリッシュする
RUN dotnet publish -c Release -o out

# ランタイムイメージを利用し、実行環境を準備する。
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /api
COPY --from=build-env /api/out .
# http通信を行い、ポートを設定
ENV ASPNETCORE_URLS="http://+:8080;"
CMD ["dotnet", "Server.dll"]