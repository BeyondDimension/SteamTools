// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed class AcceleratorAutoMapperProfile : Profile
{
    public AcceleratorAutoMapperProfile()
    {
        CreateMap<ScriptDTO, Script>()
            .ForMember(x => x.Pid, d => d.MapFrom(x => x.Id))
            .ForMember(x => x.Id, d => d.MapFrom(x => x.LocalId));
        CreateMap<Script, ScriptDTO>()
            .ForMember(x => x.LocalId, d => d.MapFrom(x => x.Id))
            .ForMember(x => x.Id, d => d.MapFrom(x => x.Pid));
    }
}