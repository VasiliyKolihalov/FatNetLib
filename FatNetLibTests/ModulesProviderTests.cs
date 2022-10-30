using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Modules;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib;

public class ModulesProviderTests
{
    private IModulesProvider _modulesProvider = null!;
    private ModuleContext _moduleContext = null!;

    [SetUp]
    public void SetUp()
    {
        _modulesProvider = new ModulesProvider();
        var dependencyContext = new DependencyContext();
        dependencyContext.Put("SendingMiddlewares", _ => null!);
        dependencyContext.Put("ReceivingMiddlewares", _ => null!);
        dependencyContext.Put("IEndpointRecorder", _ => null!);
        dependencyContext.Put("IEndpointsStorage", _ => null!);
        _moduleContext = new ModuleContext(dependencyContext);
    }

    [Test]
    public void Setup_SimpleModule_SetupRegistered()
    {
        // Arrange
        var module = new Mock<Module>();
        _modulesProvider.Register(module.Object);
        
        //Act
        _modulesProvider.Setup(_moduleContext);

        // Assert
        module.Verify(_ => _.Setup(_moduleContext), Times.Once);
    }

    [Test]
    public void Setup_ModuleWithChild_SetupAll()
    {
        // Arrange
        var childModule = new Mock<Module>();
        var parentModule = new ModuleWithChild(childModule.Object);
        _modulesProvider.Register(parentModule);
        
        //Act
        _modulesProvider.Setup(_moduleContext);

        // Assert
        childModule.Verify(_ => _.Setup(_moduleContext), Times.Once);
    }

    [Test]
    public void Setup_ListOfModules_SetupAll()
    {
        // Arrange
        var module1 = new Mock<Module>();
        var module2 = new Mock<Module>();
        _modulesProvider.Register(new List<Module> { module1.Object, module2.Object });
        
        //Act
        _modulesProvider.Setup(_moduleContext);
        
        // Assert
        module1.Verify(_ => _.Setup(_moduleContext), Times.Once);
        module2.Verify(_ => _.Setup(_moduleContext), Times.Once);
    }

    [Test]
    public void Setup_IgnoredModule_Ignore()
    {
        // Arrange
        var module = new ThrowSetupModule();
        _modulesProvider.Register(module);
        _modulesProvider.Ignore<ThrowSetupModule>();
        
        //Act
        Action action = () => _modulesProvider.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
    }

    [Test]
    public void Setup_ModuleWithIgnoredChild_SetupParentAndIgnoreChild()
    {
        // Arrange
        var module = new ModuleWithChild(new ThrowSetupModule());
        _modulesProvider.Register(module);
        _modulesProvider.Ignore<ThrowSetupModule>();
        
        //Act
        Action action = () => _modulesProvider.Setup(_moduleContext);
        
        // Assert
        action.Should().NotThrow();
    }

    [Test]
    public void Setup_ReplacedModule_Setup()
    {
        // Arrange
        var module = new ThrowSetupModule();
        var replaceModule = new Mock<Module>();
        _modulesProvider.Register(module);
        _modulesProvider.Replace<ThrowSetupModule>(replaceModule.Object);
        
        //Act
        Action action = () => _modulesProvider.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
        replaceModule.Verify(_ => _.Setup(_moduleContext), Times.Once);
    }

    [Test]
    public void Setup_ModuleWithReplacedChild_SetupParentAndReplacedChild()
    {
        // Arrange
        var module = new ModuleWithChild(new ThrowSetupModule());
        var replaceModule = new Mock<Module>();
        _modulesProvider.Register(module);
        _modulesProvider.Replace<ThrowSetupModule>(replaceModule.Object);
        
        //Act
        Action action = () => _modulesProvider.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
        replaceModule.Verify(_ => _.Setup(_moduleContext), Times.Once);
    }


    private class ModuleWithChild : Module
    {
        private readonly Module _childModule;

        public ModuleWithChild(Module childModule)
        {
            _childModule = childModule;
        }

        public override void Setup(ModuleContext moduleContext)
        {
            ChildModules.Add(_childModule);
        }
    }

    private class ThrowSetupModule : Module
    {
        public override void Setup(ModuleContext moduleContext)
        {
            throw new NotImplementedException();
        }
    }
}