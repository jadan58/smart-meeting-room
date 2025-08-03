using AutoMapper;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AllMeetingsDto, Meeting>().ReverseMap();   
            // Room → RoomResponseDto (GET endpoint)
            CreateMap<Room, RoomResponseDto>()
                .ForMember(dest => dest.FeatureIds, opt => opt.MapFrom(src =>
                    src.RoomFeatures.Select(rf => rf.FeatureId).ToList()));

            // CreateRoomRequestDto → Room (POST endpoint)
            CreateMap<CreateRoomRequestDto, Room>()
                .ForMember(dest => dest.RoomFeatures, opt => opt.Ignore()); // We’ll handle this manually in controller/repo

            // UpdateRoomRequestDto → Room (PUT endpoint)
            CreateMap<UpdateRoomRequestDto, Room>()
                .ForMember(dest => dest.RoomFeatures, opt => opt.Ignore()); // Same as above

            // RoomFeature ↔ RoomFeatureDto (if used anywhere)
            CreateMap<RoomFeature, RoomFeatureDto>().ReverseMap();


            // Meeting mappings
            CreateMap<Meeting, MeetingResponseDto>();
            CreateMap<CreateMeetingRequestDto, Meeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<UpdateMeetingRequestDto, Meeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Note mappings
            CreateMap<Note, NoteDto>();
            CreateMap<CreateNoteRequestDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<UpdateNoteRequestDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // ActionItem mappings
            CreateMap<ActionItem, ActionItemDto>();
            CreateMap<CreateActionItemRequestDto, ActionItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore());
            CreateMap<UpdateActionItemRequestDto, ActionItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore());

            // Invitee mappings
            CreateMap<Invitee, InviteeDto>();
            CreateMap<CreateInviteeRequestDto, Invitee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore())
                .ForMember(dest => dest.Attended, opt => opt.MapFrom(src => false));

            // Attachment mappings
            CreateMap<Attachment, AttachmentDto>();
            CreateMap<CreateAttachmentRequestDto, Attachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());

            //User mappings
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();

            //feature mappings
            CreateMap<Feature, FeatureDto>().ReverseMap();
            CreateMap<CreateFeatureRequestDTO, Feature>().ReverseMap();
            CreateMap<UpdateFeatureDTO, Feature>().ReverseMap();

        }
    }
}
