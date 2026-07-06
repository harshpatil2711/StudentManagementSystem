using System.ComponentModel.DataAnnotations;

namespace BusinessLayer1.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last name can only contain letters and spaces")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [RegularExpression(@"^\+?[0-9\-\(\)\s]{7,15}$", ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }
    }
}
