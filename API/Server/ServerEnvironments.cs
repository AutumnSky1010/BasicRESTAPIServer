namespace Server;

public enum VariableTypes
{
    /// <summary>
    /// データベースの接続文字列
    /// </summary>
    DBConnectionString,
    /// <summary>
    /// ハッシュ化時に用いるペッパー
    /// </summary>
    HasherPepper,
    /// <summary>
    /// JWTで用いる発行者
    /// </summary>
    JWTIssuer,
    /// <summary>
    /// リフレッシュトークン生成に用いる秘密鍵
    /// </summary>
    JWTKeyForRefreshToken,
    /// <summary>
    /// アクセストークン生成に用いる秘密鍵
    /// </summary>
    JWTKeyForAccessToken,
    /// <summary>
    /// JWTの受信者情報(基本的にURI形式)
    /// </summary>
    JWTAudience,
}

public class ServerEnvironments
{
    /// <summary>
    /// 環境変数の値を取得する。失敗した場合は落ちる。
    /// </summary>
    /// <param name="type">取得可能な環境変数の種類</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">typeが不正な値だった場合スローされる</exception>
    /// <exception cref="ArgumentException">環境変数が設定されていなかった場合スローされる</exception>
    public static string Get(VariableTypes type)
    {
        var variableName = type switch
        {
            VariableTypes.DBConnectionString => "DB_CONNECTION_STR",
            VariableTypes.HasherPepper => "PEPPER",
            VariableTypes.JWTIssuer => "JWT_ISSURER",
            VariableTypes.JWTKeyForRefreshToken => "JWT_KEY_REFRESH_TOKEN",
            VariableTypes.JWTKeyForAccessToken => "JWT_KEY_ACCESS_TOKEN",
            VariableTypes.JWTAudience => "JWT_AUDIENCE",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"環境変数名の設定を忘れています: {type}")
        };

        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new ArgumentException($"環境変数 {variableName} が設定されていません。", nameof(type));
    }
}
