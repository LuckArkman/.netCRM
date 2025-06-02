using _Enums;
using DTOs;
using Entities;
using Interfaces;

namespace Services;

public class AgendaService : IAgendaService
    {
        private readonly IRepository<Compromisso> _compromissoRepository;
        private readonly IRepository<Cidadao> _cidadaoRepository;
        private readonly IMapper _mapper;

        public AgendaService(IRepository<Compromisso> compromissoRepository,
                             IRepository<Cidadao> cidadaoRepository,
                             IMapper mapper)
        {
            _compromissoRepository = compromissoRepository;
            _cidadaoRepository = cidadaoRepository;
            _mapper = mapper;
        }

        public async Task<CompromissoDto> CreateCompromissoAsync(CreateCompromissoDto dto, Guid currentUserId)
        {
            // Lógica para encontrar ou criar cidadão similar à de SolicitacaoService
            Cidadao cidadao = null;
            if (dto.CidadaoId.HasValue)
            {
                cidadao = await _cidadaoRepository.GetByIdAsync(dto.CidadaoId.Value);
            }
            else if (!string.IsNullOrEmpty(dto.CidadaoNome))
            {
                // Aqui você pode adicionar lógica para buscar por nome ou criar um novo cidadão simples
                cidadao = new Cidadao { Nome = dto.CidadaoNome }; // Apenas um exemplo simplificado
                await _cidadaoRepository.AddAsync(cidadao);
                await _cidadaoRepository.SaveChangesAsync();
            }

            var compromisso = _mapper.Map<Compromisso>(dto);
            compromisso.Id = Guid.NewGuid();
            compromisso.CidadaoId = cidada?.Id;
            compromisso.ResponsavelId = dto.ResponsavelId ?? currentUserId; // Atribui ao criador se não especificado
            compromisso.Status = StatusCompromisso.Pendente;

            await _compromissoRepository.AddAsync(compromisso);
            await _compromissoRepository.SaveChangesAsync();

            return _mapper.Map<CompromissoDto>(compromisso);
        }

        public async Task<CompromissoDto> GetCompromissoByIdAsync(Guid id)
        {
            var compromisso = await _compromissoRepository.GetByIdAsync(id);
            return _mapper.Map<CompromissoDto>(compromisso);
        }

        public async Task<IEnumerable<CompromissoDto>> GetCompromissosByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? responsavelId = null)
        {
            var allCompromissos = await _compromissoRepository.GetAllAsync(); // Idealmente, filtre no repo
            var filteredCompromissos = allCompromissos
                .Where(c => c.DataHoraInicio >= startDate && c.DataHoraInicio <= endDate &&
                            (!responsavelId.HasValue || c.ResponsavelId == responsavelId.Value));

            return _mapper.Map<IEnumerable<CompromissoDto>>(filteredCompromissos);
        }

        public async Task<bool> UpdateCompromissoAsync(Guid id, UpdateCompromissoDto dto, Guid currentUserId)
        {
            var compromisso = await _compromissoRepository.GetByIdAsync(id);
            if (compromisso == null) return false;

            _mapper.Map(dto, compromisso);
            // Atualizar status, responsável, etc.

            await _compromissoRepository.UpdateAsync(compromisso);
            await _compromissoRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCompromissoAsync(Guid id)
        {
            var compromisso = await _compromissoRepository.GetByIdAsync(id);
            if (compromisso == null) return false;

            await _compromissoRepository.DeleteAsync(compromisso);
            await _compromissoRepository.SaveChangesAsync();
            return true;
        }
    }