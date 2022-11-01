using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Modules;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib;

public class ModulesRecorderTests
{
    private IModulesRecorder _modulesRecorder = null!;
    private ModuleContext _moduleContext = null!;
    private IDependencyContext _dependencyContext = null!;

    [SetUp]
    public void SetUp()
    {
        _dependencyContext = new DependencyContext();
        _dependencyContext.Put("SendingMiddlewares", _ => null!);
        _dependencyContext.Put("ReceivingMiddlewares", _ => null!);
        _dependencyContext.Put("IEndpointRecorder", _ => null!);
        _dependencyContext.Put("IEndpointsStorage", _ => null!);
        _moduleContext = new ModuleContext(_dependencyContext);
        _modulesRecorder = new ModulesRecorder();
    }

    [Test]
    public void Setup_SimpleModule_SetupRegistered()
    {
        // Arrange
        var module = new Mock<IModule>();
        _modulesRecorder.Register(module.Object);

        // Act
        _modulesRecorder.Setup(_moduleContext);

        // Assert
        module.Verify(_ => _.Setup(_moduleContext), Once);
    }

    [Test]
    public void Setup_ModuleWithChild_SetupAllInTheRightOrder()
    {
        // Arrange
        var firstModule = new Mock<IModule>();
        var secondModule =
            new DependencyModule(new KeyValuePair<string, object>("id", "oldDependency"), firstModule.Object);
        var thirdModule = new DependencyModule(new KeyValuePair<string, object>("id", "newDependency"), secondModule);
        _modulesRecorder.Register(thirdModule);

        // Act
        _modulesRecorder.Setup(_moduleContext);

        // Assert
        firstModule.Verify(_ => _.Setup(_moduleContext), Once);
        _dependencyContext.Get<string>("id").Should().Be("newDependency");
    }

    [Test]
    public void Setup_ListOfModules_SetupAll()
    {
        // Arrange
        var module1 = new Mock<IModule>();
        var module2 = new Mock<IModule>();
        _modulesRecorder.Register(new List<IModule> { module1.Object, module2.Object });

        // Act
        _modulesRecorder.Setup(_moduleContext);

        // Assert
        module1.Verify(_ => _.Setup(_moduleContext), Once);
        module2.Verify(_ => _.Setup(_moduleContext), Once);
    }

    [Test]
    public void Setup_IgnoredModule_Ignore()
    {
        // Arrange
        var module = new ThrowingModule();
        _modulesRecorder.Register(module);
        _modulesRecorder.Ignore<ThrowingModule>();

        // Act
        Action action = () => _modulesRecorder.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
    }

    [Test]
    public void Setup_ModuleWithIgnoredChild_SetupParentAndIgnoreChild()
    {
        // Arrange
        var module = new ModuleWithChild(new ThrowingModule());
        _modulesRecorder.Register(module);
        _modulesRecorder.Ignore<ThrowingModule>();

        // Act
        Action action = () => _modulesRecorder.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
    }

    [Test]
    public void Setup_ReplacedModule_Setup()
    {
        // Arrange
        var module = new ThrowingModule();
        var replaceModule = new Mock<IModule>();
        _modulesRecorder.Register(module);
        _modulesRecorder.Replace<ThrowingModule>(replaceModule.Object);

        // Act
        Action action = () => _modulesRecorder.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
        replaceModule.Verify(_ => _.Setup(_moduleContext), Once);
    }

    [Test]
    public void Setup_ModuleWithReplacedChild_SetupParentAndReplacedChild()
    {
        // Arrange
        var module = new ModuleWithChild(new ThrowingModule());
        var replaceModule = new Mock<IModule>();
        _modulesRecorder.Register(module);
        _modulesRecorder.Replace<ThrowingModule>(replaceModule.Object);

        // Act
        Action action = () => _modulesRecorder.Setup(_moduleContext);

        // Assert
        action.Should().NotThrow();
        replaceModule.Verify(_ => _.Setup(_moduleContext), Once);
    }

    [Test]
    public void Setup_ReplacedModuleWithChild_SetupReplacedParentAndChild()
    {
        // Arrange
        var moduleToReplace = new ThrowingModule();
        var childModule = new Mock<IModule>();
        var parentModule = new ModuleWithChild(childModule.Object);
        _modulesRecorder.Register(moduleToReplace);
        _modulesRecorder.Replace<ThrowingModule>(parentModule);

        // Act
        _modulesRecorder.Setup(_moduleContext);

        // Assert
        childModule.Verify(_ => _.Setup(_moduleContext), Once);
    }

    private class ModuleWithChild : IModule
    {
        public ModuleWithChild(IModule childModule)
        {
            ChildModules = new List<IModule> { childModule };
        }

        public void Setup(ModuleContext moduleContext)
        {
        }

        public IList<IModule> ChildModules { get; }
    }

    private class ThrowingModule : IModule
    {
        public void Setup(ModuleContext moduleContext)
        {
            throw new NotImplementedException();
        }

        public IList<IModule> ChildModules => null!;
    }

    private class DependencyModule : IModule
    {
        private readonly KeyValuePair<string, object> _dependency;

        public DependencyModule(KeyValuePair<string, object> dependency, IModule? childModule = null)
        {
            if (childModule is not null)
                ChildModules.Add(childModule);

            _dependency = dependency;
        }

        public void Setup(ModuleContext moduleContext)
        {
            moduleContext.DependencyContext.Put(_dependency.Key, _ => _dependency.Value);
        }

        public IList<IModule> ChildModules { get; } = new List<IModule>();
    }
}