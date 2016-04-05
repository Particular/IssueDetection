using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameHelpers
{
    public class CollectionNamer
    {
        public static string NameByDefaultRavenConvention(string candidate)
        {
            return Raven.Client.Util.Inflector.Pluralize(candidate);
        }

        public static string NameByNSBConvention(string candidate)
        {
            return candidate.Replace("Data", string.Empty);
        }
    }
}
