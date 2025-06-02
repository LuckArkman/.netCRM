using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DTOs;
using Entities;
using GabineteDigital.Application.Interfaces;
using Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Services;

public class UserService : IUserService
    {
        private readonly UserManager<MembroEquipe> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(UserManager<MembroEquipe> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager,
                           IConfiguration configuration,
                           IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResultDto { IsSuccess = false, Errors = new List<string> { "Credenciais inválidas." } };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles.ToList());

            return new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"])),
                UserId = user.Id.ToString(),
                Email = user.Email,
                UserName = user.UserName,
                Roles = userRoles.ToList(),
                IsSuccess = true
            };
        }

        public async Task<AuthResultDto> RegisterMembroEquipeAsync(RegisterMembroDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResultDto { IsSuccess = false, Errors = new List<string> { "Email já registrado." } };
            }

            var newUser = _mapper.Map<MembroEquipe>(registerDto);
            newUser.Id = Guid.NewGuid(); // Garantir que o GUID seja gerado

            var createResult = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!createResult.Succeeded)
            {
                return new AuthResultDto { IsSuccess = false, Errors = createResult.Errors.Select(e => e.Description).ToList() };
            }

            foreach (var role in registerDto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role)); // Cria role se não existir
                }
                await _userManager.AddToRoleAsync(newUser, role);
            }

            var userRoles = await _userManager.GetRolesAsync(newUser);
            var token = GenerateJwtToken(newUser, userRoles.ToList());

            return new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"])),
                UserId = newUser.Id.ToString(),
                Email = newUser.Email,
                UserName = newUser.UserName,
                Roles = userRoles.ToList(),
                IsSuccess = true
            };
        }

        public async Task<IEnumerable<MembroEquipeDto>> GetAllMembrosEquipeAsync()
        {
            var membros = _userManager.Users.ToList();
            var membroDtos = _mapper.Map<List<MembroEquipeDto>>(membros);

            foreach (var dto in membroDtos)
            {
                var user = await _userManager.FindByIdAsync(dto.Id.ToString());
                if (user != null)
                {
                    dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                }
            }
            return membroDtos;
        }

        public async Task<MembroEquipeDto> GetMembroEquipeByIdAsync(Guid id)
        {
            var membro = await _userManager.FindByIdAsync(id.ToString());
            if (membro == null) return null;

            var dto = _mapper.Map<MembroEquipeDto>(membro);
            dto.Roles = (await _userManager.GetRolesAsync(membro)).ToList();
            return dto;
        }

        public async Task<bool> UpdateMembroEquipeAsync(Guid id, UpdateMembroDto updateDto)
        {
            var membro = await _userManager.FindByIdAsync(id.ToString());
            if (membro == null) return false;

            _mapper.Map(updateDto, membro); // Mapeia campos atualizáveis

            // Se o email for alterado, também atualize o UserName (se eles forem o mesmo)
            if (membro.Email != updateDto.Email)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(membro, updateDto.Email);
                if (!setUsernameResult.Succeeded) return false;
            }

            var result = await _userManager.UpdateAsync(membro);
            if (!result.Succeeded) return false;

            // Atualiza roles
            var currentRoles = await _userManager.GetRolesAsync(membro);
            var rolesToAdd = updateDto.Roles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(updateDto.Roles);

            await _userManager.AddToRolesAsync(membro, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(membro, rolesToRemove);

            return true;
        }

        public async Task<bool> AssignRoleToMembroAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleFromMembroAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Enumerable.Empty<string>();
            return await _userManager.GetRolesAsync(user);
        }

        private string GenerateJwtToken(MembroEquipe user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName), // Use UserName que pode ser o Email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            };

            // Adicionar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }