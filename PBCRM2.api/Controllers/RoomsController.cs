using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Data;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Security.Claims;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class RoomsController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    // =========================================================
    // ROOM TYPES
    // =========================================================

    private const string SharedRoomType = "Shared";
    private const string PrivateRoomType = "Private";

    private const int SharedRoomCapacity = 4;
    private const int PrivateRoomCapacity = 1;

    public RoomsController(
        ITenantDbContextFactory tenantDbFactory,
        UserManager<ApplicationUser> userManager)
    {
        _tenantDbFactory = tenantDbFactory;
        _userManager = userManager;
    }

    // =========================================================
    // COMPANY ID
    // =========================================================

    private int? TryGetCompanyId()
    {
        string? value = User.FindFirstValue("CompanyId");

        if (int.TryParse(value, out int companyId))
            return companyId;

        return null;
    }

    // =========================================================
    // ROLE HELPERS
    // =========================================================

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // =========================================================
    // CURRENT USER
    // =========================================================

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _userManager.FindByIdAsync(userId);
    }

    // =========================================================
    // ROOM TYPE HELPERS
    // =========================================================

    private static bool IsValidRoomType(string? roomType)
    {
        string normalized = roomType?.Trim() ?? string.Empty;

        return
            string.Equals(
                normalized,
                SharedRoomType,
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                normalized,
                PrivateRoomType,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRoomType(string roomType)
    {
        string normalized = roomType.Trim();

        if (string.Equals(
                normalized,
                SharedRoomType,
                StringComparison.OrdinalIgnoreCase))
        {
            return SharedRoomType;
        }

        if (string.Equals(
                normalized,
                PrivateRoomType,
                StringComparison.OrdinalIgnoreCase))
        {
            return PrivateRoomType;
        }

        throw new ArgumentException(
            "Room type must be either Shared or Private.");
    }

    private static int GetRoomCapacity(string roomType)
    {
        return string.Equals(
            roomType,
            PrivateRoomType,
            StringComparison.OrdinalIgnoreCase)
            ? PrivateRoomCapacity
            : SharedRoomCapacity;
    }

    // =========================================================
    // GET ROOMS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        IQueryable<Room> query =
            db.Rooms
                .Include(r => r.Branch)
                .Include(r => r.Beds)
                .AsNoTracking();

        // =====================================================
        // ADMIN
        // =====================================================

        if (IsAdmin())
        {
            // Admin can view all branches.
        }
        else
        {
            // =================================================
            // MANAGER / STAFF
            // =================================================

            if (!user.BranchId.HasValue)
            {
                return BadRequest(
                    "The current user is not assigned to a branch.");
            }

            int branchId =
                user.BranchId.Value;

            query =
                query.Where(
                    r => r.BranchId == branchId);
        }

        var rooms =
            await query
                .OrderBy(r => r.BranchId)
                .ThenBy(r => r.RoomNumber)
                .Select(r => new
                {
                    r.Id,
                    r.BranchId,

                    BranchName =
                        r.Branch != null
                            ? r.Branch.BranchName
                            : string.Empty,

                    r.RoomNumber,
                    r.RoomType,
                    r.Capacity,
                    r.Status,
                    r.IsActive,

                    BedCount =
                        r.Beds.Count(),

                    OccupiedBeds =
                        r.Beds.Count(
                            b => b.TenantId.HasValue),

                    AvailableBeds =
                        r.Beds.Count(
                            b =>
                                !b.TenantId.HasValue &&
                                b.Status == "Available"),

                    MaintenanceBeds =
                        r.Beds.Count(
                            b =>
                                b.Status == "Maintenance"),

                    PendingAssignments =
                        db.BedAssignmentRequests.Count(
                            a =>
                                a.Status == "Pending" &&
                                a.Bed != null &&
                                a.Bed.RoomId == r.Id)
                })
                .ToListAsync();

        return Ok(rooms);
    }

    // =========================================================
    // GET BEDS FOR ROOM
    // =========================================================

    [HttpGet("{id}/beds")]
    public async Task<IActionResult> GetBeds(int id)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Room? room =
            await db.Rooms
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(
                    r => r.Id == id);

        if (room == null)
            return NotFound("Room not found.");

        // =====================================================
        // BRANCH ACCESS
        // =====================================================

        if (!IsAdmin())
        {
            if (!user.BranchId.HasValue)
            {
                return BadRequest(
                    "The current user is not assigned to a branch.");
            }

            if (room.BranchId != user.BranchId.Value)
                return Forbid();
        }

        var beds =
            await db.Beds
                .Where(b => b.RoomId == id)
                .Include(b => b.Tenant)
                .OrderBy(b => b.BedNumber)
                .Select(b => new
                {
                    b.Id,
                    b.RoomId,
                    b.BedNumber,
                    b.Status,
                    b.TenantId,

                    TenantName =
                        b.Tenant != null
                            ? b.Tenant.FullName
                            : null,

                    PendingAssignment =
                        db.BedAssignmentRequests
                            .Where(
                                a =>
                                    a.BedId == b.Id &&
                                    a.Status == "Pending")
                            .OrderByDescending(
                                a => a.RequestedAt)
                            .Select(a => new
                            {
                                a.Id,
                                a.TenantId,

                                TenantName =
                                    a.Tenant != null
                                        ? a.Tenant.FullName
                                        : null,

                                a.RequestedAt
                            })
                            .FirstOrDefault()
                })
                .ToListAsync();

        return Ok(beds);
    }

    // =========================================================
    // CREATE ROOM
    // MANAGER ONLY
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> CreateRoom(
        [FromBody] RoomRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        if (string.IsNullOrWhiteSpace(request.RoomNumber))
        {
            return BadRequest(
                "Room number is required.");
        }

        if (!IsValidRoomType(request.RoomType))
        {
            return BadRequest(
                "Room type must be either Shared or Private.");
        }

        string roomType =
            NormalizeRoomType(request.RoomType);

        int capacity =
            GetRoomCapacity(roomType);

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Branch? branch =
            await db.Branches
                .FirstOrDefaultAsync(
                    b =>
                        b.Id == user.BranchId.Value &&
                        b.IsActive);

        if (branch == null)
        {
            return BadRequest(
                "The manager's branch does not exist or is inactive.");
        }

        string roomNumber =
            request.RoomNumber.Trim();

        bool duplicate =
            await db.Rooms.AnyAsync(
                r =>
                    r.BranchId == user.BranchId.Value &&
                    r.RoomNumber == roomNumber);

        if (duplicate)
        {
            return Conflict(
                "A room with this room number already exists in this branch.");
        }

        var room = new Room
        {
            BranchId =
                user.BranchId.Value,

            RoomNumber =
                roomNumber,

            RoomType =
                roomType,

            Capacity =
                capacity,

            Status =
                "Available",

            IsActive =
                true
        };

        db.Rooms.Add(room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Room created successfully.",
            room.Id,
            room.BranchId,
            room.RoomNumber,
            room.RoomType,
            room.Capacity,
            room.Status,
            room.IsActive
        });
    }

    // =========================================================
    // UPDATE ROOM
    // MANAGER ONLY
    // =========================================================

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> UpdateRoom(
        int id,
        [FromBody] RoomRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        if (string.IsNullOrWhiteSpace(request.RoomNumber))
        {
            return BadRequest(
                "Room number is required.");
        }

        if (!IsValidRoomType(request.RoomType))
        {
            return BadRequest(
                "Room type must be either Shared or Private.");
        }

        string roomType =
            NormalizeRoomType(request.RoomType);

        int capacity =
            GetRoomCapacity(roomType);

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Room? room =
            await db.Rooms
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(
                    r =>
                        r.Id == id &&
                        r.BranchId == user.BranchId.Value);

        if (room == null)
            return NotFound("Room not found.");

        int totalBeds =
            room.Beds.Count;

        int occupiedBeds =
            room.Beds.Count(
                b => b.TenantId.HasValue);

        // =====================================================
        // CAPACITY VALIDATION
        // =====================================================

        if (totalBeds > capacity)
        {
            return BadRequest(
                $"{roomType} rooms can only have {capacity} bed(s). " +
                $"This room currently has {totalBeds} bed(s). " +
                "Remove the extra beds before changing the room type.");
        }

        if (occupiedBeds > capacity)
        {
            return BadRequest(
                "The selected room type cannot accommodate " +
                "the currently occupied beds.");
        }

        string roomNumber =
            request.RoomNumber.Trim();

        bool duplicate =
            await db.Rooms.AnyAsync(
                r =>
                    r.Id != id &&
                    r.BranchId == user.BranchId.Value &&
                    r.RoomNumber == roomNumber);

        if (duplicate)
        {
            return Conflict(
                "A room with this room number already exists in this branch.");
        }

        room.RoomNumber =
            roomNumber;

        room.RoomType =
            roomType;

        room.Capacity =
            capacity;

        room.IsActive =
            request.IsActive;

        await UpdateRoomStatusAsync(
            db,
            room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Room updated successfully.",
            room.Id,
            room.BranchId,
            room.RoomNumber,
            room.RoomType,
            room.Capacity,
            room.Status,
            room.IsActive
        });
    }

    // =========================================================
    // ADD BED
    // MANAGER ONLY
    // =========================================================

    [HttpPost("{id}/beds")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> AddBed(
        int id,
        [FromBody] BedRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        if (string.IsNullOrWhiteSpace(request.BedNumber))
        {
            return BadRequest(
                "Bed number is required.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Room? room =
            await db.Rooms
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(
                    r =>
                        r.Id == id &&
                        r.BranchId == user.BranchId.Value);

        if (room == null)
            return NotFound("Room not found.");

        if (!room.IsActive)
        {
            return BadRequest(
                "Beds cannot be added to an inactive room.");
        }

        // =====================================================
        // KEEP CAPACITY CONSISTENT WITH ROOM TYPE
        // =====================================================

        int expectedCapacity =
            GetRoomCapacity(room.RoomType);

        if (room.Capacity != expectedCapacity)
        {
            room.Capacity =
                expectedCapacity;
        }

        if (room.Beds.Count >= expectedCapacity)
        {
            return BadRequest(
                $"This {room.RoomType.ToLowerInvariant()} room " +
                $"already has its maximum capacity of " +
                $"{expectedCapacity} bed(s).");
        }

        string bedNumber =
            request.BedNumber.Trim();

        bool duplicate =
            await db.Beds.AnyAsync(
                b =>
                    b.RoomId == id &&
                    b.BedNumber == bedNumber);

        if (duplicate)
        {
            return Conflict(
                "A bed with this number already exists in this room.");
        }

        var bed = new Bed
        {
            RoomId =
                room.Id,

            BedNumber =
                bedNumber,

            Status =
                "Available",

            TenantId =
                null
        };

        db.Beds.Add(bed);

        await db.SaveChangesAsync();

        await UpdateRoomStatusAsync(
            db,
            room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Bed added successfully.",
            bed.Id,
            bed.RoomId,
            bed.BedNumber,
            bed.Status
        });
    }

    // =========================================================
    // REQUEST BED ASSIGNMENT
    // MANAGER / STAFF
    // =========================================================

    [HttpPost("beds/{bedId}/request-assignment")]
    [Authorize(Roles = "Manager,Staff")]
    public async Task<IActionResult> RequestBedAssignment(
        int bedId,
        [FromBody] AssignBedRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The current user is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Bed? bed =
            await db.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(
                    b => b.Id == bedId);

        if (bed == null)
            return NotFound("Bed not found.");

        if (bed.Room == null)
        {
            return BadRequest(
                "The bed is not connected to a room.");
        }

        if (bed.Room.BranchId != user.BranchId.Value)
            return Forbid();

        if (!bed.Room.IsActive)
        {
            return BadRequest(
                "The room is inactive.");
        }

        if (bed.Status != "Available")
        {
            return BadRequest(
                "Only available beds can be requested.");
        }

        if (bed.TenantId.HasValue)
        {
            return BadRequest(
                "This bed is already assigned.");
        }

        Tenant? tenant =
            await db.Tenants
                .FirstOrDefaultAsync(
                    t => t.Id == request.TenantId);

        if (tenant == null)
            return NotFound("Tenant not found.");

        if (tenant.BranchId != user.BranchId.Value)
        {
            return BadRequest(
                "The tenant belongs to another branch.");
        }

        if (!string.Equals(
                tenant.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Only active tenants can be assigned to a bed.");
        }

        bool tenantAlreadyAssigned =
            await db.Beds.AnyAsync(
                b => b.TenantId == tenant.Id);

        if (tenantAlreadyAssigned)
        {
            return Conflict(
                "This tenant is already assigned to a bed.");
        }

        bool tenantPending =
            await db.BedAssignmentRequests.AnyAsync(
                a =>
                    a.TenantId == tenant.Id &&
                    a.Status == "Pending");

        if (tenantPending)
        {
            return Conflict(
                "This tenant already has a pending bed assignment request.");
        }

        bool bedPending =
            await db.BedAssignmentRequests.AnyAsync(
                a =>
                    a.BedId == bed.Id &&
                    a.Status == "Pending");

        if (bedPending)
        {
            return Conflict(
                "This bed already has a pending assignment request.");
        }

        var assignmentRequest =
            new BedAssignmentRequest
            {
                BedId =
                    bed.Id,

                TenantId =
                    tenant.Id,

                Status =
                    "Pending",

                RequestedByUserId =
                    user.Id,

                RequestedAt =
                    DateTime.UtcNow
            };

        db.BedAssignmentRequests.Add(
            assignmentRequest);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed assignment request submitted successfully.",

            assignmentRequest.Id,
            assignmentRequest.BedId,
            assignmentRequest.TenantId,
            assignmentRequest.Status,
            assignmentRequest.RequestedAt
        });
    }

    // =========================================================
    // GET PENDING ASSIGNMENT REQUESTS
    // MANAGER ONLY
    // =========================================================

    [HttpGet("assignment-requests")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        GetPendingAssignmentRequests()
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        var requests =
            await db.BedAssignmentRequests
                .Include(r => r.Bed)
                    .ThenInclude(b => b!.Room)
                .Include(r => r.Tenant)
                .Where(
                    r =>
                        r.Status == "Pending" &&
                        r.Bed != null &&
                        r.Bed.Room != null &&
                        r.Bed.Room.BranchId ==
                            user.BranchId.Value)
                .OrderBy(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id,
                    r.BedId,
                    r.TenantId,

                    RoomId =
                        r.Bed!.RoomId,

                    TenantName =
                        r.Tenant != null
                            ? r.Tenant.FullName
                            : string.Empty,

                    RoomNumber =
                        r.Bed.Room != null
                            ? r.Bed.Room.RoomNumber
                            : string.Empty,

                    BedNumber =
                        r.Bed.BedNumber,

                    r.Status,
                    r.RequestedByUserId,
                    r.RequestedAt
                })
                .ToListAsync();

        return Ok(requests);
    }

    // =========================================================
    // APPROVE ASSIGNMENT
    // MANAGER ONLY
    // =========================================================

    [HttpPost("assignment-requests/{requestId}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        ApproveAssignment(int requestId)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        BedAssignmentRequest? request =
            await db.BedAssignmentRequests
                .Include(r => r.Bed)
                    .ThenInclude(b => b!.Room)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(
                    r => r.Id == requestId);

        if (request == null)
        {
            return NotFound(
                "Assignment request not found.");
        }

        if (request.Status != "Pending")
        {
            return BadRequest(
                "This assignment request has already been processed.");
        }

        if (request.Bed == null ||
            request.Bed.Room == null)
        {
            return BadRequest(
                "The requested bed or room no longer exists.");
        }

        if (request.Bed.Room.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        if (!request.Bed.Room.IsActive)
        {
            return BadRequest(
                "The room is inactive.");
        }

        if (request.Tenant == null)
        {
            return BadRequest(
                "The requested tenant no longer exists.");
        }

        if (!string.Equals(
                request.Tenant.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Only active tenants can be assigned.");
        }

        if (request.Bed.TenantId.HasValue)
        {
            return Conflict(
                "The requested bed is already assigned.");
        }

        if (request.Bed.Status != "Available")
        {
            return Conflict(
                "The requested bed is no longer available.");
        }

        bool tenantAlreadyAssigned =
            await db.Beds.AnyAsync(
                b =>
                    b.TenantId ==
                    request.TenantId);

        if (tenantAlreadyAssigned)
        {
            return Conflict(
                "The tenant is already assigned to another bed.");
        }

        // =====================================================
        // ROOM CAPACITY
        // =====================================================

        int expectedCapacity =
            GetRoomCapacity(
                request.Bed.Room.RoomType);

        request.Bed.Room.Capacity =
            expectedCapacity;

        int occupiedBeds =
            await db.Beds.CountAsync(
                b =>
                    b.RoomId ==
                        request.Bed.RoomId &&
                    b.TenantId.HasValue);

        if (occupiedBeds >= expectedCapacity)
        {
            return Conflict(
                $"This {request.Bed.Room.RoomType.ToLowerInvariant()} " +
                $"room has already reached its capacity of " +
                $"{expectedCapacity}.");
        }

        request.Bed.TenantId =
            request.TenantId;

        request.Bed.Status =
            "Occupied";

        request.Status =
            "Approved";

        request.ApprovedByUserId =
            user.Id;

        request.ApprovedAt =
            DateTime.UtcNow;

        await UpdateRoomStatusAsync(
            db,
            request.Bed.Room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed assignment approved successfully.",

            request.Id,
            request.BedId,
            request.TenantId,
            request.Status,
            request.ApprovedAt
        });
    }

    // =========================================================
    // REJECT ASSIGNMENT
    // MANAGER ONLY
    // =========================================================

    [HttpPost("assignment-requests/{requestId}/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        RejectAssignment(
            int requestId,
            [FromBody] RejectAssignmentRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        if (string.IsNullOrWhiteSpace(
                request.RejectionReason))
        {
            return BadRequest(
                "A rejection reason is required.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        BedAssignmentRequest? assignment =
            await db.BedAssignmentRequests
                .Include(r => r.Bed)
                    .ThenInclude(b => b!.Room)
                .FirstOrDefaultAsync(
                    r => r.Id == requestId);

        if (assignment == null)
        {
            return NotFound(
                "Assignment request not found.");
        }

        if (assignment.Status != "Pending")
        {
            return BadRequest(
                "This assignment request has already been processed.");
        }

        if (assignment.Bed?.Room == null)
        {
            return BadRequest(
                "The requested bed or room no longer exists.");
        }

        if (assignment.Bed.Room.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        assignment.Status =
            "Rejected";

        assignment.RejectionReason =
            request.RejectionReason.Trim();

        assignment.ApprovedByUserId =
            user.Id;

        assignment.ApprovedAt =
            DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed assignment request rejected.",

            assignment.Id,
            assignment.Status,
            assignment.RejectionReason
        });
    }

    // =========================================================
    // RELEASE BED
    // MANAGER ONLY
    // =========================================================

    [HttpPost("beds/{bedId}/release")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        ReleaseBed(int bedId)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Bed? bed =
            await db.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(
                    b => b.Id == bedId);

        if (bed == null)
            return NotFound("Bed not found.");

        if (bed.Room == null)
        {
            return BadRequest(
                "The bed is not connected to a room.");
        }

        if (bed.Room.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        if (!bed.TenantId.HasValue)
        {
            return BadRequest(
                "This bed is not currently assigned.");
        }

        bed.TenantId =
            null;

        bed.Status =
            "Available";

        // =====================================================
        // CANCEL PENDING REQUESTS
        // =====================================================

        var pendingRequests =
            await db.BedAssignmentRequests
                .Where(
                    r =>
                        r.BedId == bed.Id &&
                        r.Status == "Pending")
                .ToListAsync();

        foreach (
            BedAssignmentRequest request
            in pendingRequests)
        {
            request.Status =
                "Rejected";

            request.RejectionReason =
                "The bed was released and is no longer reserved.";

            request.ApprovedByUserId =
                user.Id;

            request.ApprovedAt =
                DateTime.UtcNow;
        }

        await UpdateRoomStatusAsync(
            db,
            bed.Room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed released successfully.",

            bed.Id,
            bed.Status,
            bed.TenantId
        });
    }

    // =========================================================
    // DELETE ROOM
    // MANAGER ONLY
    // =========================================================

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        DeleteRoom(int id)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Room? room =
            await db.Rooms
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(
                    r =>
                        r.Id == id &&
                        r.BranchId ==
                            user.BranchId.Value);

        if (room == null)
            return NotFound("Room not found.");

        if (room.Beds.Any())
        {
            return BadRequest(
                "This room still has beds. " +
                "Remove the beds before deleting the room.");
        }

        bool hasAssignmentHistory =
            await db.BedAssignmentRequests
                .AnyAsync(
                    r =>
                        r.Bed != null &&
                        r.Bed.RoomId == room.Id);

        if (hasAssignmentHistory)
        {
            return BadRequest(
                "This room has assignment history and cannot be deleted.");
        }

        db.Rooms.Remove(room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Room removed successfully."
        });
    }

    // =========================================================
    // DELETE BED
    // MANAGER ONLY
    // =========================================================

    [HttpDelete("beds/{bedId}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        DeleteBed(int bedId)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Bed? bed =
            await db.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(
                    b => b.Id == bedId);

        if (bed == null)
            return NotFound("Bed not found.");

        if (bed.Room == null)
        {
            return BadRequest(
                "The bed is not connected to a room.");
        }

        if (bed.Room.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        if (bed.TenantId.HasValue)
        {
            return BadRequest(
                "This bed is currently assigned to a tenant. " +
                "Release it before removing the bed.");
        }

        bool hasAssignmentHistory =
            await db.BedAssignmentRequests
                .AnyAsync(
                    r =>
                        r.BedId == bed.Id);

        if (hasAssignmentHistory)
        {
            return BadRequest(
                "This bed has assignment history and cannot be removed.");
        }

        Room room =
            bed.Room;

        db.Beds.Remove(bed);

        await db.SaveChangesAsync();

        await UpdateRoomStatusAsync(
            db,
            room);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed removed successfully."
        });
    }

    // =========================================================
    // UPDATE BED
    // MANAGER ONLY
    // =========================================================

    [HttpPut("beds/{bedId}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult>
        UpdateBed(
            int bedId,
            [FromBody] BedRequest request)
    {
        ApplicationUser? user =
            await CurrentUserAsync();

        if (user == null)
            return Unauthorized();

        int? companyId =
            TryGetCompanyId();

        if (!companyId.HasValue)
        {
            return BadRequest(
                "CompanyId is missing from the authenticated user.");
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(
                "The manager is not assigned to a branch.");
        }

        if (string.IsNullOrWhiteSpace(
                request.BedNumber))
        {
            return BadRequest(
                "Bed number is required.");
        }

        await using TenantCRMDbContext db =
            await _tenantDbFactory.CreateAsync(
                companyId.Value);

        Bed? bed =
            await db.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(
                    b => b.Id == bedId);

        if (bed == null)
            return NotFound("Bed not found.");

        if (bed.Room == null)
        {
            return BadRequest(
                "The bed is not connected to a room.");
        }

        if (bed.Room.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        string bedNumber =
            request.BedNumber.Trim();

        bool duplicate =
            await db.Beds.AnyAsync(
                b =>
                    b.Id != bed.Id &&
                    b.RoomId == bed.RoomId &&
                    b.BedNumber == bedNumber);

        if (duplicate)
        {
            return Conflict(
                "A bed with this number already exists in this room.");
        }

        bed.BedNumber =
            bedNumber;

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Bed updated successfully.",

            bed.Id,
            bed.RoomId,
            bed.BedNumber,
            bed.Status
        });
    }

    // =========================================================
    // UPDATE ROOM STATUS
    // =========================================================

    private static async Task UpdateRoomStatusAsync(
        TenantCRMDbContext db,
        Room room)
    {
        if (!room.IsActive)
        {
            room.Status =
                "Inactive";

            return;
        }

        // =====================================================
        // ALWAYS KEEP CAPACITY CORRECT
        // =====================================================

        room.Capacity =
            GetRoomCapacity(room.RoomType);

        var beds =
            await db.Beds
                .Where(
                    b =>
                        b.RoomId == room.Id)
                .ToListAsync();

        if (beds.Count == 0)
        {
            room.Status =
                "Available";

            return;
        }

        bool hasMaintenance =
            beds.Any(
                b =>
                    b.Status == "Maintenance");

        if (hasMaintenance)
        {
            room.Status =
                "Maintenance";

            return;
        }

        int occupied =
            beds.Count(
                b =>
                    b.TenantId.HasValue);

        if (occupied >= room.Capacity)
        {
            room.Status =
                "Full";

            return;
        }

        if (occupied > 0)
        {
            room.Status =
                "Partially Occupied";

            return;
        }

        room.Status =
            "Available";
    }

    // =========================================================
    // REQUEST DTOs
    // =========================================================

    public class RoomRequest
    {
        public string RoomNumber { get; set; } =
            string.Empty;

        // ONLY:
        // Shared
        // Private
        public string RoomType { get; set; } =
            SharedRoomType;

        // Kept for compatibility with WinForms.
        //
        // The API does NOT trust this value.
        //
        // Shared  = 4
        // Private = 1
        public int Capacity { get; set; } =
            SharedRoomCapacity;

        public string Status { get; set; } =
            "Available";

        public bool IsActive { get; set; } =
            true;

        public int? BranchId { get; set; }
    }

    public class BedRequest
    {
        public string BedNumber { get; set; } =
            string.Empty;
    }

    public class AssignBedRequest
    {
        public int TenantId { get; set; }
    }

    public class RejectAssignmentRequest
    {
        public string? RejectionReason { get; set; }
    }
}