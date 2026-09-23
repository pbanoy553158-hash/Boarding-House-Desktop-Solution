using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;

namespace PBCRM2.Infrastructure.Data.Seed;

public static class NewBranchRoomBedSeeder
{
    // ============================================================
    // TARGET CONFIGURATION
    // ============================================================

    private const int TargetTenantCount = 60;
    private const int TargetRoomCount = 20;

    private const int SharedRoomCapacity = 4;
    private const int PrivateRoomCapacity = 1;

    // Private rooms:
    // 111, 112, 113, 114, 115
    private const int PrivateRoomStart = 111;
    private const int PrivateRoomEnd = 115;

    // New branch room numbers
    // 101 - 120
    private const int RoomNumberStart = 101;

    // ============================================================
    // MAIN SEED METHOD
    // ============================================================

    public static async Task SeedAsync(
        TenantCRMDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("=================================================");
        Console.WriteLine("NEW BRANCH ROOM / BED SEEDER");
        Console.WriteLine("=================================================");

        var connection =
            context.Database.GetDbConnection();

        Console.WriteLine(
            $"Database: {connection.Database}");

        Console.WriteLine(
            $"Server: {connection.DataSource}");

        Console.WriteLine("=================================================");

        // ========================================================
        // DATABASE CHECK
        // ========================================================

        if (!await context.Database.CanConnectAsync())
        {
            Console.WriteLine(
                "ERROR: Cannot connect to Tenant CRM database.");

            return;
        }

        // ========================================================
        // GET NEWEST ACTIVE BRANCH
        // ========================================================

        var branch =
            await context.Branches
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

        if (branch == null)
        {
            Console.WriteLine();
            Console.WriteLine(
                "ERROR: No active branch found.");

            Console.WriteLine(
                "Create the new branch first.");

            return;
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Target Branch: {branch.BranchName}");

        Console.WriteLine(
            $"Branch ID: {branch.Id}");

        // ========================================================
        // GET TENANTS BELONGING TO THIS BRANCH
        // ========================================================

        var tenants =
            await context.Tenants
                .Where(t =>
                    t.BranchId == branch.Id &&
                    t.Status == "Active")
                .OrderBy(t => t.Id)
                .ToListAsync();

        Console.WriteLine();
        Console.WriteLine(
            $"Active tenants in branch: {tenants.Count}");

        if (tenants.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine(
                "WARNING: No active tenants were found for this branch.");

            Console.WriteLine(
                "Seed the 60 tenants first.");

            return;
        }

        // ========================================================
        // CREATE / NORMALIZE ROOMS
        // ========================================================

        await SeedRoomsAsync(
            context,
            branch.Id);

        // ========================================================
        // CREATE / NORMALIZE BEDS
        // ========================================================

        await SeedBedsAsync(
            context,
            branch.Id);

        // ========================================================
        // ASSIGN TENANTS
        // ========================================================

        await AssignTenantsToBedsAsync(
            context,
            branch.Id,
            tenants);

        // ========================================================
        // UPDATE BED STATUS
        // ========================================================

        await UpdateBedStatusesAsync(
            context,
            branch.Id);

        // ========================================================
        // UPDATE ROOM STATUS
        // ========================================================

        await UpdateRoomStatusesAsync(
            context,
            branch.Id);

        // ========================================================
        // FINAL SUMMARY
        // ========================================================

        await PrintFinalSummaryAsync(
            context,
            branch.Id);

        Console.WriteLine();
        Console.WriteLine("=================================================");
        Console.WriteLine("NEW BRANCH ROOM / BED SEEDING COMPLETED");
        Console.WriteLine("=================================================");
        Console.WriteLine();
    }

    // ============================================================
    // ROOM SEEDER
    // ============================================================

    private static async Task SeedRoomsAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("SEEDING ROOMS FOR NEW BRANCH");
        Console.WriteLine("-----------------------------------------------");

        var branchRooms =
            await context.Rooms
                .Where(r =>
                    r.BranchId == branchId)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

        Console.WriteLine(
            $"Existing rooms in branch: {branchRooms.Count}");

        // ========================================================
        // NORMALIZE EXISTING ROOMS
        // ========================================================

        foreach (var room in branchRooms)
        {
            var roomNumber =
                ParseRoomNumber(
                    room.RoomNumber);

            room.RoomType =
                GetRoomType(
                    roomNumber);

            room.Capacity =
                GetRoomCapacity(
                    roomNumber);

            room.IsActive =
                true;
        }

        await context.SaveChangesAsync();

        // ========================================================
        // CREATE MISSING ROOMS
        // ========================================================

        var existingRoomNumbers =
            new HashSet<string>(
                branchRooms.Select(
                    r => r.RoomNumber),
                StringComparer.OrdinalIgnoreCase);

        int createdRooms = 0;

        for (
            int i = 0;
            i < TargetRoomCount;
            i++)
        {
            var roomNumber =
                (RoomNumberStart + i)
                .ToString();

            if (existingRoomNumbers.Contains(
                    roomNumber))
            {
                continue;
            }

            var number =
                ParseRoomNumber(
                    roomNumber);

            var roomType =
                GetRoomType(
                    number);

            var capacity =
                GetRoomCapacity(
                    number);

            var room =
                new Room
                {
                    BranchId =
                        branchId,

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

            context.Rooms.Add(room);

            createdRooms++;

            existingRoomNumbers.Add(
                roomNumber);
        }

        await context.SaveChangesAsync();

        Console.WriteLine(
            $"Rooms created: {createdRooms}");

        var finalRoomCount =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.BranchId ==
                        branchId);

        Console.WriteLine(
            $"Total rooms in branch: {finalRoomCount}");

        Console.WriteLine(
            $"Shared rooms: " +
            await context.Rooms.CountAsync(
                r =>
                    r.BranchId == branchId &&
                    r.RoomType == "Shared"));

        Console.WriteLine(
            $"Private rooms: " +
            await context.Rooms.CountAsync(
                r =>
                    r.BranchId == branchId &&
                    r.RoomType == "Private"));
    }

    // ============================================================
    // BED SEEDER
    // ============================================================

    private static async Task SeedBedsAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("SEEDING BEDS FOR NEW BRANCH");
        Console.WriteLine("-----------------------------------------------");

        var rooms =
            await context.Rooms
                .Where(r =>
                    r.BranchId == branchId &&
                    r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

        var roomIds =
            rooms
                .Select(r => r.Id)
                .ToList();

        var existingBeds =
            await context.Beds
                .Where(b =>
                    roomIds.Contains(
                        b.RoomId))
                .ToListAsync();

        var existingBedKeys =
            new HashSet<string>(
                existingBeds.Select(
                    b =>
                        $"{b.RoomId}:{b.BedNumber}"),
                StringComparer.OrdinalIgnoreCase);

        int createdBeds = 0;

        // ========================================================
        // CREATE BEDS
        // ========================================================

        foreach (var room in rooms)
        {
            room.Capacity =
                GetRoomCapacity(
                    ParseRoomNumber(
                        room.RoomNumber));

            for (
                int bedIndex = 1;
                bedIndex <= room.Capacity;
                bedIndex++)
            {
                var bedLetter =
                    ((char)('A' + bedIndex - 1))
                    .ToString();

                var bedNumber =
                    $"{room.RoomNumber}-{bedLetter}";

                var bedKey =
                    $"{room.Id}:{bedNumber}";

                if (existingBedKeys.Contains(
                        bedKey))
                {
                    continue;
                }

                var bed =
                    new Bed
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

                context.Beds.Add(
                    bed);

                existingBedKeys.Add(
                    bedKey);

                createdBeds++;
            }
        }

        await context.SaveChangesAsync();

        var finalBedCount =
            await context.Beds
                .CountAsync(
                    b =>
                        roomIds.Contains(
                            b.RoomId));

        Console.WriteLine(
            $"Beds created: {createdBeds}");

        Console.WriteLine(
            $"Total beds in branch: {finalBedCount}");
    }

    // ============================================================
    // ASSIGN TENANTS TO BEDS
    // ============================================================

    private static async Task AssignTenantsToBedsAsync(
        TenantCRMDbContext context,
        int branchId,
        List<Tenant> tenants)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("ASSIGNING TENANTS TO NEW BRANCH BEDS");
        Console.WriteLine("-----------------------------------------------");

        // ========================================================
        // GET BRANCH ROOMS
        // ========================================================

        var roomIds =
            await context.Rooms
                .Where(r =>
                    r.BranchId == branchId &&
                    r.IsActive)
                .Select(r => r.Id)
                .ToListAsync();

        // ========================================================
        // GET CURRENT BED ASSIGNMENTS
        // ========================================================

        var branchBeds =
            await context.Beds
                .Include(b => b.Room)
                .Where(b =>
                    roomIds.Contains(
                        b.RoomId))
                .OrderBy(b => b.RoomId)
                .ThenBy(b => b.Id)
                .ToListAsync();

        // ========================================================
        // FIND TENANTS ALREADY ASSIGNED
        // ========================================================

        var assignedTenantIds =
            new HashSet<int>(
                branchBeds
                    .Where(
                        b =>
                            b.TenantId.HasValue)
                    .Select(
                        b =>
                            b.TenantId!.Value));

        // ========================================================
        // AVAILABLE BEDS
        // ========================================================

        var availableBeds =
            branchBeds
                .Where(
                    b =>
                        b.TenantId == null)
                .ToList();

        Console.WriteLine(
            $"Branch tenants: {tenants.Count}");

        Console.WriteLine(
            $"Already assigned: {assignedTenantIds.Count}");

        Console.WriteLine(
            $"Available beds: {availableBeds.Count}");

        // ========================================================
        // ASSIGN
        // ========================================================

        int assignedCount = 0;

        foreach (var tenant in tenants)
        {
            // ----------------------------------------------------
            // ALREADY HAS A BED
            // ----------------------------------------------------

            if (assignedTenantIds.Contains(
                    tenant.Id))
            {
                continue;
            }

            // ----------------------------------------------------
            // FIND BED IN SAME BRANCH
            // ----------------------------------------------------

            var bed =
                availableBeds.FirstOrDefault(
                    b =>
                        b.Room != null &&
                        b.Room.BranchId ==
                            tenant.BranchId);

            if (bed == null)
            {
                Console.WriteLine(
                    $"No available bed for tenant: " +
                    $"{tenant.FullName}");

                continue;
            }

            // ----------------------------------------------------
            // ASSIGN
            // ----------------------------------------------------

            bed.TenantId =
                tenant.Id;

            bed.Status =
                "Occupied";

            assignedTenantIds.Add(
                tenant.Id);

            availableBeds.Remove(
                bed);

            assignedCount++;
        }

        await context.SaveChangesAsync();

        Console.WriteLine(
            $"New tenant-bed assignments: {assignedCount}");
    }

    // ============================================================
    // UPDATE BED STATUS
    // ============================================================

    private static async Task UpdateBedStatusesAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("UPDATING BED STATUSES");
        Console.WriteLine("-----------------------------------------------");

        var beds =
            await context.Beds
                .Include(b => b.Room)
                .Where(
                    b =>
                        b.Room != null &&
                        b.Room.BranchId ==
                            branchId)
                .ToListAsync();

        foreach (var bed in beds)
        {
            if (bed.TenantId.HasValue)
            {
                bed.Status =
                    "Occupied";
            }
            else
            {
                bed.Status =
                    "Available";
            }
        }

        await context.SaveChangesAsync();

        var occupied =
            beds.Count(
                b =>
                    b.TenantId.HasValue);

        var available =
            beds.Count(
                b =>
                    !b.TenantId.HasValue);

        Console.WriteLine(
            $"Occupied beds: {occupied}");

        Console.WriteLine(
            $"Available beds: {available}");
    }

    // ============================================================
    // UPDATE ROOM STATUS
    // ============================================================

    private static async Task UpdateRoomStatusesAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("UPDATING ROOM STATUSES");
        Console.WriteLine("-----------------------------------------------");

        var rooms =
            await context.Rooms
                .Include(r => r.Beds)
                .Where(
                    r =>
                        r.BranchId == branchId &&
                        r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

        foreach (var room in rooms)
        {
            if (!room.IsActive)
            {
                room.Status =
                    "Inactive";

                continue;
            }

            // ====================================================
            // MAINTENANCE
            // ====================================================

            if (room.Beds.Any(
                    b =>
                        b.Status ==
                        "Maintenance"))
            {
                room.Status =
                    "Maintenance";

                continue;
            }

            // ====================================================
            // NO BEDS
            // ====================================================

            if (room.Beds.Count == 0)
            {
                room.Status =
                    "Available";

                continue;
            }

            // ====================================================
            // OCCUPIED
            // ====================================================

            var occupiedBeds =
                room.Beds.Count(
                    b =>
                        b.TenantId.HasValue);

            // ====================================================
            // FULL
            // ====================================================

            if (occupiedBeds >= room.Capacity)
            {
                room.Status =
                    "Full";
            }
            else if (occupiedBeds > 0)
            {
                room.Status =
                    "Partially Occupied";
            }
            else
            {
                room.Status =
                    "Available";
            }
        }

        await context.SaveChangesAsync();

        Console.WriteLine(
            $"Rooms updated: {rooms.Count}");
    }

    // ============================================================
    // FINAL SUMMARY
    // ============================================================

    private static async Task PrintFinalSummaryAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("NEW BRANCH FINAL SUMMARY");
        Console.WriteLine("-----------------------------------------------");

        var branch =
            await context.Branches
                .FirstAsync(
                    b =>
                        b.Id ==
                        branchId);

        var tenants =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.BranchId ==
                        branchId);

        var activeTenants =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.BranchId ==
                            branchId &&
                        t.Status ==
                            "Active");

        var rooms =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.BranchId ==
                        branchId);

        var sharedRooms =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.BranchId ==
                            branchId &&
                        r.RoomType ==
                            "Shared");

        var privateRooms =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.BranchId ==
                            branchId &&
                        r.RoomType ==
                            "Private");

        var roomIds =
            await context.Rooms
                .Where(
                    r =>
                        r.BranchId ==
                        branchId)
                .Select(
                    r =>
                        r.Id)
                .ToListAsync();

        var beds =
            await context.Beds
                .CountAsync(
                    b =>
                        roomIds.Contains(
                            b.RoomId));

        var occupiedBeds =
            await context.Beds
                .CountAsync(
                    b =>
                        roomIds.Contains(
                            b.RoomId) &&
                        b.TenantId != null);

        var availableBeds =
            await context.Beds
                .CountAsync(
                    b =>
                        roomIds.Contains(
                            b.RoomId) &&
                        b.TenantId == null);

        var tenantsWithoutBeds =
            await context.Tenants
                .Where(
                    t =>
                        t.BranchId ==
                            branchId &&
                        t.Status ==
                            "Active")
                .Where(
                    t =>
                        !context.Beds.Any(
                            b =>
                                b.TenantId ==
                                t.Id))
                .CountAsync();

        Console.WriteLine(
            $"Branch: {branch.BranchName}");

        Console.WriteLine(
            $"Branch ID: {branch.Id}");

        Console.WriteLine("-----------------------------------------------");

        Console.WriteLine(
            $"Total tenants: {tenants}");

        Console.WriteLine(
            $"Active tenants: {activeTenants}");

        Console.WriteLine(
            $"Total rooms: {rooms}");

        Console.WriteLine(
            $"Shared rooms: {sharedRooms}");

        Console.WriteLine(
            $"Private rooms: {privateRooms}");

        Console.WriteLine(
            $"Total beds: {beds}");

        Console.WriteLine(
            $"Occupied beds: {occupiedBeds}");

        Console.WriteLine(
            $"Available beds: {availableBeds}");

        Console.WriteLine(
            $"Active tenants without beds: " +
            $"{tenantsWithoutBeds}");

        Console.WriteLine("-----------------------------------------------");

        // ========================================================
        // ROOM BREAKDOWN
        // ========================================================

        var roomBreakdown =
            await context.Rooms
                .Include(r => r.Beds)
                .Where(
                    r =>
                        r.BranchId ==
                        branchId)
                .OrderBy(
                    r =>
                        r.RoomNumber)
                .ToListAsync();

        foreach (var room in roomBreakdown)
        {
            var occupied =
                room.Beds.Count(
                    b =>
                        b.TenantId != null);

            var available =
                room.Beds.Count(
                    b =>
                        b.TenantId == null);

            Console.WriteLine(
                $"Room {room.RoomNumber} | " +
                $"{room.RoomType} | " +
                $"Capacity: {room.Capacity} | " +
                $"Beds: {room.Beds.Count} | " +
                $"Occupied: {occupied} | " +
                $"Available: {available} | " +
                $"Status: {room.Status}");
        }

        Console.WriteLine("-----------------------------------------------");
    }

    // ============================================================
    // ROOM TYPE
    // ============================================================

    private static string GetRoomType(
        int roomNumber)
    {
        if (
            roomNumber >= PrivateRoomStart &&
            roomNumber <= PrivateRoomEnd)
        {
            return "Private";
        }

        return "Shared";
    }

    // ============================================================
    // ROOM CAPACITY
    // ============================================================

    private static int GetRoomCapacity(
        int roomNumber)
    {
        if (
            roomNumber >= PrivateRoomStart &&
            roomNumber <= PrivateRoomEnd)
        {
            return PrivateRoomCapacity;
        }

        return SharedRoomCapacity;
    }

    // ============================================================
    // ROOM NUMBER PARSER
    // ============================================================

    private static int ParseRoomNumber(
        string roomNumber)
    {
        if (
            int.TryParse(
                roomNumber,
                out var number))
        {
            return number;
        }

        return 0;
    }
}