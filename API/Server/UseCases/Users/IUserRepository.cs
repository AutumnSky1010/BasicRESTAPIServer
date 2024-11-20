using Server.Models.UserAuthentications;
using Server.Models.Users;

namespace Server.UseCases.Users;

public interface IUserRepository
{
    /// <summary>
    /// ユーザ作成を試みる。
    /// </summary>
    /// <param name="newUser">新規ユーザ</param>
    /// <param name="signInId">サインインID</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワード</param>
    /// <returns>成功時: true, 失敗時: false</returns>
    Task<bool> TryCreateUserAsync(User newUser, SignInId signInId, HashedPassword hashedPassword);
}