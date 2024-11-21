using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Server;
using Server.Authentications;
using Server.Databases;
using Server.UseCases.Users;

internal class Program
{
    public const string SIGN_RATE_LIMITER = "sign";

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
            options.AddFixedWindowLimiter(SIGN_RATE_LIMITER, options =>
            {
                options.PermitLimit = 3;
                options.Window = TimeSpan.FromSeconds(12);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });
        });
    }

    /// <summary>
    /// DIコンテナにユースケースを登録する。
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    private static void RegisterUseCases(IServiceCollection services)
    {
        services.AddSingleton(services =>
        {
            var userRepository = new UserRepository(services.GetRequiredService<ILogger<UserRepository>>(), ServerEnvironments.Get(VariableTypes.DBConnectionString));
            var hasher = new HasherByPBKDF2(ServerEnvironments.Get(VariableTypes.HasherPepper));
            return new UserUseCase(userRepository, hasher);
        });
    }
}