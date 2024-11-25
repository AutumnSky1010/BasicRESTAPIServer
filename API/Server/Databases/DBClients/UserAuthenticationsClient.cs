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

    public record UserAuthenticationRow(int Id, Guid UserId, string SignInId, string Password, string RefreshToken, DateTime RefreshTokenExpiration);
    /// <summary>
    /// サインインIDをキーとして読む
    /// </summary>
    /// <param name="connection">コネクション</param>
    /// <param name="signInId">キーとなるサインインID</param>
    /// <returns>見つかった認証情報</returns>
    public async Task<UserAuthenticationRow> ReadBySignInIdAsync(SqlConnection connection, string signInId)
    {
        var param = new
        {
            signInId
        };
        var query = $"""
                SELECT
                    id,
                    user_id,
                    sign_in_id,
                    password,
                    refresh_token,
                    refresh_token_expiration
                FROM
                    [app].[user_authentications] user_authentications
                WHERE
                    user_authentications.sign_in_id = @{nameof(param.signInId)}
            """;

        var row = await connection.QueryFirstAsync<UserAuthenticationRow>(query, param);

        return row;
    }
}
