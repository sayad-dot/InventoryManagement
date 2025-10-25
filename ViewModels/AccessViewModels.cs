using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.ViewModels
{
    public class AccessControlViewModel
    {
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        
        [Display(Name = "Make inventory public")]
        public bool IsPublic { get; set; }
        
        public List<UserAccessViewModel> GrantedUsers { get; set; } = new List<UserAccessViewModel>();
        
        [Display(Name = "Add user by email")]
        public string NewUserEmail { get; set; } = string.Empty;
    }

    public class UserAccessViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
    }

    public class UserSearchResultViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}