using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Api.Data;

public class ApiKey
{
    public int Id { get; set; }
    [MaxLength(450)]
    public string Key { get; set; } = string.Empty;
    [MaxLength(200)]
    public string AppName { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    [NotMapped]
    public bool IsActive => !ExpiresAtUtc.HasValue || ExpiresAtUtc > DateTimeOffset.UtcNow;
}
