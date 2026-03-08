using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotificationWorker.Data.Dtos.Requests;

namespace NotificationWorker.Service.Implementation
{
    public static class EmailTemplateBuilder
    {
        public static EmailMessage Build(NotificationEvent evt) => evt.Module switch
        {
            "OrderPlaced" => BuildOrderPlacedEmail(evt),
            "OrderShipped" => BuildOrderShippedEmail(evt),
            "OrderCanceled" => BuildOrderCanceledEmail(evt),
            _ => BuildDefaultEmail(evt)
        };

        private static EmailMessage BuildOrderPlacedEmail(NotificationEvent evt)
        {
            var json = JsonConvert.SerializeObject(evt.Payload);
            var orderDetails = JObject.Parse(json);
            var customerId = orderDetails["customerId"]?.ToString();
            return new EmailMessage
            {
                To = evt.CustomerEmail,
                Subject = "Your order has been placed!",
                Body = $"Dear {customerId}, thank you for your order. We are processing it and will update you once it's shipped."
            };
        }

        private static EmailMessage BuildOrderShippedEmail(NotificationEvent evt)
        {
            var json = JsonConvert.SerializeObject(evt.Payload);
            var orderDetails = JObject.Parse(json);
            var customerId = orderDetails["customerId"]?.ToString();
            var trackingNumber = orderDetails["trackingNumber"]?.ToString();
            return new EmailMessage
            {
                To = evt.CustomerEmail,
                Subject = "Your order has been shipped!",
                Body = $"Dear {customerId}, good news! Your order has been shipped. You can expect delivery soon. Your tracking number is {trackingNumber}."
            };
        }

        private static EmailMessage BuildOrderCanceledEmail(NotificationEvent evt)
        {
            var json = JsonConvert.SerializeObject(evt.Payload);
            var orderDetails = JObject.Parse(json);
            var customerId = orderDetails["customerId"]?.ToString();
            return new EmailMessage
            {
                To = evt.CustomerEmail,
                Subject = "Your order has been canceled",
                Body = $"Dear {customerId}, we regret to inform you that your order has been canceled. Please contact support for more information."
            };
        }

        private static EmailMessage BuildDefaultEmail(NotificationEvent evt)
        {
            return new EmailMessage
            {
                To = evt.CustomerEmail,
                Subject = $"Notification: {evt.EventType}",
                Body = $"Dear Customer, you have a new notification regarding {evt.EventType}. Please check your account for details."
            };
        }
    }
}