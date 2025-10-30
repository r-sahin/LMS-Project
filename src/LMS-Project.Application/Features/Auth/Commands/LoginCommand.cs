using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;

namespace LMS_Project.Application.Features.Auth.Commands;

public record LoginCommand(string UserNameOrEmail, string Password) : IRequest<Result<LoginResponseDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequestDto
        {
            UserNameOrEmail = request.UserNameOrEmail,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}
