﻿﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System;
using System.Collections.Generic;
using System.Configuration;
using OpinionatedCache.API;
using OpinionatedCache.Policy;

namespace OpinionatedCache.Settings
{
    public class ApplicationSettingCachePolicyRepository : ICachePolicyRepository
    {
        private static ICachePolicy s_UnconfiguredPolicy = new DefaultCachePolicy { AbsoluteSeconds = 10 }; // make sure things expire eventually if not configured at all
        private static Dictionary<string, ICachePolicyAdjust> s_Cache = new Dictionary<string, ICachePolicyAdjust>();
        private static Lazy<CachePolicySection> s_ConfigSection = new Lazy<CachePolicySection>(() => { return ConfigurationManager.GetSection("cachePolicies") as CachePolicySection; });

        public string KeySeparator { get; set; }
        public string PolicyKeySeparator { get; set; }

        public ApplicationSettingCachePolicyRepository()
        {
            KeySeparator = s_ConfigSection.Value.KeySeparator ?? "/";
            PolicyKeySeparator = s_ConfigSection.Value.PolicyKeySeparator ?? "/";
        }

        public ICachePolicy DefaultPolicy()
        {
            return ComputePolicy("*", s_UnconfiguredPolicy); // convention is that "*" is the default policy
        }

        public ICachePolicy ComputePolicy(string key, ICachePolicy defaultPolicy)
        {
            ICachePolicyAdjust adjuster;
            if (!s_Cache.TryGetValue(key, out adjuster))
            {
                adjuster = ReadAdjustment(key, defaultPolicy);
                s_Cache[key] = adjuster;
            }

            if (adjuster != null)
                return adjuster.Adjust(defaultPolicy);
            else
                return defaultPolicy.Clone();   // do nothing, it's easy
        }

        private ICachePolicyAdjust ReadAdjustment(string policyKey, ICachePolicy basePolicy)
        {
            var config = s_ConfigSection.Value;

            if (config != null)
            {
                var policies = config.Policies;

                foreach (CachePolicyConfigurationElement policySetting in policies)
                {
                    if (policySetting.Key.Equals(policyKey, System.StringComparison.OrdinalIgnoreCase))
                    {
                        var policy = basePolicy.Clone();

                        if (policySetting.AbsoluteSeconds.HasValue)
                            policy.AbsoluteSeconds = policySetting.AbsoluteSeconds.Value;

                        if (policySetting.SlidingSeconds.HasValue)
                            policy.SlidingSeconds = policySetting.SlidingSeconds.Value;

                        if (policySetting.RefillCount.HasValue)
                            policy.RefillCount = policySetting.RefillCount.Value;

                        return new ApplicationSettingPolicyAdjust { ConfiguredPolicy = policy };
                    }
                }
            }

            // if we authoritatively tried to read the entry and failed none, we cache a noop
            return UnchangedPolicyAdjust.Instance;
        }
    }

    public class ApplicationSettingPolicyAdjust : ICachePolicyAdjust
    {
        public ICachePolicy ConfiguredPolicy { get; set; }

        public ICachePolicy Adjust(ICachePolicy policy)
        {
            if (ConfiguredPolicy.AbsoluteSeconds != policy.AbsoluteSeconds
                || ConfiguredPolicy.SlidingSeconds != policy.SlidingSeconds
                || ConfiguredPolicy.RefillCount != policy.RefillCount)
            {
                policy = policy.Clone();
                policy.AbsoluteSeconds = ConfiguredPolicy.AbsoluteSeconds;
                policy.SlidingSeconds = ConfiguredPolicy.SlidingSeconds;
                policy.RefillCount = ConfiguredPolicy.RefillCount;
            }

            return policy;
        }
    }
}
