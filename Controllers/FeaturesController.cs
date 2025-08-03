using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public FeaturesController(IFeatureRepository featureRepository, IMapper mapper)
        {
            _featureRepository = featureRepository;
            _mapper = mapper;
        }

        // GET: api/Features
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeatureDto>>> GetAllFeatures()
        {
            var features = await _featureRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<FeatureDto>>(features));
        }

        // GET: api/Features/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FeatureDto>> GetFeatureById(Guid id)
        {
            var feature = await _featureRepository.GetByIdAsync(id);
            if (feature == null)
                return NotFound();

            return Ok(_mapper.Map<FeatureDto>(feature));
        }

        // POST: api/Features
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeatureDto>> CreateFeature([FromBody] CreateFeatureRequestDTO createDto)
        {
            var feature = _mapper.Map<Feature>(createDto);
            feature.Id = Guid.NewGuid();

            var created = await _featureRepository.AddAsync(feature);
            return CreatedAtAction(nameof(GetFeatureById), new { id = created.Id }, _mapper.Map<FeatureDto>(created));
        }

        // PUT: api/Features/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeatureDto>> UpdateFeature(Guid id, [FromBody] UpdateFeatureDTO updateDto)
        {
            var existing = await _featureRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(updateDto, existing);
            var updated = await _featureRepository.UpdateAsync(existing);

            return Ok(_mapper.Map<FeatureDto>(updated));
        }

        // DELETE: api/Features/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeature(Guid id)
        {
            var success = await _featureRepository.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
