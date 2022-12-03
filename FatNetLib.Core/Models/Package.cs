using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class Package
    {
        public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public IDictionary<string, object> NonSendingFields { get; set; } = new Dictionary<string, object>();

        public Route? Route
        {
            get => GetField<Route>(nameof(Route));
            set => SetField(nameof(Route), value);
        }

        public object? Body
        {
            get => GetField<object?>(nameof(Body));
            set => SetField(nameof(Body), value);
        }

        public T GetBodyAs<T>()
        {
            return (T)Body!;
        }

        public Guid ExchangeId
        {
            get => GetField<Guid>(nameof(ExchangeId));
            set => SetField(nameof(ExchangeId), value);
        }

        public bool IsResponse
        {
            get => GetField<bool>(nameof(IsResponse));
            set => SetField(nameof(IsResponse), value);
        }

        public byte[]? Serialized
        {
            get => GetNonSendingField<byte[]>(nameof(Serialized));
            set => SetNonSendingField(nameof(Serialized), value);
        }

        public PackageSchema? Schema
        {
            get => GetNonSendingField<PackageSchema>(nameof(Schema));
            set => SetNonSendingField(nameof(Schema), value);
        }

        public IDependencyContext? Context
        {
            get => GetNonSendingField<IDependencyContext>(nameof(Context));
            set => SetNonSendingField(nameof(Context), value);
        }

        public ICourier? Courier => Context?.Get<ICourier>();

        public ServerCourier? ServerCourier => Courier as ServerCourier;

        public ClientCourier? ClientCourier => Courier as ClientCourier;

        public INetPeer? FromPeer
        {
            get => GetNonSendingField<INetPeer?>(nameof(FromPeer));
            set => SetNonSendingField(nameof(FromPeer), value);
        }

        public INetPeer? ToPeer
        {
            get => GetNonSendingField<INetPeer?>(nameof(ToPeer));
            set => SetNonSendingField(nameof(ToPeer), value);
        }

        public Reliability? Reliability
        {
            get => GetNonSendingField<Reliability?>(nameof(Reliability));
            set => SetNonSendingField(nameof(Reliability), value);
        }

        public object? this[string key]
        {
            get => GetField<object>(key);
            set => SetField(key, value);
        }

        public T GetField<T>(string key)
        {
            return Fields.ContainsKey(key)
                ? (T)Fields[key]
                : throw new FatNetLibException($"Field {key} was not present in the package");
        }

        public void SetField<T>(string key, T value)
        {
            Fields[key] = value!;
        }

        public void RemoveField(string key)
        {
            Fields.Remove(key);
        }

        public T GetNonSendingField<T>(string key)
        {
            return NonSendingFields.ContainsKey(key)
                ? (T)NonSendingFields[key]
                : throw new FatNetLibException($"Non-sending field {key} was not present in the package");
        }

        public void SetNonSendingField<T>(string key, T value)
        {
            NonSendingFields[key] = value!;
        }

        public void RemoveNonSendingField(string key)
        {
            NonSendingFields.Remove(key);
        }
    }
}
