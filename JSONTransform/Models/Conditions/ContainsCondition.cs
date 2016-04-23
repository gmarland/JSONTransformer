using Newtonsoft.Json.Linq;

namespace JSONTransform.Models.Conditions
{
    public class ContainsCondition : Comparitor, ICondition
    {
        public ContainsCondition(string condition, JObject resource) : base(condition, resource)
        {
            this.Initialize("CONTAINS");
        }

        public int isValid()
        {
            string cleanedLeftSide = LeftSide.Trim().TrimStart(new char[] { '\'' }).TrimEnd(new char[] { '\'' });
            string cleanedRightSide = RightSide.Trim().TrimStart(new char[] { '\'' }).TrimEnd(new char[] { '\'' });

            if (cleanedLeftSide.Contains(cleanedRightSide)) return 1;
            else return 0;
        }
    }
}
