using _Enums;
using DTOs;
using Entities;
using Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Services;

public class SolicitacaoService : ISolicitacaoService // Crie esta interface
    {
        private readonly ISolicitacaoRepository _solicitacaoRepository;
        private readonly IRepository<Cidadao> _cidadaoRepository;
        private readonly IRepository<HistoricoSolicitacao> _historicoRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<MembroEquipe> _userManager; // Para obter nomes de usuários

        public SolicitacaoService(ISolicitacaoRepository solicitacaoRepository,
                                  IRepository<Cidadao> cidadaoRepository,
                                  IRepository<HistoricoSolicitacao> historicoRepository,
                                  IMapper mapper,
                                  UserManager<MembroEquipe> userManager)
        {
            _solicitacaoRepository = solicitacaoRepository;
            _cidadaoRepository = cidadaoRepository;
            _historicoRepository = historicoRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<SolicitacaoDto> CreateSolicitacaoAsync(CreateSolicitacaoDto dto, Guid currentUserId)
        {
            Cidadao cidadao = null;
            if (dto.CidadaoId.HasValue)
            {
                cidadao = await _cidadaoRepository.GetByIdAsync(dto.CidadaoId.Value);
            }
            else if (!string.IsNullOrEmpty(dto.CidadaoEmail) || !string.IsNullOrEmpty(dto.CidadaoTelefone))
            {
                // Tenta encontrar cidadão existente ou cria um novo
                cidadao = await _cidadaoRepository.GetAllAsync() // Otimizar para busca por email/telefone
                                .ContinueWith(t => t.Result.FirstOrDefault(c =>
                                    (!string.IsNullOrEmpty(dto.CidadaoEmail) && c.Email == dto.CidadaoEmail) ||
                                    (!string.IsNullOrEmpty(dto.CidadaoTelefone) && c.Telefone == dto.CidadaoTelefone)
                                ));
                if (cidadao == null)
                {
                    cidadao = new Cidadao
                    {
                        Nome = dto.CidadaoNome,
                        Email = dto.CidadaoEmail,
                        Telefone = dto.CidadaoTelefone,
                        Endereco = dto.CidadaoEndereco,
                        Bairro = dto.CidadaoBairro,
                        CPF = dto.CidadaoCpf
                    };
                    await _cidadaoRepository.AddAsync(cidadao);
                    await _cidadaoRepository.SaveChangesAsync(); // Salvar cidadão antes de vincular
                }
            }
            
            var solicitacao = _mapper.Map<Solicitacao>(dto);
            solicitacao.Id = Guid.NewGuid(); // Garantir que tenha um GUID
            solicitacao.Protocolo = await GerarProximoProtocoloAsync();
            solicitacao.CidadaoId = cidadao?.Id;
            solicitacao.Status = StatusSolicitacao.Registrado;

            // Adicionar tags
            foreach (var tagString in dto.Tags)
            {
                solicitacao.Tags.Add(new TagSolicitacao { Id = Guid.NewGuid(), Tag = tagString });
            }

            await _solicitacaoRepository.AddAsync(solicitacao);
            await _solicitacaoRepository.SaveChangesAsync();

            // Registrar histórico
            await _historicoRepository.AddAsync(new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacao.Id,
                TipoEvento = TipoEventoHistorico.SolicitacaoCriada,
                Detalhes = $"Solicitação '{solicitacao.Protocolo}' criada.",
                UsuarioResponsavelId = currentUserId
            });
            await _historicoRepository.SaveChangesAsync();

            return _mapper.Map<SolicitacaoDto>(solicitacao);
        }

        public async Task<SolicitacaoDto> GetSolicitacaoByIdAsync(Guid id)
        {
            var solicitacao = await _solicitacaoRepository.GetByIdAsync(id);
            return _mapper.Map<SolicitacaoDto>(solicitacao);
        }

        public async Task<SolicitacaoDto> GetSolicitacaoByProtocoloAsync(string protocolo)
        {
            var solicitacao = await _solicitacaoRepository.GetByProtocoloAsync(protocolo);
            return _mapper.Map<SolicitacaoDto>(solicitacao);
        }

        public async Task<IEnumerable<SolicitacaoDto>> GetFilteredSolicitacoesAsync(
            CategoriaSolicitacao? categoria = null, StatusSolicitacao? status = null,
            PrioridadeSolicitacao? prioridade = null, Guid? membroAtribuidoId = null,
            string searchString = null, int pageNumber = 1, int pageSize = 10)
        {
            var solicitacoes = await _solicitacaoRepository.GetFilteredSolicitacoesAsync(
                categoria, status, prioridade, membroAtribuidoId, searchString, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<SolicitacaoDto>>(solicitacoes);
        }

        public async Task<bool> UpdateSolicitacaoAsync(Guid id, UpdateSolicitacaoDto dto, Guid currentUserId)
        {
            var solicitacao = await _solicitacaoRepository.GetByIdAsync(id);
            if (solicitacao == null) return false;

            // Mapeia e atualiza campos, cuidando para não sobrescrever o Protocolo, etc.
            var originalStatus = solicitacao.Status;
            var originalAtribuicao = solicitacao.MembroAtribuidoId;

            _mapper.Map(dto, solicitacao); // AutoMapper cuida da maior parte

            // Atualização de Tags
            solicitacao.Tags.Clear();
            foreach (var tagString in dto.Tags)
            {
                solicitacao.Tags.Add(new TagSolicitacao { Id = Guid.NewGuid(), SolicitacaoId = solicitacao.Id, Tag = tagString });
            }


            solicitacao.DataUltimaAtualizacao = DateTime.UtcNow;

            await _solicitacaoRepository.UpdateAsync(solicitacao);
            await _solicitacaoRepository.SaveChangesAsync();

            // Registrar histórico de mudanças
            if (originalStatus != solicitacao.Status)
            {
                await _historicoRepository.AddAsync(new HistoricoSolicitacao
                {
                    SolicitacaoId = solicitacao.Id,
                    TipoEvento = TipoEventoHistorico.StatusAlterado,
                    Detalhes = $"Status alterado de '{originalStatus}' para '{solicitacao.Status}'.",
                    UsuarioResponsavelId = currentUserId
                });
            }
            if (originalAtribuicao != solicitacao.MembroAtribuidoId)
            {
                var newAssignee = solicitacao.MembroAtribuidoId.HasValue ? (await _userManager.FindByIdAsync(solicitacao.MembroAtribuidoId.Value.ToString()))?.NomeCompleto : "Ninguém";
                 await _historicoRepository.AddAsync(new HistoricoSolicitacao
                {
                    SolicitacaoId = solicitacao.Id,
                    TipoEvento = TipoEventoHistorico.AtribuicaoAlterada,
                    Detalhes = $"Atribuída a: {newAssignee}.",
                    UsuarioResponsavelId = currentUserId
                });
            }
            await _historicoRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddObservacaoAsync(Guid solicitacaoId, AddObservacaoDto dto, Guid currentUserId)
        {
            var solicitacao = await _solicitacaoRepository.GetByIdAsync(solicitacaoId);
            if (solicitacao == null) return false;

            var observacao = new Observacao
            {
                Id = Guid.NewGuid(),
                SolicitacaoId = solicitacaoId,
                Conteudo = dto.Conteudo,
                AutorId = currentUserId,
                DataRegistro = DateTime.UtcNow
            };
            solicitacao.Observacoes.Add(observacao); // EF Core rastreia automaticamente

            solicitacao.DataUltimaAtualizacao = DateTime.UtcNow;
            await _solicitacaoRepository.UpdateAsync(solicitacao); // Ou apenas SaveChangesAsync()
            await _solicitacaoRepository.SaveChangesAsync();

            await _historicoRepository.AddAsync(new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacaoId,
                TipoEvento = TipoEventoHistorico.ObservacaoAdicionada,
                Detalhes = "Nova observação adicionada.",
                UsuarioResponsavelId = currentUserId
            });
            await _historicoRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddAnexoAsync(Guid solicitacaoId, Stream fileStream, string fileName, string contentType, Guid currentUserId)
        {
             var solicitacao = await _solicitacaoRepository.GetByIdAsync(solicitacaoId);
            if (solicitacao == null) return false;

            // Supondo que você tenha um IFileStorageService injetado
            // Ex: private readonly IFileStorageService _fileStorageService;
            // var fileUrl = await _fileStorageService.UploadFileAsync(fileStream, fileName, contentType);

            // Mock de URL para exemplo
            var fileUrl = $"https://seuservico.com/anexos/{Guid.NewGuid()}-{fileName}";

            solicitacao.Anexos.Add(new Anexo
            {
                Id = Guid.NewGuid(),
                SolicitacaoId = solicitacaoId,
                NomeArquivo = fileName,
                Url = fileUrl,
                TipoConteudo = contentType,
                DataUpload = DateTime.UtcNow,
                UploadedByUserId = currentUserId
            });
            solicitacao.DataUltimaAtualizacao = DateTime.UtcNow;

            await _solicitacaoRepository.UpdateAsync(solicitacao);
            await _solicitacaoRepository.SaveChangesAsync();

            await _historicoRepository.AddAsync(new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacaoId,
                TipoEvento = TipoEventoHistorico.AnexoAdicionado,
                Detalhes = $"Anexo '{fileName}' adicionado.",
                UsuarioResponsavelId = currentUserId
            });
            await _historicoRepository.SaveChangesAsync();

            return true;
        }


        public async Task<IEnumerable<HistoricoSolicitacaoDto>> GetSolicitacaoHistoricoAsync(Guid solicitacaoId)
        {
            var historico = await _solicitacaoRepository.GetHistoricoBySolicitacaoIdAsync(solicitacaoId);
            return _mapper.Map<IEnumerable<HistoricoSolicitacaoDto>>(historico);
        }

        private async Task<string> GerarProximoProtocoloAsync()
        {
            // Implementação simples de geração de protocolo VRYYYYXXX
            // Pode ser otimizado para evitar colisões e garantir sequencialidade em ambiente distribuído
            var ultimoProtocolo = (await _solicitacaoRepository.GetAllAsync()) // Otimizar para pegar o último apenas
                                    .OrderByDescending(s => s.DataRegistro)
                                    .Select(s => s.Protocolo)
                                    .FirstOrDefault();

            int numeroSequencial = 1;
            if (ultimoProtocolo != null && ultimoProtocolo.StartsWith("VR" + DateTime.Now.Year))
            {
                var parteNumerica = ultimoProtocolo.Substring(6); // Pega XXX da VRYYYYXXX
                if (int.TryParse(parteNumerica, out int parsedNum))
                {
                    numeroSequencial = parsedNum + 1;
                }
            }
            return $"VR{DateTime.Now.Year}{numeroSequencial:D3}";
        }
    }