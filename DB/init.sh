#!/bin/bash

# エラーが発生したら即座に終了する
set -e

# 環境変数のチェック
required_vars=("MSSQL_SA_PASSWORD" "PROJECT_PATH" "DACPAC_PATH")
for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo "エラー: ${var} が設定されていません。"
        exit 1
    fi
done

wait_mssql() {
    echo "DBSを起動します(最低10秒待機します)。"
    sleep 10

    timelimit=30
    for i in $(seq 1 "${timelimit}"); do
        if version=$(/opt/mssql-tools18/bin/sqlcmd -S localhost,1433 -U sa -P "${MSSQL_SA_PASSWORD}" -Q "SELECT @@VERSION" -C 2>/dev/null); then
            echo "起動完了しました。バージョン情報: ${version}"
            return 0
        else
            echo "起動待機中... (${i}/${timelimit})"
            sleep 1
        fi
    done

    echo "タイムアウト: DBSの起動に失敗しました。"
    return 1
}

execute_ddl() {
    echo "sqlプロジェクトをビルドします。"
    if ! dotnet build "${PROJECT_PATH}"; then
        echo "ビルドに失敗しました。"
        return 1
    fi

    echo "プロジェクトビルド内容をmssqlにパブリッシュします。"
    if ! sqlpackage /Action:Publish \
        /SourceFile:"${DACPAC_PATH}" \
        /TargetServerName:"localhost,1433" \
        /TargetUser:"sa" \
        /TargetPassword:"${MSSQL_SA_PASSWORD}" \
        /TargetDatabaseName:"${DB_NAME}" \
        /TargetTrustServerCertificate:"True"; then
        echo "パブリッシュに失敗しました。"
        return 1
    fi

    echo "DDLの実行が完了しました。"
    return 0
}

main() {
    if ! wait_mssql; then
        exit 1
    fi

    initialized="/init"
    if [ ! -e "${initialized}" ]; then
        if execute_ddl; then
            touch "${initialized}"
            echo "初期化が完了しました。"
        else
            echo "初期化に失敗しました。"
            exit 1
        fi
    else
        echo "既に初期化済みです。"
    fi
}

main &
exec /opt/mssql/bin/sqlservr --accept-eula