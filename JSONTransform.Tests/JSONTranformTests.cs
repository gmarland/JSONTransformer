using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using JSONTranform;
using System.IO;

namespace JSONTranform.Tests
{
    [TestClass]
    public class TransformationHelperTests
    {
        // Transform JSON tests

        [TestMethod]
        public void TransformSimpleJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\simple.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simple.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("I'm doing pretty well", (string)child["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformArrayRouteJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\arrayRoute.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simple.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(2, resultArray.Count);

            Assert.IsInstanceOfType(resultArray[0], typeof(JArray));

            JArray childOne = (JArray)resultArray[0];

            Assert.AreEqual(childOne.Count, 3);

            foreach (JToken response in childOne.Children())
            {
                if (response["responseType"] != null) Assert.AreEqual("survey", (string)response["responseType"]);
                else if (response["item"] != null)
                {
                    if (((string)response["item"]) == "question") Assert.AreEqual("How are you doing?", (string)response["text"]);
                    else if (((string)response["item"]) == "answer") Assert.AreEqual("I'm doing pretty well", (string)response["text"]);
                }
            }

            Assert.IsInstanceOfType(resultArray[1], typeof(JArray));

            JArray childTwo = (JArray)resultArray[1];

            Assert.AreEqual(childTwo.Count, 3);

            foreach (JToken response in childTwo.Children())
            {
                if (response["responseType"] != null) Assert.AreEqual("survey", (string)response["responseType"]);
                else if (response["item"] != null)
                {
                    if (((string)response["item"]) == "question") Assert.AreEqual("What is the weather like?", (string)response["text"]);
                    else if (((string)response["item"]) == "answer") Assert.AreEqual("It's pretty sunny outside", (string)response["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformMultipleJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\multiple.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("I'm doing pretty well and I wish I was better", (string)child["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformNestedJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\nested.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("I'm doing pretty well", (string)child["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformArrayJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\array.json");
            JToken transform = GetJSONObject(@"JSON\Transform\array.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("I'm doing pretty well", (string)child["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformArrayWholeJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\array.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simple.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer")
                    {
                        JArray childArray = (JArray)child["text"];

                        Assert.AreEqual(2, childArray.Count);
                        Assert.AreEqual("I'm doing pretty well", childArray[0]);
                        Assert.AreEqual("I wish I was better", childArray[1]);
                    }
                }
            }
        }

        [TestMethod]
        public void TransformNestedArrayJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedArray.json");
            JToken transform = GetJSONObject(@"JSON\Transform\arrayNested.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("A different response", (string)child["text"]);
                }
            }
        }

        [TestMethod]
        public void TransformArrayNestedJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\nestedArray.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JObject));

            JObject resultObject = ((JObject)result);

            Assert.AreEqual("survey", (string)resultObject["responseType"]);

            Assert.IsInstanceOfType(resultObject["items"], typeof(JArray));

            JArray itemsArray = ((JArray)resultObject["items"]);

            foreach (JToken entry in itemsArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if ((child["type"] != null) && (((string)child["type"]) == "answer"))
                {
                    Assert.IsInstanceOfType(child["text"], typeof(JArray));

                    JArray textArray = (JArray)child["text"];

                    Assert.AreEqual("I'm doing pretty well", textArray[0]);
                    Assert.AreEqual("A different response", textArray[1]);
                }
            }
        }

        [TestMethod]
        public void TransformNestedNestedJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\nestedNested.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 3);

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["responseType"] != null) Assert.AreEqual("survey", (string)child["responseType"]);
                else if (child["item"] != null)
                {
                    if (((string)child["item"]) == "question") Assert.AreEqual("How are you doing?", (string)child["text"]);
                    else if (((string)child["item"]) == "answer") Assert.AreEqual("A different response", (string)child["text"]["response_b"]);
                }
            }
        }

        [TestMethod]
        public void TransformNestedFailingJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\nestedFailing.json");

            bool failed = false;

            try
            {
                JToken result = Transformer.TransformJSON(source, transform);
            }
            catch (Exception)
            {
                failed = true;
            }

            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TransformSimpleEqualConditionalJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleEqualConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            CheckConditionalResults(result);
        }

        [TestMethod]
        public void TransformSimpleNotEqualConditionalJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleNotEqualConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            CheckConditionalResults(result);
        }

        [TestMethod]
        public void TransformSimplContainsConditionalJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleContainsConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            CheckConditionalResults(result);
        }

        [TestMethod]
        public void TransformSimpleAndConditionalJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleAndConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            CheckConditionalResults(result);
        }

        [TestMethod]
        public void TransformSimpleOrConditionalJSONValid()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleOrConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            CheckConditionalResults(result);
        }

        private void CheckConditionalResults(JToken result)
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 2);

            bool passingThere = false;
            bool failingThere = false;

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["item"] != null)
                {
                    if (((string)child["item"]) == "questionPassing")
                    {
                        passingThere = true;
                        Assert.AreEqual("How are you doing?", (string)child["text"]);
                    }
                    else if (((string)child["item"]) == "questionFailing") failingThere = true;
                }
            }

            Assert.IsTrue(passingThere);
            Assert.IsFalse(failingThere);
        }

        [TestMethod]
        public void TransformMaskedAndConditional()
        {
            JToken source = GetJSONObject(@"JSON\Source\nestedNested.json");
            JToken transform = GetJSONObject(@"JSON\Transform\maskedAndConditional.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 2);

            bool passingThere = false;
            bool failingThere = false;

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["passing"] != null)
                {
                    if (((string)child["passing"]["item"]) == "questionPassing")
                    {
                        passingThere = true;
                        Assert.AreEqual("How are you doing?", (string)child["passing"]["text"]);
                    }
                }
                if (child["failing"] != null) failingThere = true;
            }

            Assert.IsTrue(passingThere);
            Assert.IsFalse(failingThere);
        }

        [TestMethod]
        public void TransformEach()
        {
            JToken source = GetJSONObject(@"JSON\Source\array.json");
            JToken transform = GetJSONObject(@"JSON\Transform\simpleEach.json");

            JToken result = Transformer.TransformJSON(source, transform);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JArray));

            JArray resultArray = ((JArray)result);

            Assert.AreEqual(resultArray.Count, 2);

            bool answersThere = false;

            foreach (JToken entry in resultArray.Children())
            {
                Assert.IsInstanceOfType(entry, typeof(JObject));

                JObject child = (JObject)entry;

                if (child["answers"] != null)
                {
                    answersThere = true;

                    Assert.IsInstanceOfType(child["answers"], typeof(JArray));

                    JArray answersArray = (JArray)child["answers"];

                    Assert.AreEqual(2, answersArray.Count);

                    Assert.AreEqual(answersArray[0]["answer"], "I'm doing pretty well");
                    Assert.AreEqual(answersArray[1]["answer"], "I wish I was better");
                }
                if (child["failing"] != null) answersThere = true;
            }

            Assert.IsTrue(answersThere);
        }

        private JToken GetJSONObject(string path)
        {
            return JToken.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, path)));
        }
    }
}
