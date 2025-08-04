using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsRepository _repository;
    private readonly IMapper _mapper;

    public NotificationsController(INotificationsRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var notifications = await _repository.GetUserNotificationsAsync(userId);
        return Ok(_mapper.Map<List<NotificationDto>>(notifications));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            Subject = dto.Subject,
            Body = dto.Body,
            UserId = dto.UserId
        };
        await _repository.CreateAsync(notification);
        return Ok("Notification created successfully.");
    }

    [HttpPut("mark-as-read/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var notification = await _repository.GetByIdAsync(id);

        if (notification == null)
            return NotFound("Notification not found.");

        if (notification.UserId != userId)
            return Forbid("You cannot mark someone else's notification as read.");

        await _repository.MarkAsReadAsync(id);
        return Ok("Notification marked as read.");
    }


    [HttpDelete("{id}")]
    [Authorize(Roles ="Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var notification = await _repository.GetByIdAsync(id);

        if (notification == null)
            return NotFound("Notification not found.");

        if (notification.UserId != userId)
            return Forbid("You cannot delete someone else's notification.");

        await _repository.DeleteAsync(id);
        return Ok("Notification deleted.");
    }

}
