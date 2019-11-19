using AutoMapper;
using BusinessLogic.BusinessObjects;
using BusinessLogic.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BusinessLogic.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<User, UserLoginViewModel>();
            CreateMap<UserViewModel, User>();
            //CreateMap<Project, ProjectNewViewModel>().ForMember(d => d.TypeId, opt => opt.MapFrom(src => src.TypeId));




        }

    }
}