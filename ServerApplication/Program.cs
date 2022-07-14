
using Framework;
using ServerApplication;

UdpFramework udpFramework = new UdpFrameworkBuilder()
    .Build();

udpFramework.AddController(new ItemsController(new ItemsService(udpFramework.UdpFrameworkClient)));
udpFramework.Run();
