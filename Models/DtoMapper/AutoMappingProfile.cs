using AutoMapper;
using Models.DtoModels;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.ViewModels;
using Models.ViewModels;
using Models.DtoModels;
using System.Transactions;

namespace Models.DtoMapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            // User -> UserDto
            CreateMap<Users, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.UserBranches != null ? (src.UserBranches.FirstOrDefault(ub => ub.IsDefault) != null ? (int?)src.UserBranches.FirstOrDefault(ub => ub.IsDefault).BranchId : null) : null))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.UserBranches != null ? (src.UserBranches.FirstOrDefault(ub => ub.IsDefault) != null && src.UserBranches.FirstOrDefault(ub => ub.IsDefault).Branch != null ? src.UserBranches.FirstOrDefault(ub => ub.IsDefault).Branch.Name : null) : null))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.FullName)
                                                                          ? src.FullName
                                                                          : $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Photo, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Photo)))
                .ForMember(dest => dest.Sign, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Sign)));
            CreateMap<UserViewModel, Users>();

            // Branch
            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null));

            CreateMap<BranchViewModel, Branch>();

            // ================= USER BRANCH =================
            CreateMap<UserBranch, UserBranchDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null));


            // ================= MENU =================
            CreateMap<Menu, MenuDto>().ReverseMap();
          
        }
    }
}
