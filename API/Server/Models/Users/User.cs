using System.Diagnostics.CodeAnalysis;

namespace Server.Models.Users;

public readonly record struct UserId(Guid Value)
{
    public static readonly UserId Empty = new(Guid.Empty);
}

public class User
{
    private User(UserId id, UserName name, DateTime registeredAt)
    {
        Id = id;
        Name = name;
        RegisteredAt = registeredAt;
    }

    public static readonly User Unknown = new(UserId.Empty, UserName.Empty, DateTime.MinValue);

    public UserId Id { get; }

    public UserName Name { get; }

    public DateTime RegisteredAt { get; }

    /// <summary>
    /// 既存の情報からユーザを作成する
    /// </summary>
    /// <param name="id">ユーザID</param>
    /// <param name="userName">ユーザ名</param>
    /// <param name="registeredAt">登録日時</param>
    /// <returns>既存の情報から作成したユーザ</returns>
    public static User CreateFrom(UserId id, UserName userName, DateTime registeredAt)
    {
        return new(id, userName, registeredAt);
    }

    /// <summary>
    /// ユーザを新規作成する
    /// </summary>
    /// <param name="userName">ユーザ名</param>
    /// <returns>新規ユーザ</returns>
    public static User CreateNew(UserName userName)
    {
        return new(new(Guid.NewGuid()), userName, DateTime.Now);
    }

    public override bool Equals(object? obj)
    {
        if (obj is User user)
        {
            return user.Id == Id;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}