﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterParams param)
    {
        // 登録ユースケースを実行
        var (resultTypes, validationResult) = await _userUseCase.RegisterUserAsync(param.Name, param.SignInId, param.Password);
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
}