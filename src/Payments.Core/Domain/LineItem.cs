﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Domain
{
    public class LineItem
    {
        [Key]
        public int Id { get; set; } 
        
        public string Description { get; set; }

        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        public decimal Amount { get; set; }
        
        public decimal Total { get; set; }
    }
}
