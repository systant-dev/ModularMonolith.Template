using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UserModule.Models;

namespace UserModule;

internal static partial class UserLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "User created")]
    public static partial void UserCreated(ILogger logger, [LogProperties] User user);

    #endregion
}