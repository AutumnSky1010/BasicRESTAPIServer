using Server.Models.UserAuthentications;
using Server.UseCases.Users;

namespace Server.UseCases.UserAuthentications;

public class UserAuthenticationUseCase(IHasher hasher, IAccessTokenGenerator tokenGenerator, IUserAuthenticationRepository authRepository, IUserRepository userRepository)
{
    private readonly IUserAuthenticationRepository _authRepository = authRepository;

    private readonly IUserRepository _userRepository = userRepository;

    private readonly IHasher _hasher = hasher;

    private readonly IAccessTokenGenerator _tokenGenerator = tokenGenerator;

    /// <summary>
    /// サインインのユースケース
    /// </summary>
    /// <param name="signInIdString">サインインIDの文字列</param>
    /// <param name="rawPasswordString">ユーザが入力した生パスワード文字列</param>
    /// <returns>(ユースケース実行結果、アクセストークン文字列)</returns>
    public async Task<(ResultTypes result, string accessToken)> SignInAsync(string signInIdString, string rawPasswordString)
    {
        // ユーザ入力の文字列からサインインID、生パスワードオブジェクトを生成する
        if (!SignInId.TryCreate(signInIdString, out var signInId) || !RawPassword.TryCreate(rawPasswordString, out var rawPassword))
        {
            return (ResultTypes.ValidationError, "");
        }

        // サインインIDから登録されたユーザID、ハッシュ化済みパスワードを取得す
        // ここで失敗する場合はどちらかというとユーザ入力が原因であることが多いため、バリデーションエラーとする
        var (isFoundAuthentication, userId, hashedPassword) = await _authRepository.TryFindAuthentication(signInId);
        if (!isFoundAuthentication)
        {
            return (ResultTypes.ValidationError, "");
        }

        // ユーザIDからユーザを取得する。ここで失敗した場合は確実にサーバ側エラー
        var (isFoundUser, user) = await _userRepository.TryFindUserByIdAsync(userId);
        if (!isFoundUser)
        {
            return (ResultTypes.InternalError, "");
        }

        // パスワードの検証を行い、成功した場合はアクセストークンを返却する
        if (hashedPassword.Verify(_hasher, rawPassword, user))
        {
            var accessToken = _tokenGenerator.Generate(userId);
            return (ResultTypes.Success, accessToken);
        }

        return (ResultTypes.ValidationError, "");
    }
}