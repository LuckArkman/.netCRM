using Data;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Repositories;

public class CompromissoRepository : Repository<Compromisso>, ICompromissoRepository
{
    public CompromissoRepository(GabineteDigitalDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Compromisso> GetByIdAsync(Guid id)
    {
        return await _dbContext.Compromissos
            .Include(c => c.Cidadao)
            .Include(c => c.Responsavel)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Compromisso>> GetCompromissosComDetalhesAsync(DateTime startDate, DateTime endDate, Guid? responsavelId = null)
    {
        IQueryable<Compromisso> query = _dbContext.Compromissos
            .Include(c => c.Cidadao)
            .Include(c => c.Responsavel)
            .Where(c => c.DataHoraInicio >= startDate && c.DataHoraInicio <= endDate);

        if (responsavelId.HasValue)
        {
            query = query.Where(c => c.ResponsavelId == responsavelId.Value);
        }

        return await query.OrderBy(c => c.DataHoraInicio).ToListAsync();
    }
}