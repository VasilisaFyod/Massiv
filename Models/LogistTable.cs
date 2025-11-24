using System;
using System.Collections.Generic;

namespace Massiv.Models;

public partial class LogistTable
{
    public int LogistTableId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string TableType { get; set; } = null!;

    public DateOnly? PlanDate { get; set; }

    public string? Ldsp { get; set; }

    public string? Kr1 { get; set; }

    public string? Furniture { get; set; }

    public string? Hands { get; set; }

    public string? Anchor { get; set; }

    public string? Table { get; set; }

    public string? Kr3D { get; set; }

    public string? KrTable { get; set; }

    public string? Side { get; set; }

    public string? Base { get; set; }

    public string? Glass { get; set; }

    public string? RangeHood { get; set; }

    public string? Wash { get; set; }

    public string? Panel { get; set; }

    public string? ShipmentDate { get; set; }

    public bool? IsCompleted { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public string? ColorMark { get; set; }
}
