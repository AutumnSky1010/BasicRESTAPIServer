using Server.Models.Users;

namespace ServerTests.Models.Users;
public class TestUserName
{
    [Theory(DisplayName = "ユーザ名のバリデーションの正常系")]
    [InlineData("1")]
    [InlineData("01234567890123456789012345678901234567890123456789")]
    public void ValidateUserName_Valid(string userNameValue)
    {
        var ok = UserName.TryCreate(userNameValue, out var userName);
        Assert.True(ok);
        Assert.Equal(userNameValue, userName?.Value);
    }

    [Theory(DisplayName = "ユーザ名のバリデーションの異常系")]
    [InlineData("")]
    [InlineData("012345678901234567890123456789012345678901234567890")]
    public void ValidateUserName_Invalid(string userNameValue)
    {
        var ok = UserName.TryCreate(userNameValue, out var userName);
        Assert.False(ok);
        Assert.Null(userName);
    }
}
