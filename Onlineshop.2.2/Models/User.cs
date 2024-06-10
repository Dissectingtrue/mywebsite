using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Userss.Models
{
    public class Person
    {
        

        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public Person(string email, string password, Role role)
        {
            Email = email;
            Password = password;
            Role = role;
        }
    }
    public class Role
    {
        public string Name { get; set; }
        public Role(string name) => Name = name;
    }
    

}