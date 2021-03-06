﻿﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using OpinionatedCache.API;

namespace OpinionatedCache.Web
{
    // prevents the backfilling of a key from recursing into the cache to find the existing entry by prefixing the key string.
    internal class RefillerKeyAdapter : IBaseCacheKey
    {
        public IBaseCacheKey WrappedKey { get; private set; }

        public RefillerKeyAdapter(IBaseCacheKey wrappedKey)
        {
            WrappedKey = wrappedKey;
        }

        public string Prefix { get { return WrappedKey.Prefix; } set { } }   // don't allow altering it.
        public string SubKey { get { return WrappedKey.SubKey; } set { } }   // don't allow altering it.

        public string Key
        {
            get { return "Refiller" + WrappedKey.Key; }
        }

        public string PolicyKey
        {
            get { return WrappedKey.PolicyKey; }
        }

        public ICachePolicyRepository PolicyRepository
        {
            get { return WrappedKey.PolicyRepository; }
        }

        public ICachePolicy DefaultPolicy
        {
            get { return WrappedKey.DefaultPolicy; }
        }

        public ICachePolicy Policy
        {
            get { return WrappedKey.Policy; }
        }

        public string BuildKey()
        {
            return WrappedKey.BuildKey();
        }

        public string BuildKey(params string[] vals)
        {
            return WrappedKey.BuildKey(vals);
        }

        public string BuildPolicyKey()
        {
            return WrappedKey.BuildPolicyKey();
        }

        public string BuildPolicyKey(string[] vals)
        {
            return WrappedKey.BuildPolicyKey(vals);
        }
    }
}
