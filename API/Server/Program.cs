using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Server;
using Server.Authentications;
using Server.Databases;
using Server.Models.UserAuthentications;
using Server.UseCases.UserAuthentications;
using Server.UseCases.Users;

internal class Program
{
    public const string REGISTER_USER_RATE_LIMITER = "register";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // カラム名のアンダースコアを無視する。こうすることでキャメルケースでもマップできる。
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Hasherを登録
        RegisterHasher(builder.Services);

        // トークン生成器を登録
        RegisterTokenGenerator(builder.Services);

        // リポジトリを登録
        RegisterRepositories(builder.Services);

        // ユースケースを登録
        RegisterUseCases(builder.Services);

        // レート制限を設定
        RegisterRateLimit(builder.Services);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        // レート制限ミドルウェアを開始
        app.UseRateLimiter();

        app.Run();
    }

    /// <summary>
    /// レート制限をかけるための設定を行う
    /// </summary>
    /// <param name="services">サービス</param>
    private static void RegisterRateLimit(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(REGISTER_USER_RATE_LIMITER, options =>
            {
                // 時間枠内で受け取れるリクエスト数
                options.PermitLimit = 1;
                // リクエストを受け取る時間枠
                options.Window = TimeSpan.FromSeconds(60);
                // リクエストのキューの容量
                options.QueueLimit = 0;
            });
        });
    }

    /// <summary>
    /// ハッシュ化処理を登録
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    private static void RegisterTokenGenerator(IServiceCollection services)
    {
        services
            .AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>(services =>
            {
                var audience = ServerEnvironments.Get(VariableTypes.JWTAudience);
                var issuer = ServerEnvironments.Get(VariableTypes.JWTIssuer);
                var key = ServerEnvironments.Get(VariableTypes.JWTKeyForAccessToken);

                return new(key, issuer, audience);
            });
    }

    /// <summary>
    /// ハッシュ化処理を登録
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    private static void RegisterHasher(IServiceCollection services)
    {
        services
            .AddSingleton<IHasher, HasherByPBKDF2>(services => new(ServerEnvironments.Get(VariableTypes.HasherPepper)));
    }

    /// <summary>
    /// リポジトリを登録する
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    private static void RegisterRepositories(IServiceCollection services)
    {
        var connectionString = ServerEnvironments.Get(VariableTypes.DBConnectionString);
        services
            .AddSingleton<IUserRepository, UserRepository>(services => new(services.GetRequiredService<ILogger<UserRepository>>(), connectionString))
            .AddSingleton<IUserAuthenticationRepository, UserAuthenticationRepository>(
                    services => new(services.GetRequiredService<ILogger<UserAuthenticationRepository>>(), connectionString)
                );
    }

    /// <summary>
    /// DIコンテナにユースケースを登録する。
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    private static void RegisterUseCases(IServiceCollection services)
    {
        services
            .AddSingleton(services =>
            {
                var userRepository = services.GetRequiredService<IUserRepository>();
                var hasher = services.GetRequiredService<IHasher>();
                return new UserUseCase(userRepository, hasher);
            })
            .AddSingleton(services =>
            {
                var userRepository = services.GetRequiredService<IUserRepository>();
                var hasher = services.GetRequiredService<IHasher>();
                var tokenGenerator = services.GetRequiredService<IAccessTokenGenerator>();
                var userAuthRepository = services.GetRequiredService<IUserAuthenticationRepository>();
                return new UserAuthenticationUseCase(hasher, tokenGenerator, userAuthRepository, userRepository);
            });
    }
}