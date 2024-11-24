using Server.Models.Users;

namespace Server.UseCases.UserAuthentications;

public interface IAccessTokenGenerator
{
    /// <summary>
    /// トークンを生成する。
    /// </summary>
    /// <param name="userId">ユーザID</param>
    /// <returns>トークン</returns>
    string Generate(UserId userId);
}
