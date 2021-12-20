#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class CharacterView
    {
        protected Character Character;
        protected GUIStyle BoxStyle;
        
        public CharacterView(Character character)
        {
            Character = character;
        }
        
        private int _labelWidth;
        private int _textAreaHeight;
        private int _borderSpacing;
        private float _twoColumnWidth;

        private bool _itemCountEditAllow = false;

        private void StylesInit()
        {
            _labelWidth = 160;
            _textAreaHeight = 32;
            _borderSpacing = 20;
            _twoColumnWidth = Screen.width - _borderSpacing * 2 - _labelWidth;
            
            BoxStyle = new GUIStyle("box");
            BoxStyle.normal.textColor = Color.white * 0.8f;
            BoxStyle.margin = new RectOffset(0, 0, 0, 0);
        }

        public void DrawInspector()
        {
            StylesInit();
            EditorGUILayout.BeginVertical("box");
            DrawID(Character.ID);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Имя", GUILayout.Width(_labelWidth));
            Character.Name = EditorGUILayout.TextField(Character.Name, GUILayout.Width(_twoColumnWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Предыстория", GUILayout.Width(_labelWidth));
            Character.Description = EditorGUILayout.TextArea(Character.Description, GUILayout.Height(_textAreaHeight),
                GUILayout.Width(_twoColumnWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);

            DrawAchievmentsBlock();
            EditorGUILayout.Space(10);

            DrawAtributesBlock();
            EditorGUILayout.Space(10);

            DrawParametersBlock();
            EditorGUILayout.Space(10);

            DrawItemsBlock();
            EditorGUILayout.Space(10);
        }
        
        protected void DrawID(int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"ID : {id}", BoxStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }

        private void DrawParametersBlock()
        {
            int index;
            int removeIndex = -1;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField("Параметры", GUILayout.Width(_labelWidth));
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Slot", GUILayout.Width(90)))
            {
                Character.AddParameterEmptySlot();
                Character.EditorInitializeParametersValidate();
            }
            EditorGUILayout.LabelField("", GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();

            for (index = 0; index < Character.Parameters.Count; index++)
            {
                EditorGUI.BeginChangeCheck();
                
                var param = Character.Parameters[index];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {index+1}. ", GUILayout.Width(30));


                if (param is null)
                {
                    Character.Parameters[index] = EditorGUILayout.ObjectField(param, typeof(Parameter), false,
                        GUILayout.Width(200), GUILayout.Height(24)) as Parameter;
                    /*if (Character.Parameters[index] != null)
                    {
                        var clone = Character.Parameters[index].DoClone() as Parameter;
                        Character.Parameters[index] = clone;
                        AssetDatabase.AddObjectToAsset(clone, Character);
                    }*/
                        
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    param.title = EditorGUILayout.TextField(param.title);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    param.description = EditorGUILayout.TextArea(param.description, GUILayout.Height(_textAreaHeight));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (param.GetType() == typeof(IntParameter))
                        DrawParameterData((IntParameter)param);
                    
                    if (param.GetType() == typeof(FloatParameter))
                        DrawParameterData((FloatParameter)param);
                    
                    if (param.GetType() == typeof(BoolParameter))
                        DrawParameterData((BoolParameter)param);
                    
                    if (param.GetType() == typeof(StringsListParameter))
                        DrawParameterData((StringsListParameter)param);
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(24)))
                    removeIndex = index;
                
                if(EditorGUI.EndChangeCheck())
                {
                    Character.EditorInitializeParametersValidate(index);
                    if (Character.Parameters[index] != null)
                        EditorUtility.SetDirty(Character.Parameters[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(8);
            }

            EditorGUILayout.EndVertical();

            if (removeIndex != -1)
            {
                if (Character.Parameters[removeIndex] == null)
                    Character.RemoveParameterSlot(removeIndex);
                else
                    Character.ClearParameterSlot(removeIndex);
            }
        }
        private void DrawParameterData(IntParameter p)
        {
            EditorGUILayout.LabelField("Значение:", GUILayout.Width(_labelWidth));
            var stringValue = EditorGUILayout.TextField(p.Value.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            try
            {
                p.Value = int.Parse(stringValue);
            }
            catch (System.Exception e)
            {
                Debug.Log("Int Incorrect");
            }
        }
        private void DrawParameterData(FloatParameter p)
        {
            EditorGUILayout.LabelField("Значение:", GUILayout.Width(_labelWidth));
            var stringValue = EditorGUILayout.TextField(p.Value.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            try
            {
                p.Value = float.Parse(stringValue);
            }
            catch (System.Exception e)
            {
                Debug.Log("Float Incorrect");
            }
        }
        private void DrawParameterData(BoolParameter p)
        {
            EditorGUILayout.LabelField("Значение:", GUILayout.Width(_labelWidth));
            p.Value = EditorGUILayout.Toggle(p.Value, GUILayout.Width(20));
            EditorGUILayout.LabelField("", GUILayout.Width(0));
            string toggleDescription = p.Value ? "(активно / включено)" : "(неактивно / выключено)";
            EditorGUILayout.LabelField(toggleDescription);
        }
        private void DrawParameterData(StringsListParameter p)
        {
            int count = p.OptionsList.Count;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(8);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Значение: ", GUILayout.Width(_labelWidth));
            EditorGUILayout.LabelField("", GUILayout.Width(40));
            p.CurrentIndex = EditorGUILayout.Popup("", p.CurrentIndex, p.OptionsList.ToArray());
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawItemsBlock()
        {
            int index = -1;
            int removeIndex = -1;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField($"Предметы ({Character.Items.Count})", GUILayout.Width(_labelWidth));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Count Edit Allow:", GUILayout.Width(100));
            _itemCountEditAllow = EditorGUILayout.Toggle(_itemCountEditAllow, GUILayout.Width(50));
            if (GUILayout.Button("Add Slot", GUILayout.Width(90)))
            {
                Character.AddItemEmptySlot();
                Character.EditorItemsValidate(Character.Items, Character.ItemsCount);
            }
            EditorGUILayout.LabelField("", GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();


            for (index = 0; index < Character.Items.Count; index++)
            {
                EditorGUI.BeginChangeCheck();
                
                var item = Character.Items[index];
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {index+1}. ", GUILayout.Width(30));

                int count = -1;
                
                if (item == null)
                {
                    Character.Items[index] = EditorGUILayout.ObjectField(item, typeof(Item), false,
                        GUILayout.Width(200), GUILayout.Height(24)) as Item;
                    /*if (Character.Items[index] != null)
                    {
                        var clone = Character.Items[index].DoClone() as Item;
                        Character.Items[index] = clone;
                        AssetDatabase.AddObjectToAsset(clone, Character);
                    }*/
                }
                else
                {
                    count = Character.ItemsCount[index];
                    
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    item.title = EditorGUILayout.TextField(item.title);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    item.description = EditorGUILayout.TextArea(item.description, GUILayout.Height(_textAreaHeight));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField("Count:", GUILayout.Width(40));
                    if (_itemCountEditAllow == false)
                        EditorGUILayout.LabelField($"{count}", GUILayout.Width(30));
                    else
                    {
                         string str = EditorGUILayout.TextField(Character.ItemsCount[index].ToString(), GUILayout.Width(30));
                         try
                         {
                             Character.ItemsCount[index] = int.Parse(str);
                         }
                         catch (System.Exception e)
                         {
                             Character.ItemsCount[index] = 0;
                         }
                    }

                    EditorGUILayout.LabelField("", GUILayout.Width(80));
                    EditorGUILayout.LabelField("Stackable", GUILayout.Width(100));
                    EditorGUI.BeginChangeCheck();
                    item.isStackable = EditorGUILayout.Toggle(item.isStackable);
                    if(EditorGUI.EndChangeCheck() && item.isStackable == false)
                        item.StackableValidate();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(24)))
                    removeIndex = index;
                
                if(EditorGUI.EndChangeCheck())
                {
                    
                    Character.EditorItemsValidate(Character.Items, Character.ItemsCount, index);
                    if(Character.Items[index] != null)
                        EditorUtility.SetDirty(Character.Items[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();

            if (removeIndex != -1)
            {
                if (Character.Items[removeIndex] == null)
                    Character.RemoveItemSlot(removeIndex);
                else
                    Character.ClearItemSlot(removeIndex);
            }
        }

        private void DrawAtributesBlock()
        {
            int index;
            int removeIndex = -1;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField("Атрибуты", GUILayout.Width(_labelWidth));
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Slot", GUILayout.Width(90)))
            {
                Character.AddAtributeEmptySlot();
                Character.EditorInitializeAtributesValidate();
            }
            EditorGUILayout.LabelField("", GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();

            for (index = 0; index < Character.Atributes.Count; index++)
            {
                EditorGUI.BeginChangeCheck();
                
                var atribute = Character.Atributes[index];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {index+1}. ", GUILayout.Width(30));


                if (atribute is null)
                {
                    Character.Atributes[index] = EditorGUILayout.ObjectField(atribute, typeof(Atribute), false,
                        GUILayout.Width(200), GUILayout.Height(24)) as Atribute;
                    /*if (Character.Atributes[index] != null)
                    {
                        var clone = Character.Atributes[index].DoClone() as Atribute;
                        Character.Atributes[index] = clone;
                        AssetDatabase.AddObjectToAsset(clone, Character);
                    }*/
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    atribute.title = EditorGUILayout.TextField(atribute.title);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    atribute.description = EditorGUILayout.TextArea(atribute.description, GUILayout.Height(_textAreaHeight));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(24)))
                    removeIndex = index;
                
                if(EditorGUI.EndChangeCheck())
                {
                    
                    Character.EditorInitializeAtributesValidate(index);
                    if(Character.Atributes[index] != null)
                        EditorUtility.SetDirty(Character.Atributes[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();

            if (removeIndex != -1)
            {
                if (Character.Atributes[removeIndex] == null)
                    Character.RemoveAtributeSlot(removeIndex);
                else
                    Character.ClearAtributeSlot(removeIndex);
            }
        }

        private void DrawAchievmentsBlock()
        {
            int index;
            int removeIndex = -1;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField("Ачивки", GUILayout.Width(_labelWidth));
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Slot", GUILayout.Width(90)))
            {
                Character.AddAchievmentEmptySlot();
                Character.EditorInitializeAchievmentsValidate();
            }
            EditorGUILayout.LabelField("", GUILayout.Width(28));
            EditorGUILayout.EndHorizontal();
            
            for (index = 0; index < Character.Achievments.Count; index++)
            {
                EditorGUI.BeginChangeCheck();
                
                var achievment = Character.Achievments[index];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {index+1}. ", GUILayout.Width(30));

                if (achievment == null)
                {
                    Character.Achievments[index] = EditorGUILayout.ObjectField(achievment, typeof(Achievment),
                        GUILayout.Width(200), GUILayout.Height(24)) as Achievment;
                    /*if (Character.Achievments[index] != null)
                    {
                        var clone = Character.Achievments[index].DoClone() as Achievment;
                        Character.Achievments[index] = clone;
                        AssetDatabase.AddObjectToAsset(clone, Character);
                    }*/
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    achievment.title = EditorGUILayout.TextField(achievment.title);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    achievment.description =
                        EditorGUILayout.TextArea(achievment.description, GUILayout.Height(_textAreaHeight));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(24)))
                    removeIndex = index;
                
                if(EditorGUI.EndChangeCheck())
                {
                    Character.EditorInitializeAchievmentsValidate(index);
                    if(Character.Achievments[index] != null)
                        EditorUtility.SetDirty(Character.Achievments[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();

            if (removeIndex != -1)
            {
                if (Character.Achievments[removeIndex] == null)
                    Character.RemoveAchievmentSlot(removeIndex);
                else
                    Character.ClearAchievmentSlot(removeIndex);
            }
        }
    }
}
#endif