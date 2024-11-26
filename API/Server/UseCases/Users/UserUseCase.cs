using Server.Models.UserAuthentications;
using Server.Models.Users;
using Server.UseCases.UserAuthentications;

namespace Server.UseCases.Users;

public class UserUseCase(IUserRepository userRepository, IUserAuthenticationRepository authRepository, IHasher hasher)
{
    private readonly IUserRepository _userRepository = userRepository;

    private readonly IUserAuthenticationRepository _authRepository = authRepository;

    private readonly IHasher _hasher = hasher;

    public record RegisterValidationResult(bool UserNameOk, bool SignInIdOk, bool RawPasswordOk);
    /// <summary>
    /// ユーザ登録ユースケース
    /// </summary>
    /// <param name="userNameValue">ユーザ名</param>
    /// <param name="signInIdValue">サインインID</param>
    /// <param name="rawPasswordValue">生パスワード</param>
    /// <returns>(ユースケース実行結果、入力値バリデーション結果)</returns>
    public async Task<(ResultTypes resultTypes, RegisterValidationResult validationResult)> RegisterUserAsync(string userNameValue, string signInIdValue, string rawPasswordValue)
    {
        // 入力値のバリデーションを行う。
        if (!UserName.TryCreate(userNameValue, out var userName))
        {
            var validationResult = new RegisterValidationResult(
                false,
                SignInId.TryCreate(signInIdValue, out _),
                RawPassword.TryCreate(rawPasswordValue, out _)
            );
            return (ResultTypes.ValidationError, validationResult);
        }
        if (!SignInId.TryCreate(signInIdValue, out var signInId))
        {
            var validationResult = new RegisterValidationResult(
                true,
                SignInId.TryCreate(signInIdValue, out _),
                RawPassword.TryCreate(rawPasswordValue, out _)
            );
            return (ResultTypes.ValidationError, validationResult);
        }
        if (!RawPassword.TryCreate(rawPasswordValue, out var rawPassword))
        {
            var validationResult = new RegisterValidationResult(true, true, false);
            return (ResultTypes.ValidationError, validationResult);
        }

        // サインインIDが既に登録されているかをチェックする
        var (existsId, _, _) = await _authRepository.TryFindAuthentication(signInId);
        if (existsId)
        {
            var validationResult = new RegisterValidationResult(true, false, true);
            return (ResultTypes.ValidationError, validationResult);
        }

        // 以降の処理ではバリデーションに成功している。
        var validParamResult = new RegisterValidationResult(true, true, true);

        // ユーザを作成し、DBに格納する。
        var user = User.CreateNew(userName);
        var hashedPassword = HashedPassword.HashFromRawPassword(_hasher, rawPassword, user);
        var ok = await _userRepository.TryCreateUserAsync(user, signInId, hashedPassword);
        if (ok)
        {
            // 成功。
            return (ResultTypes.Success, validParamResult);
        }
        else
        {
            // DBに格納する処理で失敗した場合は内部エラーとして扱う。
            return (ResultTypes.InternalError, validParamResult);
        }
    }

    /// <summary>
    /// ユーザを取得するユースケース
    /// </summary>
    /// <param name="userIdValue">ユーザID</param>
    /// <returns>結果、ユーザ名</returns>
    public async Task<(ResultTypes result, User user)> GetUserAsync(Guid userIdValue)
    {
        var userId = new UserId(userIdValue);

        // ユーザを取得し、ユーザ名を返す
        var (ok, user) = await _userRepository.TryFindUserByIdAsync(userId);
        if (ok)
        {
            return (ResultTypes.Success, user);
        }

        // ユーザが見つからなかった場合は内部エラーの可能性が高い
        return (ResultTypes.InternalError, user);
    }
}
