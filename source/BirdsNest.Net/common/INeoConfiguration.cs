using System;

namespace common
{
    public interface INeoConfiguration: IDisposable
    {
        string DB_URI { get; }
        string DB_Username { get; }
        string DB_Password { get;  }
    }
}
