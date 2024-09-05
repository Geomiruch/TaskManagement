using AutoMapper;
using TaskManagement.Domain.Models;
using TaskManagement.BL.DTO.User;
using TaskManagement.BL.DTO.Task;

namespace TaskManagement.BL
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserLoginDto>().ReverseMap();
            CreateMap<User, UserRegisterDto>().ReverseMap();

            CreateMap<Domain.Models.Task, TaskDTO>().ReverseMap();
        }
    }
}
