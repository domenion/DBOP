
using DBOP.Constants;

namespace DBOP.Models;

public class ConnectionOptions
{
    public DbSource Source { get; set; }
    public string Database { get; set; } = null!;
    public string DataSource { get; set; } = null!;
    public int? Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool? Pooling { get; set; }
    public int? ConnectionLifetime { get; set; }
    public string CharSet { get; set; } = null!;
    public bool? AllowZeroDateTime { get; set; }
    public bool? ConvertZeroDateTime { get; set; }
}
