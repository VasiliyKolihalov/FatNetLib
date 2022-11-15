using System;
using System.Collections;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Core.Configurations
{
    public class PackageSchema : IEnumerable<KeyValuePair<string, Type>>
    {
        private readonly IDictionary<string, Type> _fieldTypes = new Dictionary<string, Type>();

        public PackageSchema()
        {
        }

        public PackageSchema(PackageSchema otherSchema)
        {
            _fieldTypes = new Dictionary<string, Type>(otherSchema._fieldTypes);
        }

        public Type this[string key]
        {
            get => _fieldTypes[key];
            set => _fieldTypes[key] = value;
        }

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
        {
            return _fieldTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fieldTypes.GetEnumerator();
        }

        public void Add(string key, Type type)
        {
            _fieldTypes[key] = type;
        }

        public bool ContainsKey(string key) => _fieldTypes.ContainsKey(key);

        public void Patch(PackageSchema patch)
        {
            foreach (KeyValuePair<string, Type> newType in patch)
            {
                this[newType.Key] = newType.Value;
            }
        }
    }
}
