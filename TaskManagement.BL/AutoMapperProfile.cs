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
            CreateMap<User, UserLoginDTO>().ReverseMap();
            CreateMap<User, UserRegisterDTO>().ReverseMap();

            CreateMap<Domain.Models.Task, TaskDTO>().ReverseMap();
            CreateMap<Domain.Models.Task, TaskDetailsDTO>().ReverseMap();
        }
    }
}
