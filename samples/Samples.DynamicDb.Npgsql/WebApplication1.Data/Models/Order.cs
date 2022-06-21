using System;

namespace WebApplication1.Data.Models
{
    public class Order
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

    }
}
