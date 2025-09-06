using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;


[Authorize]
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IWebHostEnvironment env, AppDbContext dbContext, ILogger<FilesController> logger)
    {
        _env = env;
        _dbContext = dbContext;
        _logger = logger;
    }


    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    private bool IsCreator(Meeting meeting, Guid userId) => meeting.UserId == userId;

    private bool IsCreatorOrInvitee(Meeting meeting, Guid userId) =>
        IsCreator(meeting, userId) || meeting.Invitees.Any(i => i.UserId == userId && i.Attendance == "Accepted");

    private IActionResult ServeFile(string type, string? entityId, string fileName)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "upload", type, fileName);

        if (!string.IsNullOrEmpty(entityId))
        {
            filePath = Path.Combine(_env.ContentRootPath, "upload", type, entityId, fileName);

        }
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
            contentType = "application/octet-stream";

        return PhysicalFile(filePath, contentType, fileDownloadName: fileName, enableRangeProcessing: true);
    }

    // ===================== 1. Meeting attachments =====================
    [HttpGet("meetings/{meetingId}/{fileName}")]
    public async Task<IActionResult> GetMeetingAttachment(Guid meetingId, string fileName)
    {
        var userId = GetCurrentUserId();
        var meeting = await _dbContext.Meetings
            .Include(m => m.Invitees)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            return NotFound("Meeting not found.");

        if (!IsCreatorOrInvitee(meeting, userId))
            return Forbid();

        return ServeFile("meetings", meetingId.ToString(), fileName);
    }

    // ===================== 2. Action item assignment attachments =====================
    [HttpGet("action-items/{itemId}/assignment/{fileName}")]
    public async Task<IActionResult> GetAssignmentAttachment(Guid itemId, string fileName)
    {
        var userId = GetCurrentUserId();
        var item = await _dbContext.ActionItems
            .Include(ai => ai.Meeting)
            .FirstOrDefaultAsync(ai => ai.Id == itemId);

        if (item == null)
            return NotFound("Action item not found.");

        // Only meeting creator or assigned-to user can access assignment attachments
        if (!IsCreator(item.Meeting, userId)&& userId!=item.AssignedToUserId)
            return Forbid();

        return ServeFile(Path.Combine("action-items", itemId.ToString(), "assignment"), null, fileName);
    }

    // ===================== 3. Action item submission attachments =====================
    [HttpGet("action-items/{itemId}/submission/{fileName}")]
    public async Task<IActionResult> GetSubmissionAttachment(Guid itemId, string fileName)
    {
        var userId = GetCurrentUserId();
        var item = await _dbContext.ActionItems
            .Include(ai => ai.Meeting)
            .FirstOrDefaultAsync(ai => ai.Id == itemId);

        //_logger.LogInformation("ActionItem loaded: {ItemJson}", JsonSerializer.Serialize(item, new JsonSerializerOptions
        //{
        //    WriteIndented = true,
        //    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        //}));


        if (item == null)
            return NotFound("Action item not found.");

        // User must be assigned to the item OR meeting creator
        if (item.AssignedToUserId != userId && !IsCreator(item.Meeting, userId))
            return Forbid();

        return ServeFile(Path.Combine("action-items", itemId.ToString(), "submission"), null, fileName);
    }

}
