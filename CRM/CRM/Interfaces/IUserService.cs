using DTOs;
using Entities;
using Microsoft.AspNetCore.Identity;

namespace Interfaces
{
    public interface IUserService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RegisterMembroEquipeAsync(RegisterMembroDto registerDto);
        Task<IEnumerable<MembroEquipeDto>> GetAllMembrosEquipeAsync();
        Task<MembroEquipeDto> GetMembroEquipeByIdAsync(Guid id);
        Task<bool> UpdateMembroEquipeAsync(Guid id, UpdateMembroDto updateDto);
        Task<bool> AssignRoleToMembroAsync(Guid userId, string roleName);
        Task<bool> RemoveRoleFromMembroAsync(Guid userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    }
}