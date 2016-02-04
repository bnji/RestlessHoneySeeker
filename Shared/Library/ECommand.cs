using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public enum ECommand
    {
        //GET
        GetDesktop,
        GetWebcam,
        GetPorts,
        GetChromeData,
        GetClipboardData,
        GetShares,
        GetLanComputers,
        GetGateways,
        GetFile,
        GetFileEvents,
        GetProcesses,
        //RUN
        Speak,
        StreamDesktop,
        StopStreamDesktop,
        RunCommand, // command e.g. cmd.exe /c DIR
        Portscan,
        //MOVE_CURSOR,
        SetTransmissionInterval,
        StartPlugin,
        KillPlugin,
        UploadPlugin,
        KillProcess,
        RunCode,
        //SYS
        SysShutdown,
        SysRestart,
        SysLogoff,
        SysLock,
        SysHibernate,
        SysSleep,
        //MISC
        UploadFile,
        DoNothing
    }
}
