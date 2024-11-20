using Microsoft.AspNetCore.Identity;
using Server;
using Server.Authentications;
using Server.Databases;
using Server.UseCases.Users;

internal class Program
{
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

        app.Run();
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