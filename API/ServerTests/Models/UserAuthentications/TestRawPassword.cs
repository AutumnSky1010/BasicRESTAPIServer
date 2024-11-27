using Server.Models.UserAuthentications;

namespace ServerTests.Models.UserAuthentications;
public class TestRawPassword
{
    [Theory(DisplayName = "パスワードのバリデーションの正常系")]
    [InlineData("1234567890")]
    public void ValidatePassword_Valid(string passwordValue)
    {
        var ok = RawPassword.TryCreate(passwordValue, out var rawPassword);
        Assert.True(ok);
        Assert.Equal(passwordValue, rawPassword?.Value);
    }

    [Theory(DisplayName = "パスワードのバリデーションの異常系")]
    [InlineData("123456789")]
    public void ValidatePassword_Invalid(string passwordValue)
    {
        var ok = RawPassword.TryCreate(passwordValue, out var rawPassword);
        Assert.False(ok);
        Assert.Null(rawPassword?.Value);
    }
}
