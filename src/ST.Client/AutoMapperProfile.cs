using AutoMapper;
using System.Application.Entities;
using System.Application.Models;

namespace System.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ScriptDTO, Script>() 
                .ForMember(x=>x.Pid,d=>d.MapFrom(x=>x.Id))
                .ForMember(x=>x.Id,d=>d.Ignore());
            CreateMap<Script, ScriptDTO>()
                .ForMember(x => x.LocalId, d => d.MapFrom(x => x.Id))
                .ForMember(x => x.Id, d => d.Ignore());
            //CreateMap<AccelerateProjectGroup, AccelerateProjectGroupDTO>();
            //CreateMap<AccelerateProject, AccelerateProjectDTO>();
            //CreateMap<Script, ScriptDTO>();
            //CreateMap<NotificationRecord, NotificationRecordDTO>();
            //CreateMap<AccelerateProject, Guid>().ConvertUsing(x => x.Id);
        }
    }
}
