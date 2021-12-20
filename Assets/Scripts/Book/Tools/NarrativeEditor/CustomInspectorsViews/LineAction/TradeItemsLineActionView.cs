#if UNITY_EDITOR
using System.Collections.Generic;
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class TradeItemsLineActionView : ChangeItemsLineActionView
    {
        private new TradeItemsLineAction Instance;
        
        public TradeItemsLineActionView(TradeItemsLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        //private GUIStyle _countertextStyle;
        public override void DrawInspector(Rect rect)
        {
            InitStyles(rect);
            DrawID(Instance.ID);
            DrawCharacterHeader(ref Instance.character, _fullWidth);
            DrawCharacterBlock(ref Instance.character, ref Instance.ShowMoreData, rect);

            if (Instance.character != null && Instance.character == Instance.opponent)
                Instance.character = null;
            
            GUILayout.Space(15);
            DrawItemsHeader(Instance.character);
            GUILayout.Space(10);
            DrawGivingItemsBlock(ref Instance.character);
            
            GUILayout.Space(12);
            
            DrawCharacterHeader(ref Instance.opponent, _fullWidth);
            DrawCharacterBlock(ref Instance.opponent, ref Instance.ShowMoreData, rect);
            
            if (Instance.opponent != null && Instance.character == Instance.opponent)
                Instance.opponent = null;
            
            GUILayout.Space(15);
            DrawItemsHeader(Instance.opponent);
            GUILayout.Space(10);
            DrawGivingItemsBlock(ref Instance.opponent);
        }

        private void DrawAddingInfo(Item targetItem, List<Item> characterItems, List<Item> opponentItems)
        {
            
        }

        protected void DrawGivingItemsBlock(ref Character targetCharacter)
        {
            if (targetCharacter is null)
                return;
            
            var removeIndex = -1;
            var givingItems = targetCharacter == Instance.character ? 
                Instance.GivingItemsFromCharacter : Instance.GivingItemsFromOpponent;
            var givingItemsCount = targetCharacter == Instance.character ? 
                Instance.GivingItemsCountFromCharacter : Instance.GivingItemsCountFromOpponent;
            
            for (int index = 0; index < givingItems.Count; index++)
            {
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {index+1}. ", GUILayout.Width(30));
                
                if (givingItems[index] == null)
                {
                    givingItems[index] = EditorGUILayout.ObjectField(givingItems[index], typeof(Item), false,
                        GUILayout.Width(200), GUILayout.Height(24)) as Item;
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(_stringPadding + givingItems[index].title + _stringPadding, _boxStyle);
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                    
                    EditorGUILayout.LabelField("Count:", GUILayout.Width(40));
                    
                    string str = EditorGUILayout.TextField(givingItemsCount[index].ToString(), GUILayout.Width(30));
                    try
                    {
                        givingItemsCount[index] = int.Parse(str);
                    }
                    catch (System.Exception e)
                    {
                        givingItemsCount[index] = 1;
                    }

                    if(givingItems[index].isStackable == false)
                        EditorGUILayout.LabelField( "   [Is not Stackable]   ", CountertextStyle, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.LabelField("", GUILayout.Width(10));

                if (GUILayout.Button("X", _crossButtonSmallStyle))
                    removeIndex = index;
                
                GUILayout.FlexibleSpace();
                
                if(EditorGUI.EndChangeCheck())
                {
                    Character.EditorItemsValidate(givingItems, givingItemsCount, index);
                    
                    if(givingItems[index] != null)
                        EditorUtility.SetDirty(givingItems[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
                
                DrawCheckInfo(targetCharacter, givingItems[index]);
            }
            
            if (removeIndex != -1)
            {
                if (givingItems[removeIndex] == null)
                    Instance.RemoveItemSlot(givingItems, givingItemsCount, removeIndex);
                else
                    Instance.ClearItemSlot(givingItems, givingItemsCount, removeIndex);
            }
        }
        protected override void DrawCheckInfo(Character targetCharacter, Item targetItem)
        {
            var otherCharacter = targetCharacter == Instance.character ? Instance.opponent : Instance.character;
            if (targetCharacter is null || otherCharacter is null || targetItem is null)
                return;
            
            var givingItems = targetCharacter == Instance.character ? 
                Instance.GivingItemsFromCharacter : Instance.GivingItemsFromOpponent;
            var givingItemsCount = targetCharacter == Instance.character ? 
                Instance.GivingItemsCountFromCharacter : Instance.GivingItemsCountFromOpponent;
            int itemIndex;
            int givingCount = Character.GetItemsCount(targetItem, givingItems, givingItemsCount);
            string errorText = Instance.ValidateTradedItem(targetCharacter, otherCharacter, targetItem, givingCount);
            if(string.IsNullOrEmpty(errorText) == false)
                DrawRedText(errorText, new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
        }

        protected void DrawCharacterHeader(ref Character targetCharacter, float width)
        {
            if (targetCharacter is null)
                return;
            
            GUILayout.Label($"Отдает персонаж  :  {targetCharacter.Name}", _tabSelectedStyle, GUILayout.Height(_lineHeight));
        }

        protected void DrawItemsHeader(Character targetCharacter)
        {
            if (targetCharacter is null)
                return;
            
            var givingItems = targetCharacter == Instance.character ? 
                Instance.GivingItemsFromCharacter : Instance.GivingItemsFromOpponent;
            var givingItemsCount = targetCharacter == Instance.character ? 
                Instance.GivingItemsCountFromCharacter : Instance.GivingItemsCountFromOpponent;
            int itemIndex = -1;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField("Передаваемые предметы:", GUILayout.Width(_labelWidth));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Slot", GUILayout.Width(90)))
            {
                Debug.Log("Try Add Giving Item");
                Instance.AddItemEmptySlot(givingItems, givingItemsCount);
                Character.EditorItemsValidate(givingItems, givingItemsCount);
            }
            EditorGUILayout.LabelField("", GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();
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