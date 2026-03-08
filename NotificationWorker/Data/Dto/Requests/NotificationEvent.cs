namespace NotificationWorker.Data.Dtos.Requests
{
    public class NotificationEvent
    {
        public string Module        { get; set; } = default!;
        public string EventType     { get; set; } = default!;
        public string CustomerEmail { get; set; } = default!;
        public object Payload { get; set; } = new();
        public DateTime OccurredAt  { get; set; } = DateTime.UtcNow;
    }
}