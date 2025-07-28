using System;
using System.Collections.Generic;

namespace SmartMeetingRoomAPI.DTOs
{
    public class UpdateRoomRequestDto
    {
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }

        public List<Guid> FeatureIds { get; set; }
    }
}
