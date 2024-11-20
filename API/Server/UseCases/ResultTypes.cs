namespace Server.UseCases;

public enum ResultTypes
{
    /// <summary>
    /// ユースケースが成功した場合
    /// </summary>
    Success,
    /// <summary>
    /// ユースケースがユーザ入力値のバリデーション結果で失敗した場合
    /// </summary>
    ValidationError,
    /// <summary>
    /// ユースケースが内部エラーが原因で失敗した場合
    /// </summary>
    InternalError,
}
