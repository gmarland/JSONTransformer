using JSONTransform.Utils;
using Newtonsoft.Json.Linq;
using System;

namespace JSONTransform.Models.Conditions
{
    public class Comparitor
    {
        private string _condition;
        private JObject _resource;

        private string _leftSide;
        private string _rightSide;

        private int? _leftSideInt = null;
        private int? _rightSideInt = null;

        public Comparitor(string condition, JObject resource)
        {
            _condition = condition.Substring(1, condition.Length - 2);
            _resource = resource;
        }

        public void Initialize(string operation)
        {
            string[] conditionParts = _condition.Split(new string[] { operation }, StringSplitOptions.RemoveEmptyEntries);

            if (conditionParts.Length == 2)
            {
                string leftSide = conditionParts[0].Trim(),
                        rightSide = conditionParts[1].Trim();

                if (!leftSide.StartsWith("'"))
                {
                    try
                    {
                        LeftSideInt = Int32.Parse(leftSide);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            LeftSide = "'" + ReplaceUtils.GetProperty(leftSide, _resource).ToString() + "'";
                        }
                        catch (Exception) { }
                    }
                }
                else LeftSide = leftSide;

                if (!rightSide.StartsWith("'"))
                {
                    try
                    {
                        RightSideInt = Int32.Parse(rightSide);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            RightSide = "'" + ReplaceUtils.GetProperty(rightSide, _resource).ToString() + "'";
                        }
                        catch (Exception) { }
                    }
                }
                else RightSide = rightSide;
            }
            else throw new Exception("Invalid operation in " + _condition);
        }

        public string LeftSide
        {
            get
            {
                return _leftSide;
            }

            set
            {
                _leftSide = value;
            }
        }

        public string RightSide
        {
            get
            {
                return _rightSide;
            }

            set
            {
                _rightSide = value;
            }
        }

        public int? RightSideInt
        {
            get
            {
                return _rightSideInt;
            }

            set
            {
                _rightSideInt = value;
            }
        }

        public int? LeftSideInt
        {
            get
            {
                return _leftSideInt;
            }

            set
            {
                _leftSideInt = value;
            }
        }
    }
}
