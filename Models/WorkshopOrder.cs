using System;
using System.Collections.Generic;

namespace Massiv.Models;

public partial class WorkshopOrder
{
    public int WorkshopOrderId { get; set; }

    public int OrderId { get; set; }

    public string NumberWorkshopOrder { get; set; } = null!;

    public bool IsReady { get; set; }

    public virtual Order Order { get; set; } = null!;
}
