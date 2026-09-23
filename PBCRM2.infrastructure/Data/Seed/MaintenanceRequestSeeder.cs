using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBCRM2.Infrastructure.Data.Seed
{
    public static class MaintenanceRequestSeeder
    {
        // ============================================================
        // SETTINGS
        // ============================================================

        private const int TargetRequestCount = 30;

        // ============================================================
        // SEED
        // ============================================================

        public static async Task SeedAsync(TenantCRMDbContext context)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("MAINTENANCE REQUEST SEEDER");
            Console.WriteLine("==============================================");

            // --------------------------------------------------------
            // Get newest active branch
            // --------------------------------------------------------

            var newestBranch = await context.Branches
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            if (newestBranch == null)
            {
                Console.WriteLine("No active branch found.");
                return;
            }

            Console.WriteLine(
                $"Target Branch: {newestBranch.BranchName} (ID: {newestBranch.Id})");

            // --------------------------------------------------------
            // Get tenants from newest branch
            // --------------------------------------------------------

            var tenants = await context.Tenants
                .Where(t =>
                    t.BranchId == newestBranch.Id &&
                    t.Status == "Active")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (tenants.Count == 0)
            {
                Console.WriteLine(
                    "No active tenants found in the newest branch.");

                return;
            }

            Console.WriteLine($"Active Tenants: {tenants.Count}");

            // --------------------------------------------------------
            // Count existing maintenance requests
            // for this branch
            // --------------------------------------------------------

            int existingRequestCount =
                await context.MaintenanceRequests
                    .Where(r =>
                        context.Tenants.Any(t =>
                            t.Id == r.TenantId &&
                            t.BranchId == newestBranch.Id))
                    .CountAsync();

            Console.WriteLine(
                $"Existing Requests: {existingRequestCount}");

            // --------------------------------------------------------
            // Do not duplicate if target already reached
            // --------------------------------------------------------

            if (existingRequestCount >= TargetRequestCount)
            {
                Console.WriteLine(
                    $"Maintenance requests already reached {TargetRequestCount}.");

                await PrintSummaryAsync(context, newestBranch.Id);

                return;
            }

            int requestsToCreate =
                TargetRequestCount - existingRequestCount;

            Console.WriteLine(
                $"Creating {requestsToCreate} maintenance requests...");

            // --------------------------------------------------------
            // Request templates
            // --------------------------------------------------------

            var templates = new List<MaintenanceTemplate>
            {
                new MaintenanceTemplate
                {
                    Title = "Leaking Faucet",
                    Description =
                        "The bathroom faucet is leaking continuously and needs inspection.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Broken Light",
                    Description =
                        "The room light is not working and may need a bulb replacement.",
                    Priority = "Low"
                },

                new MaintenanceTemplate
                {
                    Title = "Electrical Outlet Problem",
                    Description =
                        "One electrical outlet is not functioning properly.",
                    Priority = "High"
                },

                new MaintenanceTemplate
                {
                    Title = "Water Leakage",
                    Description =
                        "There is water leaking from the plumbing area and it needs inspection.",
                    Priority = "High"
                },

                new MaintenanceTemplate
                {
                    Title = "Door Lock Problem",
                    Description =
                        "The room door lock is difficult to operate and may need replacement.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Damaged Chair",
                    Description =
                        "The chair provided inside the room is damaged and needs repair.",
                    Priority = "Low"
                },

                new MaintenanceTemplate
                {
                    Title = "Broken Bed Frame",
                    Description =
                        "The bed frame appears damaged and requires maintenance inspection.",
                    Priority = "High"
                },

                new MaintenanceTemplate
                {
                    Title = "Air Conditioner Issue",
                    Description =
                        "The air-conditioning unit is not cooling properly.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Bathroom Drain Clogged",
                    Description =
                        "The bathroom drain is clogged and water drains very slowly.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Ceiling Fan Problem",
                    Description =
                        "The ceiling fan is making unusual noise and needs inspection.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Window Lock Damaged",
                    Description =
                        "The window lock is damaged and needs to be repaired.",
                    Priority = "High"
                },

                new MaintenanceTemplate
                {
                    Title = "Low Water Pressure",
                    Description =
                        "Water pressure in the room's bathroom is unusually low.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Cabinet Door Damaged",
                    Description =
                        "The cabinet door is loose and needs adjustment or repair.",
                    Priority = "Low"
                },

                new MaintenanceTemplate
                {
                    Title = "Shower Problem",
                    Description =
                        "The shower fixture is not functioning properly.",
                    Priority = "Normal"
                },

                new MaintenanceTemplate
                {
                    Title = "Urgent Electrical Issue",
                    Description =
                        "An electrical problem has been reported and requires immediate inspection.",
                    Priority = "Urgent"
                }
            };

            // --------------------------------------------------------
            // Random generator
            // --------------------------------------------------------

            var random = new Random(20260924);

            var statuses = new[]
            {
                "Pending",
                "Pending",
                "Pending",
                "In Progress",
                "In Progress",
                "Resolved",
                "Resolved",
                "Cancelled"
            };

            // --------------------------------------------------------
            // Create requests
            // --------------------------------------------------------

            for (int i = 0; i < requestsToCreate; i++)
            {
                var tenant = tenants[random.Next(tenants.Count)];

                var template =
                    templates[random.Next(templates.Count)];

                var status =
                    statuses[random.Next(statuses.Length)];

                int daysAgo = random.Next(1, 121);

                DateTime dateReported =
                    DateTime.UtcNow.AddDays(-daysAgo);

                DateTime? dateResolved = null;

                string? resolutionNotes = null;

                // ----------------------------------------------------
                // Resolved request
                // ----------------------------------------------------

                if (status == "Resolved")
                {
                    int resolutionDays =
                        random.Next(1, Math.Max(2, daysAgo));

                    dateResolved =
                        dateReported.AddDays(resolutionDays);

                    if (dateResolved > DateTime.UtcNow)
                    {
                        dateResolved = DateTime.UtcNow;
                    }

                    resolutionNotes =
                        GetResolutionNotes(template.Title);
                }

                // ----------------------------------------------------
                // Cancelled request
                // ----------------------------------------------------

                if (status == "Cancelled")
                {
                    int cancelDays =
                        random.Next(1, Math.Max(2, daysAgo));

                    dateResolved =
                        dateReported.AddDays(cancelDays);

                    if (dateResolved > DateTime.UtcNow)
                    {
                        dateResolved = DateTime.UtcNow;
                    }

                    resolutionNotes =
                        "Request was cancelled after review.";
                }

                // ----------------------------------------------------
                // Create entity
                // ----------------------------------------------------

                var request = new MaintenanceRequest
                {
                    TenantId = tenant.Id,

                    Title = template.Title,

                    Description = template.Description,

                    Priority = template.Priority,

                    Status = status,

                    DateReported = dateReported,

                    DateResolved = dateResolved,

                    ResolutionNotes = resolutionNotes
                };

                context.MaintenanceRequests.Add(request);
            }

            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------

            await context.SaveChangesAsync();

            Console.WriteLine();
            Console.WriteLine(
                $"{requestsToCreate} maintenance requests created.");

            // --------------------------------------------------------
            // Summary
            // --------------------------------------------------------

            await PrintSummaryAsync(
                context,
                newestBranch.Id);
        }

        // ============================================================
        // RESOLUTION NOTES
        // ============================================================

        private static string GetResolutionNotes(string title)
        {
            return title switch
            {
                "Leaking Faucet" =>
                    "Faucet was repaired and the water connection was checked.",

                "Broken Light" =>
                    "Defective light bulb was replaced.",

                "Electrical Outlet Problem" =>
                    "Electrical outlet was inspected and repaired.",

                "Water Leakage" =>
                    "Leaking pipe connection was repaired and tested.",

                "Door Lock Problem" =>
                    "Door lock mechanism was adjusted and repaired.",

                "Damaged Chair" =>
                    "Damaged chair was repaired and returned to the room.",

                "Broken Bed Frame" =>
                    "Bed frame was repaired and inspected for stability.",

                "Air Conditioner Issue" =>
                    "Air-conditioning unit was inspected and serviced.",

                "Bathroom Drain Clogged" =>
                    "Bathroom drain was cleared and water flow was restored.",

                "Ceiling Fan Problem" =>
                    "Ceiling fan was inspected and repaired.",

                "Window Lock Damaged" =>
                    "Window lock was replaced and tested.",

                "Low Water Pressure" =>
                    "Water supply connection was inspected and adjusted.",

                "Cabinet Door Damaged" =>
                    "Cabinet hinges were adjusted and the door was secured.",

                "Shower Problem" =>
                    "Shower fixture was repaired and tested.",

                "Urgent Electrical Issue" =>
                    "Electrical issue was inspected and the affected connection was repaired.",

                _ =>
                    "Maintenance request was inspected and resolved."
            };
        }

        // ============================================================
        // SUMMARY
        // ============================================================

        private static async Task PrintSummaryAsync(
            TenantCRMDbContext context,
            int branchId)
        {
            var requests = await context.MaintenanceRequests
                .Where(r =>
                    context.Tenants.Any(t =>
                        t.Id == r.TenantId &&
                        t.BranchId == branchId))
                .ToListAsync();

            Console.WriteLine();
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("MAINTENANCE REQUEST SUMMARY");
            Console.WriteLine("----------------------------------------------");

            Console.WriteLine(
                $"Total Requests : {requests.Count}");

            Console.WriteLine(
                $"Pending        : {requests.Count(r => r.Status == "Pending")}");

            Console.WriteLine(
                $"In Progress    : {requests.Count(r => r.Status == "In Progress")}");

            Console.WriteLine(
                $"Resolved       : {requests.Count(r => r.Status == "Resolved")}");

            Console.WriteLine(
                $"Cancelled      : {requests.Count(r => r.Status == "Cancelled")}");

            Console.WriteLine(
                $"Low Priority   : {requests.Count(r => r.Priority == "Low")}");

            Console.WriteLine(
                $"Normal Priority: {requests.Count(r => r.Priority == "Normal")}");

            Console.WriteLine(
                $"High Priority  : {requests.Count(r => r.Priority == "High")}");

            Console.WriteLine(
                $"Urgent         : {requests.Count(r => r.Priority == "Urgent")}");

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine();
        }

        // ============================================================
        // TEMPLATE CLASS
        // ============================================================

        private class MaintenanceTemplate
        {
            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string Priority { get; set; } = "Normal";
        }
    }
}