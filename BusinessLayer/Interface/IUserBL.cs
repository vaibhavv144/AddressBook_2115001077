using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.DTO;
using ModelLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        UserEntity RegistrationBL(RegisterDTO registerDTO);
        (UserEntity user, string token) LoginnUserBL(LoginDTO loginDTO);

        public bool ValidateEmail(string email);

        public bool UpdateUserPassword(string email, string newPassword);

        public UserEntity GetByEmail(string email);
    }
}
