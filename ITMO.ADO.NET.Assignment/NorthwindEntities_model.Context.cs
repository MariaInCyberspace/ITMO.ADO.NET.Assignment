﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ITMO.ADO.NET.Assignment
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class northwndEntities : DbContext
    {
        public northwndEntities()
            : base("name=northwndEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<Order_Details> Order_Details { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
    }
}
