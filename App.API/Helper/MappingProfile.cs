using App.API.DTOs;
using App.Core.Entities;
using App.Core.Models;
using AutoMapper;
using System.Reflection;

namespace App.API.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            var asm = Assembly.Load("App.Core");
            var classes =
                asm.GetTypes().Where(p =>
                    p.Namespace != null && (p.Namespace.Equals("App.Core.Entities") || p.Namespace.Equals("App.Core.Entities.Lookups")) &&
                    p.IsClass



                ).ToList();
            foreach (Type c in classes)
            {
                CreateMap(c, c)
                        .ForMember("CreationDate", act => act.Ignore())
                        .ForMember("LastUpdatedDate", act => act.Ignore());
            }
            // map Entity with Model
            CreateMap<AppUser, AppUserModel>().ReverseMap();


            // map Model with DTO
            CreateMap<AppUserModel, AppUserDTO>().ReverseMap();



        }
    }
}
