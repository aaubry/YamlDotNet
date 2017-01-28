using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YamlDotNet.Samples.Helpers {
    public class ExampleRunner : MonoBehaviour {

        private StringTestOutputHelper helper = new StringTestOutputHelper();

        public string[] disabledTests = new string[] {};

        private class StringTestOutputHelper : ITestOutputHelper {
            private StringBuilder output = new StringBuilder();
            public void WriteLine() {
                output.AppendLine();
            }
            public void WriteLine(string value) {
                output.AppendLine(value);
            }
            public void WriteLine(string format, params object[] args) {
                output.AppendFormat(format, args);
                output.AppendLine();
            }

            public override string ToString() { return output.ToString(); }
            public          void   Clear()    { output = new StringBuilder(); }
        }

        public static string[] GetAllTestNames() {
            bool skipMethods;
            var results = new List<string>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.Namespace == "YamlDotNet.Samples" && t.IsClass) {
                    skipMethods = false;
                    foreach (MethodInfo mi in t.GetMethods()) {
                        if (mi.Name == "Main") {
                            SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
                            if (sa != null) {
                                results.Add(t.Name);
                                skipMethods = true;
                                break;
                            }
                        }
                        if (skipMethods) break;
                    }
                }
            }
            return results.ToArray();
        }

        public static string[] GetAllTestTitles() {
            bool skipMethods;
            var results = new List<string>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.Namespace == "YamlDotNet.Samples" && t.IsClass) {
                    skipMethods = false;
                    foreach (MethodInfo mi in t.GetMethods()) {
                        if (mi.Name == "Main") {
                            SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
                            if (sa != null) {
                                results.Add(sa.Title);
                                skipMethods = true;
                                break;
                            }
                        }
                        if (skipMethods) break;
                    }
                }
            }
            return results.ToArray();
        }

        private void Start() {
            bool skipMethods;
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.Namespace == "YamlDotNet.Samples" && t.IsClass && Array.IndexOf(disabledTests, t.Name) == -1) {
                    skipMethods = false;
                    foreach (MethodInfo mi in t.GetMethods()) {
                        if (mi.Name == "Main") {
                            SampleAttribute sa = (SampleAttribute) Attribute.GetCustomAttribute(mi, typeof(SampleAttribute));
                            if (sa != null) {
                                helper.WriteLine("{0} - {1}", sa.Title, sa.Description);
                                var testObject = t.GetConstructor(new Type[] { typeof(StringTestOutputHelper) }).Invoke(new object[] { helper });
                                mi.Invoke(testObject, new object[] {});
                                Debug.Log(helper.ToString());
                                helper.Clear();
                                skipMethods = true;
                                break;
                            }
                        }
                        if (skipMethods) break;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExampleRunner))]
    public class ExampleRunnerEditor : Editor {
        private ExampleRunner runner;
        private string[] allTests;
        private string[] allTitles;
        private bool[] enabledTests;

        public void OnEnable() {
            runner = (ExampleRunner) target;

            allTests  = ExampleRunner.GetAllTestNames();
            allTitles = ExampleRunner.GetAllTestTitles();
            enabledTests = new bool[allTests.Length];
            for (int i = 0;  i < allTests.Length; i++)
                enabledTests[i] = Array.IndexOf(runner.disabledTests, allTests[i]) == -1;
        }

        public override void OnInspectorGUI() {
            int nextDisabledIndex = 0;
            for (int i = 0;  i < allTests.Length; i++) {
                EditorGUI.BeginChangeCheck();
                if (!enabledTests[i])
                    nextDisabledIndex++;
                enabledTests[i] = EditorGUILayout.Toggle(allTitles[i], enabledTests[i]);
                if (EditorGUI.EndChangeCheck()) {
                    if (enabledTests[i]) {
                        var l = new List<string>(runner.disabledTests);
                        l.Remove(allTests[i]);
                        runner.disabledTests = l.ToArray();
                    } else {
                        var l = new List<string>(runner.disabledTests);
                        l.Insert(nextDisabledIndex, allTests[i]);
                        runner.disabledTests = l.ToArray();
                    }
                }
            }
        }
    }
#endif
}
