using Server.Models.Users;

namespace Server.Models.UserAuthentications;

public record HashedPassword
{
    private HashedPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// ハッシュ化したパスワードの生成をする。
    /// </summary>
    /// <param name="hasher">ハッシュ化用オブジェクト</param>
    /// <param name="rawPassword">生パスワード</param>
    /// <param name="user">パスワードの所有者</param>
    /// <returns>ハッシュ化したパスワード</returns>
    public static HashedPassword Create(IHasher hasher, RawPassword rawPassword, User user)
    {
        // 生パスワードをハッシュ化する。
        return new(hasher.Generate(user, rawPassword));
    }
}