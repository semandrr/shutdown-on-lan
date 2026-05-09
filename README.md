# Shutdown-on-LAN

Dead simple C# console app that allows shutting down by [Wake-on-LAN (WoL)](https://en.wikipedia.org/wiki/Wake-on-LAN) magic packet.

Like an author of [that article](https://habr.com/ru/articles/816765/) (in Russian), I also wondered if it is possible to do remote shutdowns without using any other applications (as for me, I used `sudo shutdown now` via SSH), so I decided to write my own implementation in C# that does the same thing via WoL.

## Build

1. Install [.NET 10](https://dotnet.microsoft.com/en-us/).

2. Edit `Config.cs` (see "Known issues" in the end).

3. Run `dotnet build`.

4. The compiled application should be in `bin` folder. On Linux, you must run it with root privileges, as it is not possible to bind to ports less than <1024 without them and even shutting down the system.

## Background launch

It is (technically) possible to launch this app in background. On Linux, you can create systemd service:

```
[Unit]
Description=Shutdown-on-LAN
Requires=network.target
After=network.target

[Service]
User=root
WorkingDirectory=/root/shutdown-on-lan
ExecStart=/root/shutdown-on-lan/shutdown-on-lan/bin/Debug/net10.0/shutdown-on-lan
Restart=on-failure

[Install]
WantedBy=multi-user.target
```

On Windows, it should be possible by creating a service via [NSSM](https://nssm.cc/). Please do your own research, I rarely use Windows nowadays and therefore I will not provide any support for it.

## Known issues

- Currently, the only way to configure the application is to edit variables in `Config.cs` classfile and rebuild, which is a time-consuming and not user-friendly process. In the future, I may add configuration via file/command-line arguments.

- MacOS and FreeBSD are not supported, I never used these operating systems before. If you really need to run this application on them, make necessary changes by yourself and submit a pull request.

## License

[MIT](https://github.com/semandrr/shutdown-on-lan/blob/main/LICENSE)
