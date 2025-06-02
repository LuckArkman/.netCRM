using Entities;

namespace Interfaces;

public interface ICompromissoRepository : IRepository<Compromisso>
{
    Task<IEnumerable<Compromisso>> GetCompromissosComDetalhesAsync(DateTime startDate, DateTime endDate, Guid? responsavelId = null);
}