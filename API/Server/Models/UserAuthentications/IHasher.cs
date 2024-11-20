using Server.Models.Users;

namespace Server.Models.UserAuthentications;

public interface IHasher
{
    /// <summary>
    /// ハッシュ化されたパスワードを生成する。
    /// </summary>
    /// <param name="user">パスワードの所有者</param>
    /// <param name="rawPassword">パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    public string Generate(User user, RawPassword rawPassword);
}