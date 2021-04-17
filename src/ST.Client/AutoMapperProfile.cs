using AutoMapper;
using System.Application.Entities;
using System.Application.Models;

namespace System.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<AccelerateProjectGroup, AccelerateProjectGroupDTO>();
            //CreateMap<AccelerateProject, AccelerateProjectDTO>();
            //CreateMap<Script, ScriptDTO>();
            //CreateMap<NotificationRecord, NotificationRecordDTO>();
            //CreateMap<AccelerateProject, Guid>().ConvertUsing(x => x.Id);
        }
    }
}
