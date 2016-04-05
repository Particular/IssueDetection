using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameHelpers
{
    public static class CollectionNameChecker
    {
        public static bool CheckForMatch(string candidate, List<string> collections, out string match)
        {
            if (candidate.EndsWith("Datas"))
            {
                var candidateWithoutEndingDatasAndAnyOtherData = candidate.Substring(0, candidate.LastIndexOf("Datas", StringComparison.InvariantCulture)).Replace("Data", string.Empty);

                if (collections.Contains(candidateWithoutEndingDatasAndAnyOtherData))
                {
                    match = candidateWithoutEndingDatasAndAnyOtherData;
                    return true;
                }

                match = null;
                return false;
            }

            var candidateWithoutData = candidate.Replace("Data", string.Empty);
            foreach (var collection in collections)
            {
                if (collection == candidate)
                {
                    continue;
                }

                var pluralizedCollection = Raven.Client.Util.Inflector.Pluralize(collection);
                if (candidateWithoutData == pluralizedCollection)
                {
                    match = collection;
                    return true;
                }
            }

            match = null;
            return false;
        }
    }


}
