using System;
using DShop.Common.Messages;
using Newtonsoft.Json;

namespace DShop.Services.Discounts.Messages.Commands
{
    // Immutable
    // Custom routing key: #.discounts.create_discount
    public class CreateDiscount : ICommand
    {
        //NOTE: all are Read-Only properties/Immutable properties. Because dont need to change incoming cmd
        //Also all the msgs posted to the SvcBus must be immutable as well
        public Guid Id { get; }
        public Guid CustomerId { get; }
        public string Code { get; }
        public double Percentage { get; }

        //To deserialize incoming messages
        [JsonConstructor]
        public CreateDiscount(Guid id, Guid customerId,
            string code, double percentage)
        {
            Id = id;
            CustomerId = customerId;
            Code = code;
            Percentage = percentage;
        }
    }
}