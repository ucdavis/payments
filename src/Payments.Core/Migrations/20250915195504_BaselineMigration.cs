using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    /// <summary>
    /// Baseline migration for existing database.
    /// This migration assumes the database already exists with all tables.
    /// It serves as a starting point for future schema changes.
    /// </summary>
    public partial class BaselineMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Database already exists with all tables
            // This is a baseline migration - no operations needed
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot downgrade from baseline
            throw new NotSupportedException("Cannot downgrade from baseline migration. Database existed before EF migrations were implemented.");
        }
    }
}