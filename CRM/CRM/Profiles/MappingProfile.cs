using DTOs;
using Entities;

namespace CRM.Profiles;

public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Solicitacao Mappings
            CreateMap<Solicitacao, SolicitacaoDto>()
                .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria.ToString()))
                .ForMember(dest => dest.Prioridade, opt => opt.MapFrom(src => src.Prioridade.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.NomeCidadao, opt => opt.MapFrom(src => src.Cidadao.Nome))
                .ForMember(dest => dest.NomeMembroAtribuido, opt => opt.MapFrom(src => src.MembroAtribuido.NomeCompleto))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Tag).ToList()));
            CreateMap<CreateSolicitacaoDto, Solicitacao>();
            CreateMap<UpdateSolicitacaoDto, Solicitacao>()
                .ForMember(dest => dest.Protocolo, opt => opt.Ignore()) // Não permitir atualização do protocolo
                .ForMember(dest => dest.DataRegistro, opt => opt.Ignore()) // Não permitir atualização da data de registro
                .ForMember(dest => dest.Cidadao, opt => opt.Ignore()) // Cidadao é tratado separadamente
                .ForMember(dest => dest.CidadaoId, opt => opt.Ignore())
                .ForMember(dest => dest.Anexos, opt => opt.Ignore())
                .ForMember(dest => dest.Observacoes, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore()); // Tags são tratadas separadamente

            // Anexo Mappings
            CreateMap<Anexo, AnexoDto>();

            // Observacao Mappings
            CreateMap<Observacao, ObservacaoDto>()
                .ForMember(dest => dest.AutorNome, opt => opt.MapFrom(src => src.Autor.NomeCompleto));

            // HistoricoSolicitacao Mappings
            CreateMap<HistoricoSolicitacao, HistoricoSolicitacaoDto>()
                .ForMember(dest => dest.TipoEvento, opt => opt.MapFrom(src => src.TipoEvento.ToString()))
                .ForMember(dest => dest.UsuarioResponsavelNome, opt => opt.MapFrom(src => src.UsuarioResponsavel.NomeCompleto));

            // MembroEquipe Mappings
            CreateMap<MembroEquipe, MembroEquipeDto>();
            CreateMap<RegisterMembroDto, MembroEquipe>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)); // Username é o Email
            CreateMap<UpdateMembroDto, MembroEquipe>();

            // Compromisso Mappings
            CreateMap<Compromisso, CompromissoDto>()
                .ForMember(dest => dest.TipoAtendimento, opt => opt.MapFrom(src => src.TipoAtendimento.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.NomeCidadao, opt => opt.MapFrom(src => src.Cidadao.Nome))
                .ForMember(dest => dest.NomeResponsavel, opt => opt.MapFrom(src => src.Responsavel.NomeCompleto));
            CreateMap<CreateCompromissoDto, Compromisso>();
            CreateMap<UpdateCompromissoDto, Compromisso>();
        }
    }