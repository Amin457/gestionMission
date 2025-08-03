using gestionMissionBack.Domain.Entities;
using AutoMapper;
using gestionMissionBack.Application.DTOs.Mission;
using gestionMissionBack.Application.DTOs.TaskMission;
using gestionMissionBack.Application.DTOs.Site;
using gestionMissionBack.Application.DTOs.VehicleReservation;
using gestionMissionBack.Application.DTOs.Incident;
using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Application.DTOs.MissionCost;
using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.DTOs.Circuit;
using gestionMissionBack.Application.DTOs.City;
using gestionMissionBack.Application.DTOs.Document;
using gestionMissionBack.Application.DTOs.Role;
using gestionMissionBack.Application.DTOs.Route;
using gestionMissionBack.Application.DTOs.Vehicle;

namespace gestionMissionBack.Application.Mappings
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            // User to UserDto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null));

            // UserDto to User
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Prevent null overwrites

            // User to UserUpdateDto and reverse
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Role, RoleDto>().ReverseMap();

            CreateMap<Mission, MissionDto>().ReverseMap();
            CreateMap<Mission, MissionDtoGet>()
                .ForMember(dest => dest.Requester, opt => opt.MapFrom(src => src.Requester.FirstName + " " + src.Requester.LastName))
                .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Driver.FirstName + " " + src.Driver.LastName));

            CreateMap<TaskMission, TaskMissionDtoGet>()
                .ForMember(dest => dest.SiteName, opt => opt.MapFrom(src => src.Site.Name))
                .ForMember(dest => dest.IsFirstTask, opt => opt.MapFrom(src => src.IsFirstTask));

            CreateMap<TaskMission, TaskMissionDto>()
                .ForMember(dest => dest.IsFirstTask, opt => opt.MapFrom(src => src.IsFirstTask))
                .ReverseMap();

            CreateMap<Document, DocumentDto>().ReverseMap();

            CreateMap<Article, ArticleDto>().ReverseMap();
            CreateMap<Article, ArticleCreateDto>().ReverseMap();
            CreateMap<Article, ArticleUpdateDto>().ReverseMap();
            CreateMap<Article, ArticleGetDto>()
                .ForMember(dest => dest.PhotoUrls, opt => opt.Ignore()); // We'll handle this manually in the service

            CreateMap<Incident, IncidentDto>().ReverseMap();
            CreateMap<Incident, IncidentCreateDto>().ReverseMap();
            CreateMap<Incident, IncidentUpdateDto>().ReverseMap();
            CreateMap<Incident, IncidentGetDto>()
                .ForMember(dest => dest.IncidentDocsUrls, opt => opt.Ignore()); // We'll handle this manually in the service

            CreateMap<MissionCost, MissionCostDto>().ReverseMap();
            CreateMap<MissionCost, MissionCostCreateDto>().ReverseMap();
            CreateMap<MissionCost, MissionCostUpdateDto>().ReverseMap();
            CreateMap<MissionCost, MissionCostGetDto>()
                .ForMember(dest => dest.ReceiptPhotoUrls, opt => opt.Ignore()); // We'll handle this manually in the service

            CreateMap<Circuit, CircuitDto>()
                .ForMember(dest => dest.DepartureSiteName, opt => opt.MapFrom(src => src.DepartureSite.Name))
                .ForMember(dest => dest.ArrivalSiteName, opt => opt.MapFrom(src => src.ArrivalSite.Name))
                .ReverseMap()
                .ForMember(dest => dest.DepartureSite, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalSite, opt => opt.Ignore());

            CreateMap<Route, RouteDto>()
                .ForMember(dest => dest.DepartureSiteName, opt => opt.MapFrom(src => src.DepartureSite.Name))
                .ForMember(dest => dest.ArrivalSiteName, opt => opt.MapFrom(src => src.ArrivalSite.Name))
                .ReverseMap()
                .ForMember(dest => dest.DepartureSite, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalSite, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleDto>().ReverseMap();
            CreateMap<Vehicle, VehicleCreateDto>().ReverseMap();
            CreateMap<Vehicle, VehicleUpdateDto>().ReverseMap();
            CreateMap<Vehicle, VehicleGetDto>()
                .ForMember(dest => dest.PhotoUrls, opt => opt.Ignore()); // We'll handle this manually in the service

            CreateMap<VehicleReservation, VehicleReservationDto>().ReverseMap();
            CreateMap<VehicleReservation, VehicleReservationDtoGet>()
                .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.Requester.FirstName + " " + src.Requester.LastName))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle.LicensePlate));

            CreateMap<City, CityDto>().ReverseMap();

            CreateMap<Site, SiteDto>().ReverseMap();
            CreateMap<Site, SiteDtoGet>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name ));
        }
    }
}
