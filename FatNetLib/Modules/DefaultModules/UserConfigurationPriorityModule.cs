using Kolyhalov.FatNetLib.Configurations;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class UserConfigurationPriorityModule : IModule
{
    private readonly Configuration _userConfiguration;

    public UserConfigurationPriorityModule(Configuration userConfiguration)
    {
        _userConfiguration = userConfiguration;
    }

    public void Setup(ModuleContext moduleContext)
    {
        var mainConfiguration = moduleContext.DependencyContext.Get<Configuration>();
        if (_userConfiguration.Framerate != null!)
        {
            mainConfiguration.Framerate = _userConfiguration.Framerate;
        }

        if (_userConfiguration.Port != null!)
        {
            mainConfiguration.Port = _userConfiguration.Port;
        }

        if (_userConfiguration.ExchangeTimeout != null!)
        {
            mainConfiguration.ExchangeTimeout = _userConfiguration.ExchangeTimeout;
        }

        if (_userConfiguration is ServerConfiguration serverConfiguration && serverConfiguration.MaxPeers != null!)
        {
            (mainConfiguration as ServerConfiguration)!.MaxPeers = serverConfiguration.MaxPeers;
            return;
        }

        if ((_userConfiguration as ClientConfiguration)!.Address != null!)
            (mainConfiguration as ClientConfiguration)!.Address = (_userConfiguration as ClientConfiguration)!.Address;
    }
}