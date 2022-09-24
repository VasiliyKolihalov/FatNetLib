using System.Collections;

namespace Kolyhalov.FatNetLib;

public class PackageSchema : IEnumerable<KeyValuePair<string, Type>>
{
    private readonly IDictionary<string, Type> _fieldTypes = new Dictionary<string, Type>();
    
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
}