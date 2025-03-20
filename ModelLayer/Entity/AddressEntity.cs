using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Entity
{
    public class AddressEntity
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string FullName { get; set; } 

        [Required, EmailAddress]
        public string Email { get; set; } 

        [Required]
        public long PhoneNumber { get; set; } 

        [Required]
        public string Address { get; set; } 

        // Foreign Key for UserEntity
        [Required, EmailAddress]
        [ForeignKey("UserEmail")]
        public string UserEmail { get; set; } 

        public virtual UserEntity User { get; set; }
    }
}
