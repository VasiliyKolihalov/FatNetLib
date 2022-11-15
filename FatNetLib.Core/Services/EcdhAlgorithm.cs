using System.Runtime.CompilerServices;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Kolyhalov.FatNetLib.Core.Services
{
    // ECDH - Elliptic Curve Diffie-Hellman algorithm
    public class EcdhAlgorithm
    {
        private const string AlgorithmName = "ECDH";
        private const string CurveName = "P-521";
        private readonly AsymmetricCipherKeyPair _myKeyPair = GenerateKeyPair();
        private bool _isAlreadyUsed;

        public byte[] MyPublicKey => SerializePublicKey(_myKeyPair.Public);

        [MethodImpl(Synchronized)]
        public byte[] CalculateSharedSecret(byte[] notMyPublicKey)
        {
            if (_isAlreadyUsed)
                throw new FatNetLibException("This class was not designed for reusing");

            _isAlreadyUsed = true;
            ICipherParameters privateKey = _myKeyPair.Private;
            ICipherParameters publicKey = DeserializePublicKey(notMyPublicKey);
            byte[] agreement = CalculateAgreement(privateKey, publicKey);
            return CalculateHash256(agreement);
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            DerObjectIdentifier curveId = NistNamedCurves.GetOid(CurveName);
            IAsymmetricCipherKeyPairGenerator generator = GeneratorUtilities.GetKeyPairGenerator(AlgorithmName)!;
            generator.Init(new ECKeyGenerationParameters(curveId, new SecureRandom()));
            return generator.GenerateKeyPair();
        }

        private static byte[] CalculateAgreement(ICipherParameters privateKey, ICipherParameters publicKey)
        {
            IBasicAgreement agreement = AgreementUtilities.GetBasicAgreement(AlgorithmName);
            agreement.Init(privateKey);
            return agreement.CalculateAgreement(publicKey)
                .ToByteArray();
        }

        private static byte[] CalculateHash256(byte[] target)
        {
            IDigest digest = new Sha256Digest();
            var hash256 = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(target, inOff: 0, target.Length);
            digest.DoFinal(hash256, outOff: 0);
            return hash256;
        }

        private static byte[] SerializePublicKey(AsymmetricKeyParameter publicKey)
        {
            return ((ECPublicKeyParameters)publicKey).Q.GetEncoded();
        }

        private static ECPublicKeyParameters DeserializePublicKey(byte[] serializedKey)
        {
            ECPoint ecPoint = ECNamedCurveTable.GetByName(CurveName)
                .Curve
                .DecodePoint(serializedKey);
            return new ECPublicKeyParameters(AlgorithmName, ecPoint, NistNamedCurves.GetOid(CurveName));
        }
    }
}
