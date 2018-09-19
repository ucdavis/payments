using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Payments.Core.Domain
{
    public class User : IdentityUser
    {

        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public override string Email { get; set; }

        [StringLength(50)] // cache for campus kerb, also providerKey for the UCD login provider
        [Display(Name = "Campus Kerberos")]
        public string CampusKerberos { get; set; }

        public virtual ICollection<TeamPermission> TeamPermissions { get; set; }

        private string _emailHash;

        [NotMapped]
        public string EmailHash
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_emailHash))
                {
                    return _emailHash;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    return "";
                }

                // Create a new instance of the MD5CryptoServiceProvider object.
                var md5Hasher = MD5.Create();

                // Convert the input string to a byte array and compute the hash.
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(Email.ToLower().Trim()));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (var i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                _emailHash = sBuilder.ToString(); // Return the hexadecimal string. 
                return _emailHash;
            }
        }

        public IEnumerable<Team> GetTeams()
        {
            return TeamPermissions.Select(p => p.Team).Distinct();
        }

        public bool IsTeamAdmin(string slug)
        {
            return TeamPermissions.Any(a => a.Team.Slug == slug && a.Role.Name == TeamRole.Codes.Admin);
        }
    }
}
