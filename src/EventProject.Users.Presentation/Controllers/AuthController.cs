using EventProject.Users.Application.Abstractions.Services;
using EventProject.Users.Application.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Users.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public class AuthController(
    IAuthService authService
) : ControllerBase
{
    /// <summary>
    /// Регистрация пользователя.
    /// </summary>
    /// <param name="request">Данные для регистрации.</param>
    /// <response code="204">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Ошибка регистрации пользователя.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct = default)
    {
        await authService.RegisterUser(request, ct);
        return NoContent();
    }

    /// <summary>
    /// Авторизация пользователя.
    /// При успешной авторизации возвращает JWT-токен, 
    /// который необходимо использовать для доступа к защищенным ресурсам сервиса.
    /// </summary>
    /// <param name="request">Данные пользователя для авторизации.</param>
    /// <response code="200">Пользователь успешно авторизован.</response>
    /// <response code="404">Пользователь не найден или данные авторизации не верные.</response>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginUserRequest request, CancellationToken ct = default)
    {
        var token = await authService.LoginUser(request, ct);
        return Ok(token);
    }
}