using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiSteamFarm.IPC;

public class IPCConfig
{
    public IPCConfig(Uri uri) { Kestrel = new(uri); }

    public Kestrel Kestrel { get; private set; }
}

public class Kestrel
{
    public Kestrel(Uri uri) { Endpoints = new(uri); }

    public Endpoints Endpoints { get; private set; }
}

public class Endpoints
{
    public Endpoints(Uri uri) { Http = new(uri); }

    public Http Http { get; private set; }
}

public class Http
{
    public Http(Uri uri) { Url = uri; }

    public Uri Url { get; private set; }
}

