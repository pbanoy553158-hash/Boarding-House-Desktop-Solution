using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;

namespace PBCRM2.Infrastructure.Data.Seed;

public static class TenantCRMSeeder
{
    // ============================================================
    // TARGET DATA
    // ============================================================

    // Existing/main tenant target.
    // This is NOT the target for the new branch.
    private const int ExistingTenantTarget = 200;

    // New branch must have exactly 60 tenants.
    private const int NewBranchTenantTarget = 60;

    // Each branch gets its own 20 rooms when being seeded.
    private const int TargetRoomCount = 20;

    private const int SharedRoomCapacity = 4;
    private const int PrivateRoomCapacity = 1;

    // ============================================================
    // EXISTING ROOM NUMBERS
    // ============================================================

    // Existing rooms:
    // 101 - 120
    //
    // Private:
    // 111 - 115
    private const int ExistingRoomStart = 101;
    private const int ExistingPrivateRoomStart = 111;
    private const int ExistingPrivateRoomEnd = 115;

    // ============================================================
    // NEW BRANCH ROOM NUMBERS
    // ============================================================

    // New branch:
    // 201 - 220
    //
    // Private:
    // 211 - 215
    private const int NewBranchRoomStart = 201;
    private const int NewBranchPrivateRoomStart = 211;
    private const int NewBranchPrivateRoomEnd = 215;

    // ============================================================
    // MAIN SEED METHOD
    // ============================================================

    public static async Task SeedAsync(
        TenantCRMDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("=================================================");
        Console.WriteLine("TENANT CRM SEEDER");
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
                "ERROR: Cannot connect to the Tenant CRM database.");

            return;
        }

        // ========================================================
        // GET ACTIVE BRANCHES
        // ========================================================

        var branches =
            await context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Id)
                .ToListAsync();

        if (branches.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine(
                "WARNING: No active branches were found.");

            Console.WriteLine(
                "Tenant CRM seeding cannot continue.");

            Console.WriteLine(
                "Please create at least one active branch.");

            Console.WriteLine("=================================================");

            return;
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Active branches found: {branches.Count}");

        foreach (var branch in branches)
        {
            Console.WriteLine(
                $"Branch {branch.Id}: {branch.BranchName}");
        }

        // ========================================================
        // DETERMINE NEWEST BRANCH
        // ========================================================

        var newestBranch =
            branches
                .OrderByDescending(b => b.Id)
                .First();

        Console.WriteLine();
        Console.WriteLine(
            "-------------------------------------------------");

        Console.WriteLine(
            $"NEWEST BRANCH: {newestBranch.BranchName}");

        Console.WriteLine(
            $"NEWEST BRANCH ID: {newestBranch.Id}");

        Console.WriteLine(
            "-------------------------------------------------");

        // ========================================================
        // SEED EXISTING TENANTS
        // ========================================================

        await SeedExistingTenantsAsync(
            context,
            branches,
            newestBranch.Id);

        // ========================================================
        // SEED 60 TENANTS FOR NEWEST BRANCH
        // ========================================================

        await SeedNewBranchTenantsAsync(
            context,
            newestBranch);

        // ========================================================
        // SEED ROOMS FOR NEWEST BRANCH
        // ========================================================

        await SeedNewBranchRoomsAsync(
            context,
            newestBranch);

        // ========================================================
        // SEED BEDS FOR NEWEST BRANCH
        // ========================================================

        await SeedNewBranchBedsAsync(
            context,
            newestBranch.Id);

        // ========================================================
        // ASSIGN NEW BRANCH TENANTS TO BEDS
        // ========================================================

        await AssignNewBranchTenantsToBedsAsync(
            context,
            newestBranch.Id);

        // ========================================================
        // UPDATE BED STATUS
        // ========================================================

        await UpdateBedStatusesAsync(
            context);

        // ========================================================
        // UPDATE ROOM STATUS
        // ========================================================

        await UpdateRoomStatusesAsync(
            context);

        // ========================================================
        // FINAL SUMMARY
        // ========================================================

        await PrintFinalSummaryAsync(
            context);

        Console.WriteLine();
        Console.WriteLine("=================================================");
        Console.WriteLine("TENANT CRM SEEDING COMPLETED");
        Console.WriteLine("=================================================");
        Console.WriteLine();
    }

    // ============================================================
    // EXISTING TENANT SEEDER
    // ============================================================

    private static async Task SeedExistingTenantsAsync(
        TenantCRMDbContext context,
        List<Branch> branches,
        int newestBranchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("CHECKING EXISTING TENANTS");
        Console.WriteLine("-----------------------------------------------");

        var currentCount =
            await context.Tenants.CountAsync();

        Console.WriteLine(
            $"Existing tenants: {currentCount}");

        // ========================================================
        // IF ORIGINAL 200 ALREADY EXIST
        // ========================================================

        if (currentCount >= ExistingTenantTarget)
        {
            Console.WriteLine(
                $"Existing tenant target already reached: {currentCount}");

            return;
        }

        var random =
            new Random(20260923);

        var firstNames =
            GetFirstNames();

        var lastNames =
            GetLastNames();

        var existingEmails =
            await context.Tenants
                .Select(t => t.Email)
                .ToListAsync();

        var emailSet =
            new HashSet<string>(
                existingEmails,
                StringComparer.OrdinalIgnoreCase);

        var tenantsToCreate =
            ExistingTenantTarget -
            currentCount;

        Console.WriteLine(
            $"Existing tenants to create: {tenantsToCreate}");

        for (
            int i = 0;
            i < tenantsToCreate;
            i++)
        {
            var firstName =
                firstNames[
                    random.Next(
                        firstNames.Length)];

            var lastName =
                lastNames[
                    random.Next(
                        lastNames.Length)];

            var fullName =
                $"{firstName} {lastName}";

            var emailNumber =
                currentCount +
                i +
                1;

            var email =
                $"tenant{emailNumber}@example.com";

            while (
                emailSet.Contains(email))
            {
                emailNumber++;

                email =
                    $"tenant{emailNumber}@example.com";
            }

            emailSet.Add(email);

            var dateOfBirth =
                DateTime.Today
                    .AddYears(
                        -random.Next(18, 46))
                    .AddDays(
                        -random.Next(0, 365));

            // ====================================================
            // IMPORTANT
            // ====================================================
            //
            // Do not use the newest branch for the original
            // 200-tenant pool.
            //
            // This keeps the new branch's 60 tenants separate.
            // ====================================================

            var existingBranches =
                branches
                    .Where(
                        b =>
                            b.Id != newestBranchId)
                    .ToList();

            Branch branch;

            if (existingBranches.Count > 0)
            {
                branch =
                    existingBranches[
                        random.Next(
                            existingBranches.Count)];
            }
            else
            {
                branch =
                    branches.First(
                        b =>
                            b.Id ==
                            newestBranchId);
            }

            var moveInDate =
                DateTime.Today
                    .AddDays(
                        -random.Next(1, 730));

            var tenant =
                new Tenant
                {
                    FullName =
                        fullName,

                    DateOfBirth =
                        dateOfBirth,

                    Sex =
                        GetSex(firstName),

                    ContactNumber =
                        GeneratePhoneNumber(
                            emailNumber),

                    Email =
                        email,

                    CurrentAddress =
                        GenerateAddress(
                            emailNumber),

                    EmergencyContactName =
                        $"{firstNames[random.Next(firstNames.Length)]} " +
                        $"{lastNames[random.Next(lastNames.Length)]}",

                    EmergencyRelationship =
                        GetEmergencyRelationship(
                            random),

                    EmergencyContactNumber =
                        GenerateEmergencyPhoneNumber(
                            emailNumber),

                    BranchId =
                        branch.Id,

                    MoveInDate =
                        moveInDate,

                    ActualMoveOutDate =
                        null,

                    Status =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };

            context.Tenants.Add(
                tenant);
        }

        await context.SaveChangesAsync();

        var finalCount =
            await context.Tenants.CountAsync();

        Console.WriteLine(
            $"Tenants after existing-data seeding: {finalCount}");
    }

    // ============================================================
    // NEW BRANCH - 60 TENANTS
    // ============================================================

    private static async Task SeedNewBranchTenantsAsync(
        TenantCRMDbContext context,
        Branch branch)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("SEEDING 60 TENANTS FOR NEW BRANCH");
        Console.WriteLine("-----------------------------------------------");

        // ========================================================
        // CURRENT COUNT FOR THIS BRANCH
        // ========================================================

        var currentCount =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.BranchId ==
                        branch.Id);

        Console.WriteLine(
            $"Current tenants in new branch: {currentCount}");

        // ========================================================
        // TARGET ALREADY REACHED
        // ========================================================

        if (currentCount >= NewBranchTenantTarget)
        {
            Console.WriteLine(
                $"New branch already has {currentCount} tenants.");

            return;
        }

        // ========================================================
        // DATA
        // ========================================================

        var random =
            new Random(
                20260924 + branch.Id);

        var firstNames =
            GetFirstNames();

        var lastNames =
            GetLastNames();

        var existingEmails =
            await context.Tenants
                .Select(t => t.Email)
                .ToListAsync();

        var emailSet =
            new HashSet<string>(
                existingEmails,
                StringComparer.OrdinalIgnoreCase);

        // ========================================================
        // CREATE REMAINING TENANTS
        // ========================================================

        var tenantsToCreate =
            NewBranchTenantTarget -
            currentCount;

        Console.WriteLine(
            $"New branch tenants to create: {tenantsToCreate}");

        for (
            int i = 0;
            i < tenantsToCreate;
            i++)
        {
            var firstName =
                firstNames[
                    random.Next(
                        firstNames.Length)];

            var lastName =
                lastNames[
                    random.Next(
                        lastNames.Length)];

            var fullName =
                $"{firstName} {lastName}";

            // ====================================================
            // UNIQUE EMAIL
            // ====================================================

            var emailNumber =
                1000 +
                branch.Id * 100 +
                currentCount +
                i +
                1;

            var email =
                $"newbranch{emailNumber}@example.com";

            while (
                emailSet.Contains(email))
            {
                emailNumber++;

                email =
                    $"newbranch{emailNumber}@example.com";
            }

            emailSet.Add(email);

            // ====================================================
            // PERSONAL DATA
            // ====================================================

            var dateOfBirth =
                DateTime.Today
                    .AddYears(
                        -random.Next(18, 46))
                    .AddDays(
                        -random.Next(0, 365));

            var moveInDate =
                DateTime.Today
                    .AddDays(
                        -random.Next(1, 365));

            var emergencyFirstName =
                firstNames[
                    random.Next(
                        firstNames.Length)];

            var emergencyLastName =
                lastNames[
                    random.Next(
                        lastNames.Length)];

            // ====================================================
            // CREATE TENANT
            // ====================================================

            var tenant =
                new Tenant
                {
                    FullName =
                        fullName,

                    DateOfBirth =
                        dateOfBirth,

                    Sex =
                        GetSex(firstName),

                    ContactNumber =
                        GeneratePhoneNumber(
                            emailNumber),

                    Email =
                        email,

                    CurrentAddress =
                        GenerateAddress(
                            emailNumber),

                    EmergencyContactName =
                        $"{emergencyFirstName} " +
                        $"{emergencyLastName}",

                    EmergencyRelationship =
                        GetEmergencyRelationship(
                            random),

                    EmergencyContactNumber =
                        GenerateEmergencyPhoneNumber(
                            emailNumber),

                    // IMPORTANT:
                    // Every one of these 60 tenants belongs
                    // specifically to the newest branch.
                    BranchId =
                        branch.Id,

                    MoveInDate =
                        moveInDate,

                    ActualMoveOutDate =
                        null,

                    Status =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };

            context.Tenants.Add(
                tenant);
        }

        await context.SaveChangesAsync();

        var finalCount =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.BranchId ==
                        branch.Id);

        Console.WriteLine(
            $"Tenants in new branch after seeding: {finalCount}");
    }

    // ============================================================
    // NEW BRANCH - ROOM SEEDER
    // ============================================================

    private static async Task SeedNewBranchRoomsAsync(
        TenantCRMDbContext context,
        Branch branch)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("SEEDING ROOMS FOR NEW BRANCH");
        Console.WriteLine("-----------------------------------------------");

        var existingRooms =
            await context.Rooms
                .Where(
                    r =>
                        r.BranchId ==
                        branch.Id)
                .OrderBy(
                    r =>
                        r.RoomNumber)
                .ToListAsync();

        Console.WriteLine(
            $"Existing rooms in new branch: {existingRooms.Count}");

        // ========================================================
        // NORMALIZE EXISTING ROOMS
        // ========================================================

        foreach (var room in existingRooms)
        {
            var roomNumber =
                ParseRoomNumber(
                    room.RoomNumber);

            room.RoomType =
                GetNewBranchRoomType(
                    roomNumber);

            room.Capacity =
                GetNewBranchRoomCapacity(
                    roomNumber);

            room.IsActive =
                true;
        }

        await context.SaveChangesAsync();

        // ========================================================
        // GET ALL ROOM NUMBERS
        // ========================================================

        var existingRoomNumbers =
            new HashSet<string>(
                await context.Rooms
                    .Select(
                        r =>
                            r.RoomNumber)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

        // ========================================================
        // CREATE MISSING ROOMS
        // ========================================================

        int createdRooms = 0;

        for (
            int i = 0;
            i < TargetRoomCount;
            i++)
        {
            var roomNumber =
                (
                    NewBranchRoomStart +
                    i)
                .ToString();

            // ----------------------------------------------------
            // Already exists somewhere in database
            // ----------------------------------------------------

            if (existingRoomNumbers.Contains(
                    roomNumber))
            {
                var existingRoom =
                    await context.Rooms
                        .FirstOrDefaultAsync(
                            r =>
                                r.RoomNumber ==
                                roomNumber &&
                                r.BranchId ==
                                branch.Id);

                if (existingRoom != null)
                {
                    continue;
                }

                // Same room number belongs to another branch.
                // Do not create a duplicate room number.
                Console.WriteLine(
                    $"Room {roomNumber} already exists in another branch.");

                continue;
            }

            var number =
                ParseRoomNumber(
                    roomNumber);

            var roomType =
                GetNewBranchRoomType(
                    number);

            var capacity =
                GetNewBranchRoomCapacity(
                    number);

            var room =
                new Room
                {
                    BranchId =
                        branch.Id,

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

            context.Rooms.Add(
                room);

            existingRoomNumbers.Add(
                roomNumber);

            createdRooms++;
        }

        await context.SaveChangesAsync();

        var finalRoomCount =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.BranchId ==
                        branch.Id);

        Console.WriteLine(
            $"Rooms created for new branch: {createdRooms}");

        Console.WriteLine(
            $"Total rooms in new branch: {finalRoomCount}");

        Console.WriteLine(
            $"Shared rooms: " +
            await context.Rooms.CountAsync(
                r =>
                    r.BranchId ==
                        branch.Id &&
                    r.RoomType ==
                        "Shared"));

        Console.WriteLine(
            $"Private rooms: " +
            await context.Rooms.CountAsync(
                r =>
                    r.BranchId ==
                        branch.Id &&
                    r.RoomType ==
                        "Private"));
    }

    // ============================================================
    // NEW BRANCH - BED SEEDER
    // ============================================================

    private static async Task SeedNewBranchBedsAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("SEEDING BEDS FOR NEW BRANCH");
        Console.WriteLine("-----------------------------------------------");

        var rooms =
            await context.Rooms
                .Where(
                    r =>
                        r.BranchId ==
                            branchId &&
                        r.IsActive)
                .OrderBy(
                    r =>
                        r.RoomNumber)
                .ToListAsync();

        var roomIds =
            rooms
                .Select(
                    r =>
                        r.Id)
                .ToList();

        var existingBeds =
            await context.Beds
                .Where(
                    b =>
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
                GetNewBranchRoomCapacity(
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
            $"Beds created for new branch: {createdBeds}");

        Console.WriteLine(
            $"Total beds in new branch: {finalBedCount}");
    }

    // ============================================================
    // NEW BRANCH - ASSIGN TENANTS TO BEDS
    // ============================================================

    private static async Task AssignNewBranchTenantsToBedsAsync(
        TenantCRMDbContext context,
        int branchId)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("ASSIGNING NEW BRANCH TENANTS TO BEDS");
        Console.WriteLine("-----------------------------------------------");

        // ========================================================
        // GET TENANTS
        // ========================================================

        var tenants =
            await context.Tenants
                .Where(
                    t =>
                        t.BranchId ==
                            branchId &&
                        t.Status ==
                            "Active")
                .OrderBy(
                    t =>
                        t.Id)
                .ToListAsync();

        // ========================================================
        // GET BRANCH ROOMS
        // ========================================================

        var roomIds =
            await context.Rooms
                .Where(
                    r =>
                        r.BranchId ==
                            branchId &&
                        r.IsActive)
                .Select(
                    r =>
                        r.Id)
                .ToListAsync();

        // ========================================================
        // GET BEDS
        // ========================================================

        var beds =
            await context.Beds
                .Include(
                    b =>
                        b.Room)
                .Where(
                    b =>
                        roomIds.Contains(
                            b.RoomId))
                .OrderBy(
                    b =>
                        b.RoomId)
                .ThenBy(
                    b =>
                        b.Id)
                .ToListAsync();

        // ========================================================
        // EXISTING ASSIGNMENTS
        // ========================================================

        var assignedTenantIds =
            new HashSet<int>(
                beds
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
            beds
                .Where(
                    b =>
                        b.TenantId == null)
                .ToList();

        Console.WriteLine(
            $"New branch tenants: {tenants.Count}");

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
            // Already assigned
            // ----------------------------------------------------

            if (assignedTenantIds.Contains(
                    tenant.Id))
            {
                continue;
            }

            // ----------------------------------------------------
            // Find available bed in same branch
            // ----------------------------------------------------

            var bed =
                availableBeds.FirstOrDefault(
                    b =>
                        b.Room != null &&
                        b.Room.BranchId ==
                            branchId);

            if (bed == null)
            {
                Console.WriteLine(
                    $"No available bed for: " +
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
    // UPDATE ALL BED STATUSES
    // ============================================================

    private static async Task UpdateBedStatusesAsync(
        TenantCRMDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("UPDATING BED STATUSES");
        Console.WriteLine("-----------------------------------------------");

        var beds =
            await context.Beds
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
    // UPDATE ALL ROOM STATUSES
    // ============================================================

    private static async Task UpdateRoomStatusesAsync(
        TenantCRMDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("UPDATING ROOM STATUSES");
        Console.WriteLine("-----------------------------------------------");

        var rooms =
            await context.Rooms
                .Include(
                    r =>
                        r.Beds)
                .Where(
                    r =>
                        r.IsActive)
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

            var totalBeds =
                room.Beds.Count;

            if (totalBeds == 0)
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
        TenantCRMDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("FINAL TENANT CRM SUMMARY");
        Console.WriteLine("-----------------------------------------------");

        var branches =
            await context.Branches
                .CountAsync();

        var activeBranches =
            await context.Branches
                .CountAsync(
                    b =>
                        b.IsActive);

        var tenants =
            await context.Tenants
                .CountAsync();

        var activeTenants =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.Status ==
                        "Active");

        var movedOutTenants =
            await context.Tenants
                .CountAsync(
                    t =>
                        t.Status ==
                        "Moved Out");

        var rooms =
            await context.Rooms
                .CountAsync();

        var sharedRooms =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.RoomType ==
                        "Shared");

        var privateRooms =
            await context.Rooms
                .CountAsync(
                    r =>
                        r.RoomType ==
                        "Private");

        var beds =
            await context.Beds
                .CountAsync();

        var occupiedBeds =
            await context.Beds
                .CountAsync(
                    b =>
                        b.TenantId !=
                        null);

        var availableBeds =
            await context.Beds
                .CountAsync(
                    b =>
                        b.TenantId ==
                        null);

        var tenantsWithoutBeds =
            await context.Tenants
                .Where(
                    t =>
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
            $"Branches: {branches}");

        Console.WriteLine(
            $"Active branches: {activeBranches}");

        Console.WriteLine(
            $"Total tenants: {tenants}");

        Console.WriteLine(
            $"Active tenants: {activeTenants}");

        Console.WriteLine(
            $"Moved Out tenants: {movedOutTenants}");

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
        // NEWEST BRANCH SUMMARY
        // ========================================================

        var newestBranch =
            await context.Branches
                .Where(
                    b =>
                        b.IsActive)
                .OrderByDescending(
                    b =>
                        b.Id)
                .FirstOrDefaultAsync();

        if (newestBranch != null)
        {
            var branchTenantCount =
                await context.Tenants
                    .CountAsync(
                        t =>
                            t.BranchId ==
                            newestBranch.Id);

            var branchActiveTenants =
                await context.Tenants
                    .CountAsync(
                        t =>
                            t.BranchId ==
                                newestBranch.Id &&
                            t.Status ==
                                "Active");

            var branchRoomIds =
                await context.Rooms
                    .Where(
                        r =>
                            r.BranchId ==
                            newestBranch.Id)
                    .Select(
                        r =>
                            r.Id)
                    .ToListAsync();

            var branchBeds =
                await context.Beds
                    .Where(
                        b =>
                            branchRoomIds.Contains(
                                b.RoomId))
                    .ToListAsync();

            var branchOccupiedBeds =
                branchBeds.Count(
                    b =>
                        b.TenantId.HasValue);

            var branchAvailableBeds =
                branchBeds.Count(
                    b =>
                        !b.TenantId.HasValue);

            var branchRooms =
                await context.Rooms
                    .CountAsync(
                        r =>
                            r.BranchId ==
                            newestBranch.Id);

            Console.WriteLine();
            Console.WriteLine(
                "NEWEST BRANCH BREAKDOWN");

            Console.WriteLine("-----------------------------------------------");

            Console.WriteLine(
                $"Branch: {newestBranch.BranchName}");

            Console.WriteLine(
                $"Branch ID: {newestBranch.Id}");

            Console.WriteLine(
                $"Tenants: {branchTenantCount}");

            Console.WriteLine(
                $"Active tenants: {branchActiveTenants}");

            Console.WriteLine(
                $"Rooms: {branchRooms}");

            Console.WriteLine(
                $"Beds: {branchBeds.Count}");

            Console.WriteLine(
                $"Occupied beds: {branchOccupiedBeds}");

            Console.WriteLine(
                $"Available beds: {branchAvailableBeds}");

            Console.WriteLine("-----------------------------------------------");
        }

        // ========================================================
        // ROOM BREAKDOWN
        // ========================================================

        var roomBreakdown =
            await context.Rooms
                .Include(
                    r =>
                        r.Beds)
                .OrderBy(
                    r =>
                        r.BranchId)
                .ThenBy(
                    r =>
                        r.RoomNumber)
                .ToListAsync();

        foreach (var room in roomBreakdown)
        {
            var occupied =
                room.Beds.Count(
                    b =>
                        b.TenantId !=
                        null);

            var available =
                room.Beds.Count(
                    b =>
                        b.TenantId ==
                        null);

            Console.WriteLine(
                $"Branch {room.BranchId} | " +
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
    // NEW BRANCH ROOM TYPE
    // ============================================================

    private static string GetNewBranchRoomType(
        int roomNumber)
    {
        if (
            roomNumber >=
                NewBranchPrivateRoomStart &&
            roomNumber <=
                NewBranchPrivateRoomEnd)
        {
            return "Private";
        }

        return "Shared";
    }

    // ============================================================
    // NEW BRANCH ROOM CAPACITY
    // ============================================================

    private static int GetNewBranchRoomCapacity(
        int roomNumber)
    {
        if (
            roomNumber >=
                NewBranchPrivateRoomStart &&
            roomNumber <=
                NewBranchPrivateRoomEnd)
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

    // ============================================================
    // SEX
    // ============================================================

    private static string GetSex(
        string firstName)
    {
        var femaleNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "Maria",
                "Angela",
                "Sophia",
                "Mia",
                "Isabella",
                "Alyssa",
                "Nicole",
                "Samantha",
                "Hannah",
                "Andrea",
                "Jasmine",
                "Christine",
                "Stephanie",
                "Patricia",
                "Camille",
                "Bianca",
                "Clarisse",
                "Danielle",
                "Katrina",
                "Beatrice"
            };

        return femaleNames.Contains(
            firstName)
            ? "Female"
            : "Male";
    }

    // ============================================================
    // EMERGENCY RELATIONSHIP
    // ============================================================

    private static string GetEmergencyRelationship(
        Random random)
    {
        var relationships =
            new[]
            {
                "Mother",
                "Father",
                "Sibling",
                "Spouse",
                "Relative",
                "Guardian"
            };

        return relationships[
            random.Next(
                relationships.Length)];
    }

    // ============================================================
    // PHONE NUMBER
    // ============================================================

    private static string GeneratePhoneNumber(
        int number)
    {
        var suffix =
            number
                .ToString()
                .PadLeft(
                    4,
                    '0');

        return
            $"+63917{suffix}";
    }

    // ============================================================
    // EMERGENCY PHONE NUMBER
    // ============================================================

    private static string GenerateEmergencyPhoneNumber(
        int number)
    {
        var suffix =
            (number + 5000)
                .ToString()
                .PadLeft(
                    4,
                    '0');

        return
            $"+63918{suffix}";
    }

    // ============================================================
    // ADDRESS
    // ============================================================

    private static string GenerateAddress(
        int number)
    {
        var areas =
            new[]
            {
                "Poblacion",
                "Matina",
                "Buhangin",
                "Agdao",
                "Talomo",
                "J.P. Laurel",
                "Bangkal",
                "Toril"
            };

        var area =
            areas[
                number %
                areas.Length];

        return
            $"Purok {(number % 10) + 1}, " +
            $"{area}, Davao City, Davao del Sur";
    }

    // ============================================================
    // FIRST NAMES
    // ============================================================

    private static string[] GetFirstNames()
    {
        return
        [
            "James",
            "John",
            "Michael",
            "Daniel",
            "Joshua",
            "Matthew",
            "Andrew",
            "Christian",
            "Joseph",
            "Anthony",
            "Mark",
            "Kevin",
            "Ryan",
            "Nathan",
            "Patrick",
            "Carlo",
            "Miguel",
            "Adrian",
            "Gabriel",
            "Francis",

            "Maria",
            "Angela",
            "Sophia",
            "Mia",
            "Isabella",
            "Alyssa",
            "Nicole",
            "Samantha",
            "Hannah",
            "Andrea",
            "Jasmine",
            "Christine",
            "Stephanie",
            "Patricia",
            "Camille",
            "Bianca",
            "Clarisse",
            "Danielle",
            "Katrina",
            "Beatrice"
        ];
    }

    // ============================================================
    // LAST NAMES
    // ============================================================

    private static string[] GetLastNames()
    {
        return
        [
            "Santos",
            "Reyes",
            "Cruz",
            "Garcia",
            "Mendoza",
            "Dela Cruz",
            "Torres",
            "Flores",
            "Bautista",
            "Navarro",
            "Ramos",
            "Castillo",
            "Aquino",
            "Rivera",
            "Villanueva",
            "Fernandez",
            "Gonzales",
            "Pascual",
            "Mercado",
            "Salazar",
            "Domingo",
            "Manalo",
            "Morales",
            "Aguilar",
            "Serrano",
            "Valdez",
            "Lim",
            "Tan",
            "Chua",
            "Sy"
        ];
    }
}