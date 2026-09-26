using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTS_API.Data;
using MTS_API.DTOs;
using MTS_API.Models;

namespace MTS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MtsDbContext _context;

    public MessagesController(MtsDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // POST: api/Messages
    // SEND MESSAGE - ADMIN / CLIENT
    // =========================================================

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage(
        [FromBody] CreateMessageRequest request)
    {
        var senderUserIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(senderUserIdClaim))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identity not found."
            });
        }

        if (!int.TryParse(
                senderUserIdClaim,
                out var senderUserId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        if (request.ReceiverUserId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid ReceiverUserId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.MessageText))
        {
            return BadRequest(new
            {
                success = false,
                message = "Message text is required."
            });
        }

        if (request.ReceiverUserId == senderUserId)
        {
            return BadRequest(new
            {
                success = false,
                message = "You cannot send a message to yourself."
            });
        }

        var receiver = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == request.ReceiverUserId &&
                x.IsActive);

        if (receiver == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Receiver user not found."
            });
        }

        if (request.ProjectId.HasValue)
        {
            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.ProjectId == request.ProjectId.Value);

            if (!projectExists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Project not found."
                });
            }
        }

        var message = new Message
        {
            SenderUserId = senderUserId,
            ReceiverUserId = request.ReceiverUserId,
            ProjectId = request.ProjectId,

            Subject =
                string.IsNullOrWhiteSpace(request.Subject)
                    ? null
                    : request.Subject.Trim(),

            MessageText =
                request.MessageText.Trim(),

            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Message sent successfully.",
            messageId = message.MessageId,
            senderUserId = message.SenderUserId,
            receiverUserId = message.ReceiverUserId,
            projectId = message.ProjectId,
            subject = message.Subject,
            messageText = message.MessageText,
            isRead = message.IsRead,
            createdAt = message.CreatedAt
        });
    }


    // =========================================================
    // GET: api/Messages/my-messages
    // CLIENT INBOX
    // =========================================================

    [HttpGet("my-messages")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyMessages()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identity not found."
            });
        }

        if (!int.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(x =>
                x.ReceiverUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                messageId = x.MessageId,
                senderUserId = x.SenderUserId,
                projectId = x.ProjectId,
                subject = x.Subject,
                messageText = x.MessageText,
                isRead = x.IsRead,
                createdAt = x.CreatedAt,
                readAt = x.ReadAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            messages
        });
    }


    // =========================================================
    // GET: api/Messages
    // ADMIN INBOX
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllMessages()
    {
        var messages = await _context.Messages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                messageId = x.MessageId,

                senderUserId = x.SenderUserId,
                senderName = x.SenderUser.FullName,

                receiverUserId = x.ReceiverUserId,
                receiverName = x.ReceiverUser.FullName,

                projectId = x.ProjectId,

                projectName =
                    x.Project != null
                        ? x.Project.ProjectName
                        : null,

                subject = x.Subject,
                messageText = x.MessageText,
                isRead = x.IsRead,
                createdAt = x.CreatedAt,
                readAt = x.ReadAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            messages
        });
    }


    // =========================================================
    // PUT: api/Messages/{id}/read
    // MARK MESSAGE AS READ
    // =========================================================

    [HttpPut("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid MessageId is required."
            });
        }

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identity not found."
            });
        }

        if (!int.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var message = await _context.Messages
            .FirstOrDefaultAsync(x =>
                x.MessageId == id &&
                x.ReceiverUserId == userId);

        if (message == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Message not found."
            });
        }

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            success = true,
            message = "Message marked as read successfully.",
            messageId = message.MessageId,
            isRead = message.IsRead,
            readAt = message.ReadAt
        });
    }


    // =========================================================
    // GET: api/Messages/my-conversations
    // CLIENT / ADMIN CONVERSATIONS
    // =========================================================

    [HttpGet("my-conversations")]
    [Authorize]
    public async Task<IActionResult> GetMyConversations()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(x =>
                x.SenderUserId == userId ||
                x.ReceiverUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                messageId = x.MessageId,

                senderUserId = x.SenderUserId,
                senderName = x.SenderUser.FullName,

                receiverUserId = x.ReceiverUserId,
                receiverName = x.ReceiverUser.FullName,

                projectId = x.ProjectId,

                projectName =
                    x.Project != null
                        ? x.Project.ProjectName
                        : null,

                subject = x.Subject,
                messageText = x.MessageText,

                isRead = x.IsRead,

                createdAt = x.CreatedAt
            })
            .ToListAsync();

        var conversations = messages
            .GroupBy(x =>
                x.senderUserId == userId
                    ? x.receiverUserId
                    : x.senderUserId)
            .Select(group =>
            {
                var latest = group
                    .OrderByDescending(x => x.createdAt)
                    .First();

                var otherUserId =
                    latest.senderUserId == userId
                        ? latest.receiverUserId
                        : latest.senderUserId;

                var otherUserName =
                    latest.senderUserId == userId
                        ? latest.receiverName
                        : latest.senderName;

                var unreadCount = group.Count(x =>
                    x.receiverUserId == userId &&
                    !x.isRead);

                return new
                {
                    userId = otherUserId,
                    userName = otherUserName,

                    projectId =
                        latest.projectId,

                    projectName =
                        latest.projectName,

                    lastMessage =
                        latest.messageText,

                    lastMessageTime =
                        latest.createdAt,

                    unreadCount
                };
            })
            .OrderByDescending(x =>
                x.lastMessageTime)
            .ToList();

        return Ok(new
        {
            success = true,
            currentUserId = userId,
            conversations
        });
    }


    // =========================================================
    // GET: api/Messages/conversation/{userId}
    // CLIENT / ADMIN CONVERSATION HISTORY
    // =========================================================

    [HttpGet("conversation/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetConversation(
        int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Valid user ID is required."
            });
        }

        var currentUserIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(
                currentUserIdClaim) ||
            !int.TryParse(
                currentUserIdClaim,
                out var currentUserId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid user identity."
            });
        }

        if (currentUserId == userId)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "You cannot open a conversation with yourself."
            });
        }

        var otherUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.IsActive);

        if (otherUser == null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(x =>
                (x.SenderUserId == currentUserId &&
                 x.ReceiverUserId == userId)
                ||
                (x.SenderUserId == userId &&
                 x.ReceiverUserId == currentUserId))
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                messageId = x.MessageId,

                senderUserId =
                    x.SenderUserId,

                senderName =
                    x.SenderUser.FullName,

                receiverUserId =
                    x.ReceiverUserId,

                receiverName =
                    x.ReceiverUser.FullName,

                projectId =
                    x.ProjectId,

                projectName =
                    x.Project != null
                        ? x.Project.ProjectName
                        : null,

                subject =
                    x.Subject,

                messageText =
                    x.MessageText,

                isRead =
                    x.IsRead,

                createdAt =
                    x.CreatedAt,

                readAt =
                    x.ReadAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,

            currentUserId,

            otherUserId = userId,

            otherUserName =
                otherUser.FullName,

            messages
        });
    }
}