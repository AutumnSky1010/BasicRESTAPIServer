using System.Diagnostics.CodeAnalysis;

namespace Server.Models.UserAuthentications;
public record SignInId
{
    private SignInId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// サインインIDの作成を試みる。
    /// </summary>
    /// <param name="value">サインインIDの文字列</param>
    /// <param name="signInId">作成したサインインID</param>
    /// <returns>成功: true 失敗: false</returns>
    public static bool TryCreate(string value, [NotNullWhen(true)] out SignInId? signInId)
    {
        // ルール：サインインIDは10文字以上100文字以下とする。
        if (10 <= value.Length && value.Length <= 100)
        {
            signInId = new(value);
            return true;
        }

        signInId = null;
        return false;
    }
}