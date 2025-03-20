using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Model
{
    public class AddressRequestModel
    {
      
   
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
 
        public long PhoneNumber { get; set; }
 
        public string Address { get; set; }

        //[EmailAddress]
        //public string UEmail { get; set; }



    }
}
