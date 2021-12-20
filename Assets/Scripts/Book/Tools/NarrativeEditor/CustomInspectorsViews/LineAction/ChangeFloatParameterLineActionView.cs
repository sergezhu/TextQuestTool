#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ChangeFloatParameterLineActionView : ChangeParameterLineActionView
    {
        private ChangeFloatParameterLineAction Instance;
        
        public ChangeFloatParameterLineActionView(ChangeFloatParameterLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect) 
        {
            base.DrawInspector(rect);
        }
        
        protected override void InitStyles(Rect rect)
        {
            base.InitStyles(rect);
        }

        protected override bool ParameterTypePredicat(Parameter p)
        {
            return p.GetType() == typeof(FloatParameter);
        }

        protected override void DrawParameterBlock(ref Character targetCharacter)
        {
            if (targetCharacter is null)
                return;
            
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            
            if (Instance.parameter is null)
            {
                GUILayout.BeginVertical();
                DrawParameterSelectFieldBlock();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                needClearField = DrawSelectedParameter();
                DrawCheckInfo(Instance.parameter, targetCharacter.Parameters);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    Instance.parameter = null;
            }

            GUILayout.EndHorizontal();
        }

        protected override void DrawParameterSelectField()
        {
            Instance.parameter = EditorGUILayout.ObjectField(Instance.parameter, typeof(FloatParameter), false,
                GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as FloatParameter;
        }
        protected override void ShowSelectedParameterTitle()
        {
            EditorGUILayout.LabelField(_stringPadding + $"{Instance.parameter.title}" + _stringPadding, _boxStyle);
        }
        protected override void ShowSelectedParameterValue()
        {
            string valueStr = GetValueText();
            if(Instance.isRemovingOperation)
                Instance.parameter.View().DrawRemoveOperationValueField();
            else if(Instance.isAddingOperation)
                Instance.parameter.View().DrawAddOperationValueField();
            else
                DrawChangeOperationValueField();
        }
        public void DrawChangeOperationValueField()
        {
            string stringValue = "";
            try
            {
                if (Instance.changedValueIsDelta == false)
                {
                    stringValue = EditorGUILayout.TextField(Instance.ChangedValue.ToString(CultureInfo.CurrentCulture), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    Instance.ChangedValue = float.Parse(stringValue);
                }
                else
                {
                    stringValue = EditorGUILayout.TextField(Instance.DeltaValue.ToString(CultureInfo.CurrentCulture), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    Instance.DeltaValue = float.Parse(stringValue);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Int Incorrect");
            }
        }
        protected override string GetValueText()
        {
            return (Instance.isRemovingOperation || Instance.isChangingOperation) ? $"{_stringPadding}{Instance.parameter.GetStringedValue()}{_stringPadding}" : "";
        }

        protected override string GetAddingTabHeader()
        {
            return "Добавить Float параметр";
        }
        protected string GetChangingTabHeader()
        {
            return "Изменить Float параметр";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить Float параметр";
        }
        protected override string GetShowListHeader(Character character)
        {
            var count = character.Parameters.Where(p => p != null).Count(p => p.GetType() == typeof(FloatParameter));
            return $"Показывать список параметров (float) [{count}]:";
        }
    }
}
#endif