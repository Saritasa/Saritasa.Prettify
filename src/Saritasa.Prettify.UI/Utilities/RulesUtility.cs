using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Saritasa.Prettify.UI.Utilities
{
    public class RulesUtility
    {
        public const string ImportantRulesKey = "ImportantRules";

        public static IEnumerable<string> GetImportantRules()
        {
            var rulesRaw = ConfigurationManager.AppSettings[ImportantRulesKey];
            if (string.IsNullOrWhiteSpace(rulesRaw))
            {
                return Enumerable.Empty<string>();
            }

            var splittedRulesByComma = rulesRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return splittedRulesByComma;
        }
    }
}
