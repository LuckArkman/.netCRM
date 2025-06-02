using DTOs;

namespace Interfaces;

public interface IAgendaService
{
    Task<CompromissoDto> CreateCompromissoAsync(CreateCompromissoDto dto, Guid currentUserId);
    Task<CompromissoDto> GetCompromissoByIdAsync(Guid id);
    Task<IEnumerable<CompromissoDto>> GetCompromissosByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? responsavelId = null);
    Task<bool> UpdateCompromissoAsync(Guid id, UpdateCompromissoDto dto, Guid currentUserId);
    Task<bool> DeleteCompromissoAsync(Guid id);
}