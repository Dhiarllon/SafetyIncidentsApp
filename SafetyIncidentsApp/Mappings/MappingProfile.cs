using AutoMapper;
using SafetyIncidentsApp.DTOs;
using SafetyIncidentsApp.Models;

namespace SafetyIncidentsApp.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Incident mappings
        CreateMap<Incident, IncidentReadDto>()
            .ForMember(dest => dest.ReportedByName, opt => opt.MapFrom(src => src.ReportedBy.Name))
            .ForMember(dest => dest.InvolvedEmployeeName, opt => opt.MapFrom(src => src.InvolvedEmployee != null ? src.InvolvedEmployee.Name : null));

        CreateMap<IncidentCreateDto, Incident>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsResolved, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.RequiresManagerApproval, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedBy, opt => opt.Ignore())
            .ForMember(dest => dest.InvolvedEmployee, opt => opt.Ignore());

        CreateMap<IncidentUpdateDto, Incident>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedById, opt => opt.Ignore())
            .ForMember(dest => dest.IsResolved, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedBy, opt => opt.Ignore())
            .ForMember(dest => dest.InvolvedEmployee, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Employee mappings
        CreateMap<Employee, EmployeeReadDto>();
        CreateMap<EmployeeCreateDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedIncidents, opt => opt.Ignore())
            .ForMember(dest => dest.InvolvedIncidents, opt => opt.Ignore());
    }
}
