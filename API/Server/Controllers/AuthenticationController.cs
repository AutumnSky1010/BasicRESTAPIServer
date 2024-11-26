using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Server.UseCases;
using Server.UseCases.UserAuthentications;

namespace Server.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController(ILogger<AuthenticationController> logger, UserAuthenticationUseCase authUseCase) : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger = logger;
    private readonly UserAuthenticationUseCase _authUseCase = authUseCase;

    public record SignInParams(string SignInId, string Password);
    public record SignInResponse(string AccessToken);
    [AllowAnonymous]
    [EnableRateLimiting(Program.TOKEN_RATE_LIMITER)]
    [HttpPost("sign_in")]
    public async Task<IActionResult> RegisterAsync(SignInParams param)
    {
        // サインインユースケースを実行
        var (resultTypes, accessToken) = await _authUseCase.SignInAsync(param.SignInId, param.Password);
        if (resultTypes is ResultTypes.Success)
        {
            return Ok(new SignInResponse(accessToken));
        }

        if (resultTypes is ResultTypes.InternalError)
        {
            _logger.LogError("サインイン時にサーバ内エラーが発生しました");
            return StatusCode(500, "Internal Server Error");
        }

        return BadRequest();
    }
}
