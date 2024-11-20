using Server.Models.UserAuthentications;
using Server.Models.Users;
using System.Security.Cryptography;
using System.Text;

namespace Server.Authentications;

public class HasherByPBKDF2(string pepper) : IHasher
{
    /// <summary>
    /// ペッパー
    /// </summary>
    private readonly string _pepper = pepper;
    /// <summary>
    /// ストレッチング回数
    /// </summary>
    private readonly static int _iterations = 310000;

    /// <summary>
    /// 生パスワードの作成を試みる
    /// </summary>
    /// <param name="value">生パスワードの文字列</param>
    /// <param name="rawPassword">作成した生パスワード</param>
    /// <returns>成功: true 失敗: false</returns>
    public string Generate(User user, RawPassword rawPassword)
    {
        // ユーザ登録日をソルトとする
        var saltBytes = Encoding.UTF8.GetBytes(user.RegisteredAt.ToString("yyyyMMddHHmmss"));
        var pepperBytes = Encoding.UTF8.GetBytes(_pepper);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(rawPassword.Value),
            // ソルトとペッパーを結合して渡す
            [.. saltBytes, .. pepperBytes],
            _iterations,
            HashAlgorithmName.SHA512);

        // 8 * 64 = 512 ビットのハッシュ値を取得する
        var hash = pbkdf2.GetBytes(64);

        return Convert.ToBase64String(hash);
    }
}