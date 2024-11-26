using Microsoft.Data.SqlClient;
using Server.Databases.DBClients;
using Server.Models.UserAuthentications;
using Server.Models.Users;
using Server.UseCases.UserAuthentications;

namespace Server.Databases;

public class UserAuthenticationRepository(ILogger<UserAuthenticationRepository> logger, string connectionString) : IUserAuthenticationRepository
{
    private readonly UserAuthenticationsClient _authenticationsClient = new();

    private readonly ILogger<UserAuthenticationRepository> _logger = logger;

    /// <summary>
    /// サインインIDからユーザID、パスワードを探す
    /// </summary>
    /// <param name="signInId">サインインID</param>
    /// <returns>成功したか、見つけたユーザID、見つけたパスワード</returns>
    public async Task<(bool ok, UserId userId, StoredPassword storedPassword)> TryFindAuthenticationAsync(SignInId signInId)
    {
        using var connection = new SqlConnection(connectionString);

        try
        {
            var authRow = await _authenticationsClient.ReadBySignInIdAsync(connection, signInId.Value);
            var hashedPassword = StoredPassword.CreateFromString(authRow.Password);
            var userId = new UserId(authRow.UserId);
            return (true, userId, hashedPassword);
        }
        catch (Exception exception)
        {
            // 例外を出力する。
            _logger.LogError(exception, "ユーザID・パスワードの取得に失敗しました。");
            return (false, UserId.Empty, StoredPassword.Empty);
        }
    }
}