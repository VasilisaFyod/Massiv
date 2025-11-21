using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Massiv.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public DateOnly? ContractDate { get; set; }

    public DateOnly? OrderDate { get; set; }

    public DateOnly? ProductionStartDate { get; set; }

    public string? NumberOrder { get; set; }

    public string? Product { get; set; }

    public int? List { get; set; }

    public string? Material { get; set; }

    public bool? TableAvailable { get; set; }

    public double? SquareMeters { get; set; }

    public string? Color { get; set; }

    public string? Designer { get; set; }

    public string? Constructor { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public string? ReadyDate { get; set; }

    public DateOnly? ShipmentDate { get; set; }

    public string? ClientPhone { get; set; }

    public DateOnly? NotificationDate { get; set; }

    public bool? IsCompleted { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Facade { get; set; }

    public virtual ICollection<WorkshopOrder> WorkshopOrders { get; set; } = new List<WorkshopOrder>();

    [NotMapped]
    public bool CanEdit => !IsDeleted.Value && !IsCompleted.Value;
    public bool CanRestore => IsDeleted.Value;
    public bool CanBack => IsCompleted.Value && !IsDeleted.Value;
    public bool CanComplete => !IsCompleted.Value && !IsDeleted.Value;
    public bool CanDelete => !IsDeleted.Value;

    public string StatusText
    {
        get
        {
            if (IsCompleted == true)
                return "Выполнен";

            if (CompletionDate == null)
                return "Нет даты";

            var today = DateOnly.FromDateTime(DateTime.Today);
            var completionDate = CompletionDate.Value;
            var daysDifference = completionDate.DayNumber - today.DayNumber;

            if (daysDifference < 0)
                return $"Просрочен ({Math.Abs(daysDifference)} дн.)";
            else if (daysDifference == 0)
                return "Срочно! (сегодня)";
            else if (daysDifference <= 7)
                return $"Срочно! ({daysDifference} дн.)";
            else if (daysDifference <= 21)
                return $"Средний приоритет ({daysDifference} дн.)";
            else
                return $"В работе ({daysDifference} дн.)";
        }
    }

    public string StatusColor
    {
        get
        {
            if (IsCompleted == true)
                return "#008000";

            if (CompletionDate == null)
                return "#808080";

            var today = DateOnly.FromDateTime(DateTime.Today);
            var completionDate = CompletionDate.Value;
            var daysDifference = completionDate.DayNumber - today.DayNumber;

            if (daysDifference < 0)
                return "#ff0000"; 
            else if (daysDifference <= 7)
                return "#FF6B00";
            else if (daysDifference <= 21)
                return "#ffa500";
            else
                return "#85ccfa"; 
        }
    }

    public int StatusPriority
    {
        get
        { 
            if (CompletionDate == null) return 1;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var completionDate = CompletionDate.Value;
            var daysDifference = completionDate.DayNumber - today.DayNumber;

            if (daysDifference < 0) return 6; 
            else if (daysDifference == 0) return 5;
            else if (daysDifference <= 7) return 4;
            else if (daysDifference <= 21) return 3;
            else return 2;
        }
    }
}
