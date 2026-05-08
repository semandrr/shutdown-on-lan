using System;
namespace shutdown_on_lan;
public class Config
{
    public static readonly int wolPort = 9; // usually 9 or 7
    public static readonly string macAddr = "edit_me"; // only WOL-packets for that MAC-address will trigger shutdown, all others will be ignored
    public static readonly int instantShutdown = 1; // trigger instant shutdown, set this to 0 if you want to avoid possible accidents
    public static readonly int silent = 0; // hide messages about ignoring WoL-packets sent to different MAC-address
}
