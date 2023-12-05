// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed class AcceleratorAutoMapperProfile : Profile
{
    public AcceleratorAutoMapperProfile()
    {
        CreateMap<ScriptDTO, Script>()
            .ForMember(x => x.Pid, d => d.MapFrom(x => x.Id))
            .ForMember(x => x.Author, d => d.MapFrom(x => x.AuthorName))
            .ForMember(x => x.Icon, d => d.MapFrom(x => x.IconUrl))
            .ForMember(x => x.Description, d => d.MapFrom(x => x.Describe))
            .ForMember(x => x.Enable, d => d.MapFrom(x => !x.Disable))
            .ForMember(x => x.IsBuild, d => d.MapFrom(x => x.IsCompile))
            .ForMember(x => x.Id, d => d.MapFrom(x => x.LocalId));
        CreateMap<Script, ScriptDTO>()
            .ForMember(x => x.LocalId, d => d.MapFrom(x => x.Id))
            .ForMember(x => x.AuthorName, d => d.MapFrom(x => x.Author))
            .ForMember(x => x.Describe, d => d.MapFrom(x => x.Description))
            .ForMember(x => x.IconUrl, d => d.MapFrom(x => x.Icon))
            .ForMember(x => x.Disable, d => d.MapFrom(x => !x.Enable))
            .ForMember(x => x.IsCompile, d => d.MapFrom(x => x.IsBuild))
            .ForMember(x => x.Id, d => d.MapFrom(x => x.Pid));
    }
}