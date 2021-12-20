#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ChangeBoolParameterLineActionView : ChangeParameterLineActionView
    {
        private ChangeBoolParameterLineAction Instance;
        
        public ChangeBoolParameterLineActionView(ChangeBoolParameterLineAction instance) : base (instance)
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
            return p.GetType() == typeof(BoolParameter);
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
            Instance.parameter = EditorGUILayout.ObjectField(Instance.parameter, typeof(BoolParameter), false,
                GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as BoolParameter;
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
                Instance.parameter.View().DrawChangeOperationValueField();
        }
        protected override string GetValueText()
        {
            return (Instance.isRemovingOperation || Instance.isChangingOperation) ? $"{_stringPadding}{Instance.parameter.GetStringedValue()}{_stringPadding}" : "";
        }

        protected override string GetAddingTabHeader()
        {
            return "Добавить Bool параметр";
        }
        protected string GetChangingTabHeader()
        {
            return "Изменить Bool параметр";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить Bool параметр";
        }
        protected override string GetShowListHeader(Character character)
        {
            var count = character.Parameters.Where(p => p != null).Count(p => p.GetType() == typeof(BoolParameter));
            return $"Показывать список параметров (bool) [{count}]:";
        }
    }
}
#endif