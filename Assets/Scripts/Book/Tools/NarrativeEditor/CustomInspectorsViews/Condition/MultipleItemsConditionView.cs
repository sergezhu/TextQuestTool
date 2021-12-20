#if UNITY_EDITOR
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class MultipleItemsConditionView : BaseConditionView
    {
        protected MultipleItemsCondition Instance;
        
        protected GUIStyle CountertextStyle;

        public MultipleItemsConditionView(MultipleItemsCondition instance)
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
            
        protected override void InitStyles()
        {
            base.InitStyles();
            
            CountertextStyle = new GUIStyle();
            CountertextStyle.normal.textColor = Color.yellow * 0.9f;
            CountertextStyle.fixedHeight = _lineHeight;
            CountertextStyle.alignment = TextAnchor.MiddleCenter;
            CountertextStyle.normal.background = Tools.NarrativeEditor.EditorResourcesProvider.ColorTexture(Color.yellow * 0.5f);
        }


        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            float calculatedWidth = 0;
            foreach (var x in targetCharacter.Items)
            {
                if (x is null) continue;
                
                float currentCounterWidth = 0;
                var count = targetCharacter.GetItemsCount(x);
                currentCounterWidth = StringUtil.CalculateStringWidth(count.ToString(), _stringCounterPadding.Length);

                calculatedWidth += currentCounterWidth;

                DrawReceiveableElement(x, ref calculatedWidth);
                EditorGUILayout.LabelField(_stringCounterPadding + $"{count.ToString()}" + _stringCounterPadding,
                    CountertextStyle, GUILayout.Width(currentCounterWidth));
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
            Instance.currentConditionType = (IntCondition.ConditionType) EditorGUILayout.EnumPopup(
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
                GUILayout.Space(4);
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Количество: ", GUILayout.Width(_labelWidth));
                
                var valueStr = EditorGUILayout.TextField(Instance.Value.ToString(), GUILayout.Width(80));

                try
                {
                    Instance.Value = int.Parse(valueStr);
                }
                catch
                {
                    // ignored
                }

                EditorGUILayout.LabelField("", GUILayout.Width(_lineHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                //DrawCheckInfo(targetItem, targetCharacter.Items);

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