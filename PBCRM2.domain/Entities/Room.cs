using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Domain.Entities;

public class Room
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomType { get; set; } = "Standard";

    public int Capacity { get; set; }

    public string Status { get; set; } = "Available";

    public bool IsActive { get; set; } = true;

    public Branch? Branch { get; set; }

    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
