using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Server.Authentications;
using Server.Models.Users;
using Server.UseCases;
using Server.UseCases.Users;

namespace Server.Controllers;

[ApiController]
[Route("users")]
public class UserController(ILogger<UserController> logger, UserUseCase userUseCase) : ControllerBase
{
    private readonly ILogger<UserController> _logger = logger;
    private readonly UserUseCase _userUseCase = userUseCase;

    public record RegisterParams(string Name, string SignInId, string Password);
    public record RegisterValidationResponse(bool UserNameOk, bool SignInIdOk, bool RawPasswordOk);
    [AllowAnonymous]
    [EnableRateLimiting(Program.REGISTER_USER_RATE_LIMITER)]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterParams param)
    {
        // 登録ユースケースを実行
        var (resultTypes, validationResult) = await _userUseCase.RegisterUserAsync(UserId.CreateNew(), param.Name, DateTime.Now, param.SignInId, param.Password);
        if (resultTypes is ResultTypes.Success)
        {
            return Ok();
        }

        if (resultTypes is ResultTypes.InternalError)
        {
            _logger.LogError("ユーザ登録時にサーバ内エラーが発生しました");
            return StatusCode(500, "Internal Server Error");
        }

        var validationResultResponse = new RegisterValidationResponse(validationResult.UserNameOk, validationResult.SignInIdOk, validationResult.RawPasswordOk);
        return BadRequest(validationResultResponse);
    }

    public record GetUserResponse(string Name);
    [HttpGet]
    public async Task<IActionResult> GetUserAsync()
    {
        var userId = AccessTokenGenerator.ParseUserId(Request);
        var (resultTypes, user) = await _userUseCase.GetUserAsync(userId);

        if (resultTypes is ResultTypes.Success)
        {
            return Ok(new GetUserResponse(user.Name.Value));
        }

        _logger.LogError("ユーザ取得時にサーバ内エラーが発生しました");
        return StatusCode(500, "Internal Server Error");
    }
}
