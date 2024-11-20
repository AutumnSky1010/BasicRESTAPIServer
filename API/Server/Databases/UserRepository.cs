using Microsoft.Data.SqlClient;
using Server.Databases.DBClients;
using Server.Models.UserAuthentications;
using Server.Models.Users;
using Server.UseCases.Users;

namespace Server.Databases;

public class UserRepository(ILogger<UserRepository> logger, string connectionString) : IUserRepository
{
    private readonly UsersClient _userClient = new();

    private readonly ILogger<UserRepository> _logger = logger;

    private readonly UserAuthenticationsClient _authenticationsClient = new();

    /// <summary>
    /// ユーザ作成を試みる。
    /// </summary>
    /// <param name="newUser">新規ユーザ</param>
    /// <param name="signInId">サインインID</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワード</param>
    /// <returns>成功時: true, 失敗時: false</returns>
    public async Task<bool> TryCreateUserAsync(User newUser, SignInId signInId, HashedPassword hashedPassword)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var tx = await connection.BeginTransactionAsync();
        try
        {
            // ユーザを挿入する。
            var insertUserParam = new UsersClient.InsertUserParam(newUser.Id.Value, newUser.Name.Value, newUser.RegisteredAt);
            await _userClient.InsertAsync(connection, tx, insertUserParam);

            // 認証用情報を挿入する。
            var insertAuthParam = new UserAuthenticationsClient.InsertParam(newUser.Id.Value, signInId.Value, hashedPassword.Value);
            await _authenticationsClient.InsertAsync(connection, tx, insertAuthParam);

            // コミットする
            await tx.CommitAsync();
            return true;
        }
        catch (Exception exception)
        {
            // 例外を出力し、ロールバックする。
            _logger.LogError(exception, "ユーザ作成時にエラーが発生しました。");
            await tx.RollbackAsync();
            return false;
        }
    }
}
