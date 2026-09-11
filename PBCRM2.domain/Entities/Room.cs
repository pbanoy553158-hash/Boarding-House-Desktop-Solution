using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Domain.Entities;

public class Room
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public Branch? Branch { get; set; }
}