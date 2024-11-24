using System.Diagnostics.CodeAnalysis;

namespace Server.Models.Users;
public record UserName
{
    private UserName(string value)
    {
        Value = value;
    }

    public static readonly UserName Empty = new("");

    public string Value { get; }

    /// <summary>
    /// ユーザ名の作成を試みる
    /// </summary>
    /// <param name="value">ユーザ名の文字列</param>
    /// <param name="userName">作成したユーザ名</param>
    /// <returns>成功: true 失敗: false</returns>
    public static bool TryCreate(string value, [NotNullWhen(true)] out UserName? userName)
    {
        // ルール：空値を許可しない かつ 50文字以内
        if (!string.IsNullOrWhiteSpace(value) && value.Length <= 50)
        {
            userName = new(value);
            return true;
        }

        userName = null;
        return false;
    }
}