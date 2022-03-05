using System;

namespace Domain.Model
{
    public class OrderCreateRequest
    {
        public int UserId { get; set; }

        public decimal Total { get; set; }
    }
}
