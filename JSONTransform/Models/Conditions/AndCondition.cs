using Newtonsoft.Json.Linq;
using System;

namespace JSONTransform.Models.Conditions
{
    public class AndCondition : ICondition
    {
        private string _condition;
        private JObject _resource;

        public AndCondition(string condition, JObject resource)
        {
            _condition = condition.Substring(1, condition.Length - 2);
            _resource = resource;
        }

        public int isValid()
        {
            string[] conditionParts = _condition.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i=0; i<conditionParts.Length; i++) conditionParts[i] = conditionParts[i].Trim();

            bool isValidAnd = true;

            foreach (string conditionPart in conditionParts)
            {
                if ((conditionPart != "1") && (conditionPart != "0"))
                {
                    isValidAnd = false;
                    break;
                }
            }

            if (isValidAnd)
            {
                foreach (string conditionPart in conditionParts)
                {
                    if (conditionPart == "0") return 0;
                }

                return 1;
            }
            else throw new Exception("Invalid && condition");
        }
    }
}
