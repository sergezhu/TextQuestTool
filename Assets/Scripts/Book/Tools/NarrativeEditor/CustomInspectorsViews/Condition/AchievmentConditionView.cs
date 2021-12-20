#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class AchievmentConditionView : BaseConditionView
    {
        protected AchievmentCondition Instance;

        public AchievmentConditionView(AchievmentCondition instance)
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
            DrawConditionTypeHeader();
        }


        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            float calculatedWidth = 0;
            foreach (var x in targetCharacter.Achievments)
            {
                if (x is null) continue;

                DrawReceiveableElement(x, ref calculatedWidth);
                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }
        protected override string GetShowListHeader(Character character)
        {
            return $"Показывать список ачивок [{character.Achievments.Count}]:";
        }
        protected override void DrawConditionEnumPopup()
        {
            Instance.currentConditionType = (BaseBoolCondition.ConditionType) EditorGUILayout.EnumPopup(
                Instance.currentConditionType, GUILayout.MinWidth(80), GUILayout.MaxWidth(220));
        }
        
        protected void DrawFieldBlock()
        {
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            if (Instance.achievement is null)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ачивка для условия: ", GUILayout.Width(_labelWidth));
                Instance.achievement = EditorGUILayout.ObjectField( Instance.achievement, typeof(Achievment), false,
                    GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(24)) as Achievment;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Не выбрана ачивка.", _redtextStyle, GUILayout.MaxWidth(_fullWidth - _lineHeight * 3));
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранная ачивка: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{Instance.achievement.title}" + _stringPadding, new GUIStyle("box"));
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    Instance.achievement = null;
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif