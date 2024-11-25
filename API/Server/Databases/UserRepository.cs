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

    /// <summary>
    /// ユーザIDからユーザを探す
    /// </summary>
    /// <param name="targetUserId">見つける対象のユーザID</param>
    /// <returns>(成功したか、見つかったユーザ。失敗した場合はUnknownユーザ)</returns>
    public async Task<(bool ok, User foundUser)> TryFindUserByIdAsync(UserId targetUserId)
    {
        using var connection = new SqlConnection(connectionString);
        try
        {
            var userRow = await _userClient.ReadUserByIdAsync(connection, targetUserId.Value);
            _ = UserName.TryCreate(userRow.Name, out var userName);
            var user = User.CreateFrom(targetUserId, userName!, userRow.RegisteredAt);
            return (true, user);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "ユーザ取得時にエラーが発生しました。");
            return (false, User.Unknown);
        }
    }
}
