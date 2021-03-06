using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CardShop.Models
{
    [MetadataType(typeof(StoreMetaData))]
    public partial class Store
    {

    }

    public class StoreMetaData
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name="Discount Rate")]
        public Double DiscountRate { get; set; }
    
        public virtual User UserProfile { get; set; }
        public virtual ICollection<StoreInventory> StoreInventories { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
