using Server.Models.Users;

namespace Server.Models.UserAuthentications;
public record StoredPassword
{
    private StoredPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static readonly StoredPassword Empty = new("");

    /// <summary>
    /// 保存済みパスワード文字列から生成する
    /// </summary>
    /// <param name="value">保存済みパスワード</param>
    /// <returns>HashedPasswordオブジェクト</returns>
    public static StoredPassword CreateFromString(string value)
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
        var inputHashedPassword = HashedPassword.HashFromRawPassword(hasher, inputRawPassword, inputUser);

        return Value == inputHashedPassword.Value;
    }
}