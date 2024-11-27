using Moq;
using Server.Models.UserAuthentications;
using Server.Models.Users;
using Server.UseCases;
using Server.UseCases.UserAuthentications;
using Server.UseCases.Users;

namespace ServerTests.UseCases.Users;
public class TestUserAuthenticationUseCase
{
    [Fact(DisplayName = "サインインできるかの正常系テスト")]
    public async Task SignIn_Valid()
    {
        // Arrange
        // モックを作成
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var authRepository = new Mock<IUserAuthenticationRepository>();
        var userRepository = new Mock<IUserRepository>();
        var useCase = new UserAuthenticationUseCase(hasher.Object, tokenGenerator.Object, authRepository.Object, userRepository.Object);
        // データの定義
        var userInputCurrentPassword = "userInputPass";
        var storedPassword = StoredPassword.CreateFromString("fromDB");
        var userInputSignInId = "userInputSignInId";
        var userNameValue = "user";
        if (!SignInId.TryCreate(userInputSignInId, out var signInId) ||
            !UserName.TryCreate(userNameValue, out var userName) ||
            !RawPassword.TryCreate(userInputCurrentPassword, out var rawPassword))
        {
            Assert.Fail("テストデータが不正です");
            return;
        }
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var user = User.Create(userId, userName, registeredAt);
        var expectedAccessToken = "accessToken";
        // アクセストークン生成の動作を定義
        tokenGenerator
            .Setup(tg => tg.Generate(user.Id))
            .Returns(expectedAccessToken);
        // ハッシュ化したパスワードを保存済みパスワードと同一のものにする
        hasher
            .Setup(h => h.Generate(user, rawPassword))
            .Returns(storedPassword.Value);
        // ユーザリポジトリからユーザ情報を受け取る時の動作を定義
        userRepository
            .Setup(repo => repo.TryFindUserByIdAsync(userId))
            .ReturnsAsync((true, user));
        // ユーザリポジトリからパスワードを受け取る時の動作を定義
        authRepository
            .Setup(repo => repo.TryFindAuthenticationAsync(signInId))
            .ReturnsAsync((true, userId, storedPassword));

        // Act
        var (resultTypes, actualAccessToken) = await useCase.SignInAsync(userInputSignInId, userInputCurrentPassword);

        // Assert
        // アクセストークンを生成しているかを確認する。
        tokenGenerator.Verify(tg => tg.Generate(user.Id), Times.Once());
        // 戻り値のチェックを行う
        Assert.Equal(ResultTypes.Success, resultTypes);
        Assert.Equal(expectedAccessToken, actualAccessToken);
    }

    [Fact(DisplayName = "サインインできるかの異常系テスト")]
    public async Task SignIn_Invalid()
    {
        // Arrange
        // モックを作成
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var authRepository = new Mock<IUserAuthenticationRepository>();
        var userRepository = new Mock<IUserRepository>();
        var useCase = new UserAuthenticationUseCase(hasher.Object, tokenGenerator.Object, authRepository.Object, userRepository.Object);
        // データの定義
        var userInputCurrentPassword = "userInputPass";
        var storedPassword = StoredPassword.CreateFromString("fromDB");
        var userInputSignInId = "userInputSignInId";
        var userNameValue = "user";
        if (!SignInId.TryCreate(userInputSignInId, out var signInId) ||
            !UserName.TryCreate(userNameValue, out var userName) ||
            !RawPassword.TryCreate(userInputCurrentPassword, out var rawPassword))
        {
            Assert.Fail("テストデータが不正です");
            return;
        }
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var user = User.Create(userId, userName, registeredAt);
        var expectedAccessToken = "accessToken";
        // アクセストークン生成の動作を定義
        tokenGenerator
            .Setup(tg => tg.Generate(user.Id))
            .Returns(expectedAccessToken);
        // ユーザリポジトリからユーザ情報を受け取る時の動作を定義
        userRepository
            .Setup(repo => repo.TryFindUserByIdAsync(userId))
            .ReturnsAsync((true, user));
        // ハッシュ化したパスワードを保存済みパスワードと異なるものにする
        hasher
            .Setup(h => h.Generate(user, rawPassword))
            .Returns("異なるパスワード ");
        // ユーザリポジトリからパスワードを受け取る時の動作を定義
        authRepository
            .Setup(repo => repo.TryFindAuthenticationAsync(signInId))
            .ReturnsAsync((true, userId, storedPassword));

        // Act
        var (resultTypes, actualAccessToken) = await useCase.SignInAsync(userInputSignInId, userInputCurrentPassword);

        // Assert
        // アクセストークンを生成していないことを確認する。
        tokenGenerator.Verify(tg => tg.Generate(user.Id), Times.Never());
        // 戻り値のチェックを行う
        Assert.Equal(ResultTypes.ValidationError, resultTypes);
        Assert.NotEqual(expectedAccessToken, actualAccessToken);
    }
}
