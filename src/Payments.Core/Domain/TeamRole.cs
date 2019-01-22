using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Domain
{
    public class TeamRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role Name")]
        public string Name { get; set; }

        public class Codes
        {
            /// <summary>
            /// Reports only user
            /// </summary>
            public const string ReportUser = "ReportUser";

            /// <summary>
            /// Can do everything except manage users/roles
            /// </summary>
            public const string Editor = "Editor";

            /// <summary>
            /// Financial Officer, can process payments or write off invoices
            /// </summary>
            public const string FinanceOfficer = "FinanceOfficer";

            /// <summary>
            /// God like powers limited to the Team
            /// </summary>
            public const string Admin = "Admin"; 
        }
    }
}
