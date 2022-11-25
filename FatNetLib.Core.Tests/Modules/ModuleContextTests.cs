using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules
{
    public class ModuleContextTests
    {
        private DependencyContext _dependencyContext = null!;
        private ModuleContext _moduleContext = null!;

        [SetUp]
        public void SetUp()
        {
            _dependencyContext = new DependencyContext();
            _moduleContext = new ModuleContext(new TestModule(), _dependencyContext);
        }

        [Test]
        public void Build_CorrectCase_PutDependency()
        {
            // Act
            _moduleContext.Build();

            // Assert
            _dependencyContext.Get<string>().Should().Be("dependency-1");
            _dependencyContext.Get<string>("dependency-2-key").Should().Be("dependency-2");
        }

        [Test]
        public void Build_CorrectCase_PutModule()
        {
            // Act
            _moduleContext.Build();

            // Assert
            _dependencyContext.Get<string>("child-module-dependency-key").Should().Be("child-module-dependency");
        }

        [Test]
        public void Build_CorrectCase_PutScript()
        {
            // Act
            _moduleContext.Build();

            // Assert
            _dependencyContext.Get<string>("script-dependency-key").Should().Be("script-dependency");
        }

        private class TestModule : IModule
        {
            public void Setup(IModuleContext moduleContext)
            {
                moduleContext
                    .PutDependency(_ => "dependency-1")
                    .PutDependency("dependency-2-key", _ => "dependency-2")
                    .PutModule(new ChildModule())
                    .PutScript("AddScriptDependency", _ => _.Put("script-dependency-key", "script-dependency"));
            }
        }

        private class ChildModule : IModule
        {
            public void Setup(IModuleContext moduleContext)
            {
                moduleContext.PutDependency("child-module-dependency-key", _ => "child-module-dependency");
            }
        }
    }
}
