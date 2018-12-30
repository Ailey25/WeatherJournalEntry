using AutoMapper;
using WeatherJournalBackend.Dtos;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Helpers {
    public class AutoMapperProfile : Profile {
        public AutoMapperProfile() {
            CreateMap<CoordDto, Coord>().ReverseMap();
            CreateMap<WeatherDto, Weather>().ReverseMap();
            CreateMap<MainDto, Main>().ReverseMap();
            CreateMap<WindDto, Wind>().ReverseMap();
            CreateMap<CloudsDto, Clouds>().ReverseMap();
            CreateMap<SysDto, Sys>().ReverseMap();
            CreateMap<WeatherObjectDto, WeatherObject>().ReverseMap();

            CreateMap<UserDto, User>().ReverseMap();
        }
    }
}