#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class BoolParameterView : ParameterView
    {
        protected new BoolParameter Parameter;

        public BoolParameterView(BoolParameter parameter) : base(parameter)
        {
            Parameter = parameter;
        }
        
        public override void DrawAddOperationValueField()
        {
            Parameter.Value = EditorGUILayout.Toggle(Parameter.Value, GUILayout.Width(16));
            DrawToggleDescription();
        }
        public override void DrawRemoveOperationValueField()
        {
            DrawToggleDescription();
        }

        private void DrawToggleDescription()
        {
            string toggleDescription = Parameter.Value ? "(активно / включено)" : "(неактивно / выключено)";
            EditorGUILayout.LabelField(toggleDescription, GUILayout.MaxWidth(150));
        }
    }
}
#endif
