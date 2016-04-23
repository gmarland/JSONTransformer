using JSONTransform.Factories;
using JSONTransform.Models;
using JSONTransform.Models.Conditions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace JSONTransform.Utils
{
    public static class LogicalUtils
    {
        public static LogicalType? GetLogicalType(string property)
        {
            if (property.ToLower().Replace(" ", String.Empty).StartsWith("if(")) return LogicalType.IF;
            else if (property.ToLower().Replace(" ", String.Empty).StartsWith("each(")) return LogicalType.EACH;
            else return null;
        }

        public static bool ParseValidIfCondition(string condition, JObject resource)
        {
            if (condition.Trim().ToLower().StartsWith("if")) condition = condition.Trim().Substring(2).Trim();

            List<int> openBracketPosition = new List<int>();

            char[] conditionArray = condition.ToCharArray();

            for (int i = 0; i < conditionArray.Length; i++)
            {
                if (conditionArray[i] == '(') openBracketPosition.Add(i);
                else if (conditionArray[i] == ')')
                {
                    if (conditionArray.Length > 0)
                    {
                        int startIndex = openBracketPosition[(openBracketPosition.Count - 1)],
                            endIndex = (i + 1);

                        string currentCondition = condition.Substring(startIndex, (endIndex - startIndex));

                        ICondition conditionOperator = ConditionalFactory.GetCondition(currentCondition, resource);

                        if (conditionOperator != null) condition = condition.Substring(0, openBracketPosition[(openBracketPosition.Count - 1)]) + conditionOperator.isValid() + condition.Substring((i + 1));
                        else throw new Exception("Invalid condition '" + currentCondition + "'");
                        break;
                    }
                    else throw new Exception("Invalid if condition '" + condition + "'");
                }
            }

            if (condition.Length == 1)
            {
                if (condition == "1") return true;
                else return false;
            }
            else return ParseValidIfCondition(condition, resource);
        }

        public static IDictionary<string, string> ParseEachProperties(string condition)
        {
            if (condition.Trim().ToLower().StartsWith("each"))
            {
                string cleanedCondition = condition.Trim().Substring(4).Trim();
                cleanedCondition = cleanedCondition.Substring(1, (cleanedCondition.Length - 2));

                string[] eachParts = cleanedCondition.Split(new string[] { "IN" }, StringSplitOptions.None);

                if (eachParts.Length == 2)
                {
                    return new Dictionary<string, string>()
                    {
                        { "child", eachParts[0].Trim() },
                        { "property", eachParts[1].Trim() }
                    };
                }
                else throw new Exception("Invalid each condition");
            }
            else throw new Exception("Invalid each condition");
        }
    }
}
