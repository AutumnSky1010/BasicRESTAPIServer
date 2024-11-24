using Server.Models.Users;

namespace Server.Models.UserAuthentications;

public record HashedPassword
{
    private HashedPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static readonly HashedPassword Empty = new("");

    /// <summary>
    /// ハッシュ化したパスワードの生成をする。
    /// </summary>
    /// <param name="hasher">ハッシュ化用オブジェクト</param>
    /// <param name="rawPassword">生パスワード</param>
    /// <param name="user">パスワードの所有者</param>
    /// <returns>ハッシュ化したパスワード</returns>
    public static HashedPassword HashFromRawPassword(IHasher hasher, RawPassword rawPassword, User user)
    {
        // 生パスワードをハッシュ化する。
        return new(hasher.Generate(user, rawPassword));
    }

    /// <summary>
    /// ハッシュ化済みパスワード文字列から生成する
    /// </summary>
    /// <param name="value">ハッシュ化済みのパスワード</param>
    /// <returns>HashedPasswordオブジェクト</returns>
    public static HashedPassword CreateFromString(string value)
    {
        return new(value);
    }

    /// <summary>
    /// 入力された生パスワードが正しいかを検証する。
    /// </summary>
    /// <param name="hasher">ハッシュ化用オブジェクト</param>
    /// <param name="inputRawPassword">入力された生パスワード</param>
    /// <param name="inputUser">入力者</param>
    /// <returns>正しい: true、不正: false</returns>
    public bool Verify(IHasher hasher, RawPassword inputRawPassword, User inputUser)
    {
        var inputHashedPassword = HashFromRawPassword(hasher, inputRawPassword, inputUser);

        return this == inputHashedPassword;
    }
}