using Server.Models.UserAuthentications;
using Server.Models.Users;

namespace Server.UseCases.UserAuthentications;
public interface IUserAuthenticationRepository
{
    /// <summary>
    /// サインインIDからユーザID、ハッシュ化済みパスワードを探す
    /// </summary>
    /// <param name="signInId">サインインID</param>
    /// <returns>成功したか、見つけたユーザID、見つけたハッシュ化済みパスワード</returns>
    Task<(bool ok, UserId userId, HashedPassword hashedPassword)> TryFindAuthentication(SignInId signInId);
}