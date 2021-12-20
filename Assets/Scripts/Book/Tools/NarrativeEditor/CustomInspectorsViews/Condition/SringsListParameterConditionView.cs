#if UNITY_EDITOR
using System.Linq;
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class StringsListParameterConditionView : BaseConditionView
    {
        protected StringsListParameterCondition Instance;
        
        protected GUIStyle CountertextStyle;

        public StringsListParameterConditionView(StringsListParameterCondition instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect)
        {
            InitStyles();

            DrawID(Instance.ID);
            DrawCharacterBlock(ref Instance.character, ref Instance.ShowMoreData, rect);
            EditorGUILayout.Space(12);
            
            if (Instance.character is null)
                return;
            
            DrawFieldBlock();
        }

        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            float calculatedWidth = 0;
            foreach (var x in targetCharacter.Parameters) 
            {
                if (x is null) continue;
                if (x.GetType() != typeof(StringsListParameter)) continue;
                
                DrawReceiveableElement(x, ref calculatedWidth);
                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }
        protected override string GetShowListHeader(Character character)
        {
            var count = character.Parameters.Where(p => p != null).Count(p => p.GetType() == typeof(StringsListParameter));
            return $"Показывать список параметров (list) [{count}]:";
        }
        protected override void DrawConditionEnumPopup()
        {
            Instance.currentConditionType = (IntCondition.ConditionType) EditorGUILayout.EnumPopup(
                Instance.currentConditionType, GUILayout.MinWidth(80), GUILayout.MaxWidth(220));
        }

        private void DrawFieldBlock()
        {
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            if (Instance.parameter is null)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Параметр для условия: ", GUILayout.Width(_labelWidth));
                Instance.parameter = EditorGUILayout.ObjectField(Instance.parameter, typeof(StringsListParameter), false,
                    GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(24)) as StringsListParameter;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Не выбран параметр.", _redtextStyle, GUILayout.MaxWidth(_fullWidth - _lineHeight * 3));
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранный параметр: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{Instance.parameter.title}" + _stringPadding, new GUIStyle("box"));
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Значение для сравнения: ", GUILayout.Width(_labelWidth));
            
                Instance.selectedIndex = EditorGUILayout.Popup(Instance.selectedIndex, Instance.parameter.OptionsList.ToArray(),
                    GUILayout.MinWidth(80), GUILayout.MaxWidth(220));

                EditorGUILayout.LabelField("", GUILayout.Width(_lineHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                
                DrawConditionTypeHeader();
                GUILayout.Space(4);
                
                ShowParameterContainWarning();

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    Instance.parameter = null;
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif