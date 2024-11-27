using Server.Models.UserAuthentications;

namespace ServerTests.Models.UserAuthentications;
public class TestSignInId
{
    [Theory(DisplayName = "サインインIDのバリデーションの正常系")]
    // 10文字
    [InlineData("0123456789")]
    // 100文字
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public void ValidateSignInId_Valid(string signInIdValue)
    {
        var ok = SignInId.TryCreate(signInIdValue, out var signInId);
        Assert.True(ok);
        Assert.Equal(signInIdValue, signInId?.Value);
    }

    [Theory(DisplayName = "サインインIDのバリデーションの異常系")]
    // 9文字
    [InlineData("012345678")]
    // 100文字 + 1
    [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public void ValidateSignInId_InvalidFormat(string signInIdValue)
    {
        var ok = SignInId.TryCreate(signInIdValue, out var signInId);
        Assert.False(ok);
        Assert.Null(signInId);
    }
}
