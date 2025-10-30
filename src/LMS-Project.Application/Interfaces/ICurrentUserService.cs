namespace LMS_Project.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
