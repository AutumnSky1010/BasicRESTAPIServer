using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Server.Databases.DBClients;
public class UsersClient
{
    public record UserRow(Guid Id, string Name, DateTime RegisteredAt);
    /// <summary>
    /// ユーザIDをキーとしてユーザを読む
    /// </summary>
    /// <param name="connection">コネクション</param>
    /// <param name="userId">キーとするユーザID</param>
    /// <returns>読んだユーザ</returns>
    public async Task<UserRow> ReadUserByIdAsync(SqlConnection connection, Guid userId)
    {
        var param = new
        {
            userId,
        };

        var query = $"""
                SELECT
                    id,
                    name,
                    registered_at
                FROM
                    [app].[users] users
                WHERE
                    users.id = @{nameof(param.userId)}
            """;

        var userRow = await connection.QueryFirstAsync<UserRow>(query, param);

        return userRow;
    }

    public record InsertUserParam(Guid UserId, string Name, DateTime RegisteredAt);
    /// <summary>
    /// ユーザ情報を挿入する
    /// </summary>
    /// <param name="connection">コネクション</param>
    /// <param name="tx">トランザクション</param>
    /// <param name="param">挿入用パラメタ</param>
    /// <returns>タスク</returns>
    public async Task InsertAsync(SqlConnection connection, DbTransaction tx, InsertUserParam param)
    {
        // ユーザデータの挿入を行う。
        var userParam = new
        {
            param.UserId,
            param.Name,
            param.RegisteredAt
        };
        var userQuery = $"""
                INSERT INTO [app].[users] VALUES (
                    @{nameof(userParam.UserId)},
                    @{nameof(userParam.Name)},
                    @{nameof(userParam.RegisteredAt)}
                );
            """;
        await connection.ExecuteAsync(userQuery, userParam, tx);
    }
}
