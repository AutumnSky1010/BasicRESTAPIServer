FROM mcr.microsoft.com/mssql/server:2022-latest

# ROOT権限にする
USER root

# dotnetをインストールし、sqlprojをビルド可能にする
RUN apt-get update && apt-get install -y dotnet-sdk-8.0
RUN dotnet tool install -g microsoft.sqlpackage

# sqlpackageへのパスを通す
ENV PATH="${PATH}:/root/.dotnet/tools"

# 実行権限を付与
RUN chmod +x /root/.dotnet/tools/sqlpackage

# init.shをコピー
COPY init.sh .

# 初期化スクリプト
ENTRYPOINT ["/bin/bash", "init.sh"]