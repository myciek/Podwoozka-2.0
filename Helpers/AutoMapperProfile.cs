using AutoMapper;
using Podwoozka.Dtos;
using Podwoozka.Entities;

namespace Podwoozka.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}