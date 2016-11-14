using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Payments.Core;

namespace Payments.Core.Migrations
{
    [DbContext(typeof(PaymentsContext))]
    [Migration("20161110190305_Invoice_TotalAmount")]
    partial class Invoice_TotalAmount
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Payments.Core.Models.Invoice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("TotalAmount");

                    b.HasKey("Id");

                    b.ToTable("Invoices");
                });
        }
    }
}
