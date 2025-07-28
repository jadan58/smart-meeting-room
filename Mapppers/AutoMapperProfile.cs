using AutoMapper;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Mapppers
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile() {
            CreateMap<Room, RoomResponseDto>().ReverseMap();
            CreateMap<Room, CreateRoomRequestDto>().ReverseMap();
            CreateMap<Room, UpdateRoomRequestDto>().ReverseMap();
            CreateMap<RoomFeature, RoomFeatureDto>().ReverseMap();
        }
    }
}
