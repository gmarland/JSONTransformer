using JSONTransform.Models.Conditions;
using Newtonsoft.Json.Linq;

namespace JSONTransform.Factories
{
    public static class ConditionalFactory
    {
        public static ICondition GetCondition(string condition, JObject resource)
        {
            if (condition.Contains("==")) return new EqualsCondition(condition, resource);
            else if (condition.Contains("!=")) return new NotEqualsCondition(condition, resource);
            else if (condition.Contains("CONTAINS")) return new ContainsCondition(condition, resource);
            if (condition.Contains("||")) return new OrCondition(condition, resource);
            else if (condition.Contains("&&")) return new AndCondition(condition, resource);
            else return null;
        }
    }
}
