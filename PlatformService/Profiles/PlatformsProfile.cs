using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;
namespace PlatformService.PlatformsProfiles
{
    public class PlatformsProfile : Profile
    {
        public PlatformsProfile()
        {
            CreateMap<Platform, PlatformReadDto>(); //Source -> Target
            CreateMap<PlatformCreateDto, Platform>();
            CreateMap<PlatformReadDto, PlatformPublishedDto>(); //Source -> Target
        }
    }
}