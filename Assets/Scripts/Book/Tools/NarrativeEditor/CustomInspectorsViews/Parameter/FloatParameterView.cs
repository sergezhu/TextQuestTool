#if UNITY_EDITOR
using System.Globalization;
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class FloatParameterView : ParameterView
    {
        protected new FloatParameter Parameter;

        public FloatParameterView(FloatParameter parameter) : base(parameter)
        {
            Parameter = parameter;
        }
        
        public override void DrawAddOperationValueField()
        {
            string stringValue = "";
            try
            {
                stringValue = EditorGUILayout.TextField(Parameter.Value.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                Parameter.Value = float.Parse(stringValue);
            }
            catch (System.Exception e)
            {
                Debug.Log("Float Incorrect");
            }
        }
        public override void DrawRemoveOperationValueField()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Parameter.Value.ToString(CultureInfo.CurrentCulture), new GUIStyle("box"),
                GUILayout.MinWidth(20), GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();
        }
    }
}
#endif