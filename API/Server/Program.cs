using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server;
using Server.Authentications;
using Server.Databases;
using Server.Models.UserAuthentications;
using Server.UseCases.UserAuthentications;
using Server.UseCases.Users;

internal class Program
{
    public const string REGISTER_USER_RATE_LIMITER = "register";

    public const string TOKEN_RATE_LIMITER = "token";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers(
            // デフォルトで認証が必要にする、
            // https://qiita.com/mkuwan/items/bd5ff882108998d76dca
            options => options.Filters.Add(
                new AuthorizeFilter(
                    new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build()
                )
            )
        );
        // ベアラートークン認証関連の登録
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = ServerEnvironments.Get(VariableTypes.JWTIssuer),
                    ValidAudience = ServerEnvironments.Get(VariableTypes.JWTAudience),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            ServerEnvironments.Get(VariableTypes.JWTKeyForAccessToken))),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(option =>
        {
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWTトークンを入力してください。",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type=ReferenceType.SecurityScheme, Id="Bearer"
                        }
                    },
                    []
                }
            });
        });

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

        // 認証
        app.UseAuthentication();
        // 認可
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
            })
            .AddFixedWindowLimiter(TOKEN_RATE_LIMITER, options =>
            {
                options.PermitLimit = 3;
                options.Window = TimeSpan.FromSeconds(60);
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
        // トークン生成には機密情報を使うので、ここでのみ生成を行う。
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
                var userAuthRepository = services.GetRequiredService<IUserAuthenticationRepository>();
                var hasher = services.GetRequiredService<IHasher>();
                return new UserUseCase(userRepository, userAuthRepository, hasher);
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