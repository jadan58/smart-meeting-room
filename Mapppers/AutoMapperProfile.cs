using AutoMapper;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Room mappings
            CreateMap<Room, RoomResponseDto>().ReverseMap();
            CreateMap<CreateRoomRequestDto, Room>();
            CreateMap<UpdateRoomRequestDto, Room>();

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
        }
    }
}
