using LMS_Project.Application.DTOs;
using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using MediatR;

namespace LMS_Project.Application.Features.Auth.Commands;

public record RegisterCommand(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<Result>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequestDto
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        return await _authService.RegisterAsync(registerRequest, cancellationToken);
    }
}
