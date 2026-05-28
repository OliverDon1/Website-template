using ServeyApplication.Models;

public class UserProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
}