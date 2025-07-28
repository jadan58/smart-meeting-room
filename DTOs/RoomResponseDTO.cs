using System;
using System.Collections.Generic;

namespace SmartMeetingRoomAPI.DTOs
{
    public class RoomResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }

        public List<RoomFeatureDto> Features { get; set; }
    }
}
