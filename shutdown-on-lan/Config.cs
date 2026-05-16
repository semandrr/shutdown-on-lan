using System;
namespace shutdown_on_lan;
public class Config
{
    public static readonly int wolPort = 9; // usually 9 or 7
    public static readonly string macAddr = "edit_me"; // only WoL packets for that MAC-address will trigger shutdown, all others will be ignored
    public static readonly int instantShutdown = 1; // trigger instant shutdown, set this to 0 if you want to avoid possible accidents
    public static readonly int silent = 0; // hide messages about ignoring WoL packets sent to different MAC-address
    public static readonly string shutdownMessage = "Received WoL packet. The system will shutdown after 1 minute."; // if instantShutdown is not enabled, this message will be displayed to users before shutting down the system
    public static readonly int noMessage = 0; // if set to 1, shutdown message will not be displayed
    public static readonly int showInvalid = 0; // if set to 1, messages about invalid UDP packets will be displayed
}
