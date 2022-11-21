using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules
{
    [TestFixture]
    public class TestStep : IModuleStep
    {
        public TestStep(StepId id)
        {
            Id = id;
        }

        public StepId Id { get; }

        public void Run()
        {
            // no actions required
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new TestStep(newId);
        }

        private bool Equals(IModuleStep other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestStep)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
