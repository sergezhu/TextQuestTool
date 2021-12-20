#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ItemConditionView : BaseConditionView
    {
        protected ItemCondition Instance;

        public ItemConditionView(ItemCondition instance)
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
            foreach (var x in targetCharacter.Items)
            {
                if (x is null) continue;

                DrawReceiveableElement(x, ref calculatedWidth);
                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }
        protected override string GetShowListHeader(Character character)
        {
            return $"Показывать список предметов [{character.Items.Count}]:";
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
            if (Instance.item is null)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Предмет для условия: ", GUILayout.Width(_labelWidth));
                Instance.item = EditorGUILayout.ObjectField( Instance.item, typeof(Item), false,
                    GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(24)) as Item;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Не выбран предмет.", _redtextStyle, GUILayout.MaxWidth(_fullWidth - _lineHeight * 3));
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранный предмет: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{Instance.item.title}" + _stringPadding, new GUIStyle("box"));
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    Instance.item = null;
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif