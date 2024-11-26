using Server.Models.UserAuthentications;
using Server.Models.Users;

namespace Server.UseCases.UserAuthentications;
public interface IUserAuthenticationRepository
{
    /// <summary>
    /// サインインIDからユーザID、パスワードを探す
    /// </summary>
    /// <param name="signInId">サインインID</param>
    /// <returns>成功したか、見つけたユーザID、見つけたパスワード</returns>
    Task<(bool ok, UserId userId, StoredPassword storedPassword)> TryFindAuthenticationAsync(SignInId signInId);
}