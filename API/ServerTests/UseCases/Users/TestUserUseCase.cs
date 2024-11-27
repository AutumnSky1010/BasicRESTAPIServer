using Moq;
using Server.Models.UserAuthentications;
using Server.Models.Users;
using Server.UseCases;
using Server.UseCases.UserAuthentications;
using Server.UseCases.Users;

namespace ServerTests.UseCases.Users;
public class TestUserUseCase
{
    [Theory(DisplayName = "ユーザを挿入できるかの正常系テスト")]
    [InlineData("user", "aiueo@test.co.jp", "          ")]
    public async Task RegisterUser_Valid(string userName, string signInId, string password)
    {
        // Arrange
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var userRepository = new Mock<IUserRepository>();
        var authRepository = new Mock<IUserAuthenticationRepository>();
        var hashedPassword = "hashed";
        if (!RawPassword.TryCreate(password, out var rawPassword) ||
            !UserName.TryCreate(userName, out var expectedUserName) ||
            !SignInId.TryCreate(signInId, out var expectedSignInId))
        {
            Assert.Fail($"インラインデータが不正です。{userName},{signInId},{password}");
            return;
        }
        // authRepositoryが値を返さないように設定する
        authRepository
            .Setup(r => r.TryFindAuthenticationAsync(expectedSignInId))
            .ReturnsAsync((false, new(), StoredPassword.Empty));
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var user = User.Create(userId, expectedUserName, registeredAt);
        // ハッシュ化したパスワードを定義
        hasher
            .Setup(h => h.Generate(user, rawPassword))
            .Returns(hashedPassword);
        var expectedHashedPassword = HashedPassword.HashFromRawPassword(hasher.Object, rawPassword, user);
        // リポジトリのユーザ作成メソッドは成功を返すように設定
        userRepository
            .Setup(r => r.TryCreateUserAsync(user, expectedSignInId, expectedHashedPassword))
            .ReturnsAsync(true);
        var useCase = new UserUseCase(userRepository.Object, authRepository.Object, hasher.Object);

        // Act
        var (resultTypes, validationResult) = await useCase.RegisterUserAsync(userId, userName, registeredAt, signInId, password);

        // Assert
        // パスワードハッシュ化メソッドを呼び出しているかをテストする。
        hasher.Verify(hasher => hasher.Generate(user, rawPassword!));
        // リポジトリのユーザ作成メソッドが呼び出されている。
        userRepository.Verify(repo =>
            repo.TryCreateUserAsync(user, expectedSignInId, expectedHashedPassword),
            Times.Once());
        // ユースケースの戻り値で成功した結果が返っているか
        Assert.Equal(ResultTypes.Success, resultTypes);
        // バリデーション結果が正しいか
        Assert.Equal(new UserUseCase.RegisterValidationResult(true, true, true), validationResult);
    }

    [Theory(DisplayName = "ユーザを挿入できるかの異常系テスト(具体的に不正と判断する基準はモデルのテストで行う)")]
    // サインインIDが不正
    [InlineData("user", "", "          ", true, false, true)]
    // ユーザ名が不正
    [InlineData("", "aiueo@test.com", "          ", false, true, true)]
    // パスワードが不正
    [InlineData("user", "aiueo@test.com", "123456789", true, true, false)]
    public async Task RegisterUser_Invalid(string userName, string signInId, string password,
        bool expectedUserNameOk, bool expectedSignInIdOk, bool expectedPasswordOk)
    {
        // Arrange
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var userRepository = new Mock<IUserRepository>();
        var authRepository = new Mock<IUserAuthenticationRepository>();

        // authRepositoryが値を返さないように設定する
        authRepository
            .Setup(r => r.TryFindAuthenticationAsync(It.IsAny<SignInId>()))
            .ReturnsAsync((false, new(), StoredPassword.Empty));
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var useCase = new UserUseCase(userRepository.Object, authRepository.Object, hasher.Object);

        // Act
        var (resultTypes, validationResult) = await useCase.RegisterUserAsync(userId, userName, registeredAt, signInId, password);

        // Assert
        // リポジトリのユーザ作成メソッドが呼び出されないことを確認
        userRepository.Verify(repo =>
            repo.TryCreateUserAsync(It.IsAny<User>(), It.IsAny<SignInId>(), It.IsAny<HashedPassword>()),
            Times.Never());
        // ユースケースの結果がバリデーションエラーとなっているかを確認する
        Assert.Equal(ResultTypes.ValidationError, resultTypes);
        // バリデーション結果が期待通りかを確認する
        Assert.Equal(new UserUseCase.RegisterValidationResult(expectedUserNameOk, expectedSignInIdOk, expectedPasswordOk), validationResult);
    }

    [Fact(DisplayName = "サインインIDが既に存在する場合バリデーションエラーが戻るか")]
    public async Task TestRegisterUser_ExistsSignInId()
    {
        // Arrange
        var userName = "user";
        var signInId = "aiueo@test.com";
        var password = "1234567890";
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var userRepository = new Mock<IUserRepository>();
        var authRepository = new Mock<IUserAuthenticationRepository>();
        // authRepositoryが値を返すように設定する
        authRepository
            .Setup(r => r.TryFindAuthenticationAsync(It.IsAny<SignInId>()))
            .ReturnsAsync((true, new(), StoredPassword.Empty));
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var useCase = new UserUseCase(userRepository.Object, authRepository.Object, hasher.Object);

        // Act
        var (resultTypes, validationResult) = await useCase.RegisterUserAsync(userId, userName, registeredAt, signInId, password);

        // Assert
        // リポジトリのユーザ作成メソッドが呼び出されないことを確認
        userRepository.Verify(repo =>
            repo.TryCreateUserAsync(It.IsAny<User>(), It.IsAny<SignInId>(), It.IsAny<HashedPassword>()),
            Times.Never());
        // ユースケースの結果がバリデーションエラーとなっているかを確認する
        Assert.Equal(ResultTypes.ValidationError, resultTypes);
        // バリデーション結果が期待通りかを確認する
        Assert.Equal(new UserUseCase.RegisterValidationResult(true, false, true), validationResult);
    }

    [Fact(DisplayName = "ユーザ作成リポジトリで失敗した場合内部エラーとなるか")]
    public async Task InsertUser_InternalInvalid()
    {
        // Arrange
        var userName = "user";
        var signInId = "aiueo@test.com";
        var password = "1234567890";
        var hasher = new Mock<IHasher>();
        var tokenGenerator = new Mock<IAccessTokenGenerator>();
        var userRepository = new Mock<IUserRepository>();
        var authRepository = new Mock<IUserAuthenticationRepository>();
        var repository = new Mock<IUserRepository>();
        // TryCreateUserに失敗させる。
        repository.Setup(repo => repo.TryCreateUserAsync(
            It.IsAny<User>(),
            It.IsAny<SignInId>(),
            It.IsAny<HashedPassword>()))
            .ReturnsAsync(() => false);
        var userId = UserId.CreateNew();
        var registeredAt = DateTime.Now;
        var useCase = new UserUseCase(userRepository.Object, authRepository.Object, hasher.Object);

        // Act
        var (resultTypes, _) = await useCase.RegisterUserAsync(userId, userName, registeredAt, signInId, password);

        // Assert
        Assert.Equal(ResultTypes.InternalError, resultTypes);
    }
}
