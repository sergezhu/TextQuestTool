#if UNITY_EDITOR
using System.Collections.Generic;
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ChangeItemsLineActionView : BaseCharacterPropertyActionView
    {
        protected new ChangeItemsLineAction Instance;
        
        public ChangeItemsLineActionView(ChangeItemsLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        protected GUIStyle CountertextStyle;
        public override void DrawInspector(Rect rect)
        {
            base.DrawInspector(rect);
            DrawItemBlock(ref Instance.character);
            
            EditorGUILayout.Space(10);
        }

        protected override void InitStyles(Rect rect)
        {
            base.InitStyles(rect);
            
            CountertextStyle = new GUIStyle();
            CountertextStyle.normal.textColor = Color.yellow * 0.9f;
            CountertextStyle.fixedHeight = _lineHeight;
            CountertextStyle.alignment = TextAnchor.MiddleCenter;
            CountertextStyle.normal.background = Tools.NarrativeEditor.EditorResourcesProvider.ColorTexture(Color.yellow * 0.5f);
        }

        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            int index = -1;
            var items = targetCharacter.Items;
            var itemsCount = targetCharacter.ItemsCount;
            float calculatedWidth = 0;

            foreach (var x in items)
            {
                index++;
                if (x is null) continue;

                float currentWidth = StringUtil.CalculateStringWidth(x.title, _stringPadding.Length, _averageWidthPerSymbol);
                calculatedWidth += currentWidth;

                float currentCounterWidth = 0;
                currentCounterWidth = StringUtil.CalculateStringWidth(itemsCount[index].ToString(), _stringCounterPadding.Length,
                    _averageWidthPerSymbol);

                calculatedWidth += currentCounterWidth + _betweenElementsOffset;

                if (calculatedWidth > _fullWidth - _lineHeight * 3)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    calculatedWidth = currentWidth;
                }

                EditorGUILayout.LabelField(_stringPadding + $"{x.title}" + _stringPadding, _boxStyle,
                    GUILayout.Width(currentWidth));

                EditorGUILayout.LabelField(_stringCounterPadding + $"{itemsCount[index]}" + _stringCounterPadding,
                    CountertextStyle, GUILayout.Width(currentCounterWidth));

                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }

        protected void DrawItemBlock(ref Character targetCharacter)
        {
            if (targetCharacter is null)
                return;
            
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            
            if (Instance.item is null)
            {
                GUILayout.BeginVertical();
                DrawItemSelectField(ref Instance.item);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранный предмет: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{Instance.item.title}" + _stringPadding, _boxStyle);
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle); 

                if (Instance.item.isStackable)
                {
                    try
                    {
                        EditorGUILayout.LabelField(GetCountFieldHeader(), GUILayout.Width(80));
                        DrawItemCountField(ref Instance.deltaCountValue);
                        
                        if (Instance.isAddingOperation)
                            ValidateAddingDeltaCount();
                        else if (Instance.isRemovingOperation)
                            ValidateRemovingDeltaCount(Instance.item, targetCharacter);
                        //EditorGUILayout.LabelField(deltaCountValue.ToString(), GUILayout.Width(80));  //debug
                    }
                    catch (System.Exception e)
                    {
                        // ignored
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                DrawCheckInfo(targetCharacter, Instance.item);

                GUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    Instance.item = null;
            }

            GUILayout.EndHorizontal();
        }

        protected virtual void DrawCheckInfo(Character targetCharacter, Item targetItem)
        {
            if (Instance.isAddingOperation)
                DrawAddingInfo(targetItem, targetCharacter.Items);
            else if (Instance.isRemovingOperation)
                DrawRemovingInfo(targetItem, targetCharacter.Items);
        }

        private void DrawAddingInfo(Item targetItem, List<Item> items)
        {
            if (targetItem.isStackable == false)
            {
                DrawRedText("Внимание! Выбранный предмет может присутствовать только в единственном экземпляре.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));

                if (items.Contains(targetItem))
                {
                    DrawRedText("Он уже получен персонажем и добавлен не будет",
                        new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
                }
                else
                {
                    DrawRedText("Будет добавлен в количестве: 1",
                        new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
                }

                EditorGUILayout.LabelField("", GUILayout.Width(_lineHeight));
            }
        }
        
        private void DrawRemovingInfo(Item targetItem, List<Item> items)
        {
            if (items.Contains(targetItem) == false)
            {
                DrawRedText("Предмет отсутствует у персонажа и удален не будет",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
            else if (targetItem.isStackable == false)
            {
                DrawRedText("Внимание! Выбранный предмет может присутствовать \nтолько в единственном экземпляре.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
                DrawRedText("Будет удален в количестве: 1",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }

        protected void DrawItemSelectField(ref Item targetItem)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выберите предмет: ", GUILayout.Width(_labelWidth));
            targetItem = EditorGUILayout.ObjectField(targetItem, typeof(Item), false,
                GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as Item;
            GUILayout.EndHorizontal();

            DrawRedText("Не выбран предмет.",
                new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
        }
        
        protected void DrawItemCountField(ref int value)
        {
            value = int.Parse(EditorGUILayout.TextField(value.ToString(), GUILayout.Width(80)));
        }
        

        protected void ValidateAddingDeltaCount()
        {
            Instance.deltaCountValue = (Instance.deltaCountValue < 1) ? 1 : Instance.deltaCountValue;
        }
        
        protected void ValidateRemovingDeltaCount(Item targetItem, Character targetCharacter)
        {
            //var itemThatShouldBeFinded = targetItem;
            var itemIndex = targetCharacter.Items.FindIndex(x => x == targetItem);

            if (itemIndex == -1)
                return;
            
            var maxValue = targetCharacter.ItemsCount[itemIndex];
            Instance.deltaCountValue = Mathf.Clamp(Instance.deltaCountValue, 1, maxValue);
        }
        
        protected override string GetAddingTabHeader()
        {
            return "Добавить предметы";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить предметы";
        }

        private string GetCountFieldHeader()
        {
            if (Instance.isAddingOperation)
                return "Добавить:";
            else if (Instance.isRemovingOperation)
                return "Удалить:";

            return "";
        }
        protected override string GetShowListHeader(Character character)
        {
            return $"Показывать список предметов [{character.Items.Count}]:";
        }
    }
}
#endif