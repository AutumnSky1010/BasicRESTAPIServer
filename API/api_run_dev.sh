#!/bin/bash

# デバッガを使用する場合はサーバを起動しない。
if [ "${USE_DEBUGGER}" = true ]; then
    echo リモートエクスプローラからコンテナに接続し、デバッグしてください。
    # bashに入って終了しないようにする
    bash
else
    dotnet watch run --project /api_src/Server/Server.csproj --launch-profile http
fi