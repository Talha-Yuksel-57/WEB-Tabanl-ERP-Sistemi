using System.ComponentModel.DataAnnotations;

namespace ERP.Core.DTOs.Customer
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email girin")]
        public string Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email girin")]
        public string? Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
