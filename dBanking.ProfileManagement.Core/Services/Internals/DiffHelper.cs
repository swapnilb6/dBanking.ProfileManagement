using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace dBanking.ProfileManagement.Core.Services.Internals
{
    internal static class DiffHelper
    {
        /// <summary>
        /// Returns a comma-separated list of property names whose values differ (null-safe).
        /// </summary>
        public static string ChangedFieldsCsv<T>(T before, T after, params string[] excludeProps)
        {
            var ex = new HashSet<string>(excludeProps ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 .Where(p => p.CanRead && !ex.Contains(p.Name));

            var changed = new List<string>();
            foreach (var p in props)
            {
                var v1 = p.GetValue(before);
                var v2 = p.GetValue(after);
                if (!object.Equals(v1, v2))
                    changed.Add(p.Name);
            }
            return string.Join(",", changed);
        }
    }
}
