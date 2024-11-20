using System.Diagnostics.CodeAnalysis;

namespace Server.Models.UserAuthentications;
public record RawPassword
{
    private RawPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// 生パスワードの作成を試みる
    /// </summary>
    /// <param name="value">生パスワードの文字列</param>
    /// <param name="rawPassword">作成した生パスワード</param>
    /// <returns>成功: true 失敗: false</returns>
    public static bool TryCreate(string value, [NotNullWhen(true)] out RawPassword? rawPassword)
    {
        // ルール：パスワードは10文字以上とする。
        if (10 <= value.Length)
        {
            rawPassword = new(value);
            return true;
        }

        rawPassword = null;
        return false;
    }
}