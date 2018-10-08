using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public class NumericUserCodeService : IUserCodeGenerator
    {
        /// <summary>
        /// Gets the type of the user code.
        /// </summary>
        /// <value>
        /// The type of the user code.
        /// </value>
        public string UserCodeType => IdentityServerConstants.UserCodeTypes.Numeric;

        /// <summary>
        /// Generates the user code.
        /// </summary>
        /// <returns></returns>
        public Task<string> GenerateAsync()
        {
            var next = Next(100000000, 999999999);
            return Task.FromResult(next.ToString());
        }

        private int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue) return minValue;
            long diff = maxValue - minValue;

            var uint32Buffer = new byte[8];

            using (var rng = new RNGCryptoServiceProvider())
            {
                while (true)
                {
                    rng.GetBytes(uint32Buffer);
                    var rand = BitConverter.ToUInt32(uint32Buffer, 0);

                    const long max = 1 + (long)uint.MaxValue;
                    var remainder = max % diff;
                    if (rand < max - remainder)
                    {
                        return (int)(minValue + rand % diff);
                    }
                }
            }
        }
    }
}