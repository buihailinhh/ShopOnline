//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShopOnline.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SaleOrderProduct
    {
        public int Id { get; set; }
        public Nullable<int> Sale0rder_Id { get; set; }
        public Nullable<int> Product_Id { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Quatity { get; set; }
        public string Description { get; set; }
        public string Create_by { get; set; }
        public string Update_by { get; set; }
        public Nullable<System.DateTime> Create_date { get; set; }
        public Nullable<System.DateTime> Update_date { get; set; }
        public Nullable<int> Accessory_Id { get; set; }
    }
}
