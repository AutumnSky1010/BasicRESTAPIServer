using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Server.Databases.DBClients;
public class UserAuthenticationsClient
{
    public record InsertParam(Guid UserId, string SignInId, string HashedPassword);
    /// <summary>
    /// ユーザ認証情報を挿入する
    /// </summary>
    /// <param name="connection">コネクション</param>
    /// <param name="tx">トランザクション</param>
    /// <param name="param">挿入用パラメタ</param>
    /// <returns>タスク</returns>
    public async Task InsertAsync(SqlConnection connection, DbTransaction tx, InsertParam param)
    {
        // 認証情報を挿入する
        var authenticationParam = new
        {
            param.UserId,
            param.SignInId,
            param.HashedPassword,
        };
        var authenticationQuery = $"""
                INSERT INTO [app].[user_authentications] VALUES (
                    @{nameof(authenticationParam.UserId)},
                    @{nameof(authenticationParam.SignInId)},
                    @{nameof(authenticationParam.HashedPassword)},
                    default,
                    default
                );
            """;
        await connection.ExecuteAsync(authenticationQuery, authenticationParam, tx);
    }
}
