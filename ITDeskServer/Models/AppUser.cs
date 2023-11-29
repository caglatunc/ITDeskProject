﻿using Microsoft.AspNetCore.Identity;

namespace ITDeskServer.Models;

public class AppUser: IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int WrongTryCount { get; set; } = 0;
    public DateTime LockDate { get; set; } = DateTime.Now;
    public DateTime LastWrongTry { get; set; } = DateTime.Now;
}
