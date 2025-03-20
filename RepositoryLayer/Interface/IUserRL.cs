using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.DTO;
using ModelLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        UserEntity Registration(RegisterDTO registerDTO);
        UserEntity LoginnUserRL(LoginDTO loginDTO);
        public bool ValidateEmail(string email);
        public UserEntity FindByEmail(string email);
        public bool Update(UserEntity user);
    }
}
