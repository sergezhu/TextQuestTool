using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using System.Linq;
using Book.Tools.NarrativeEditor.EditorUtils;
using TMPro.EditorUtilities;
using UniRx;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Book.Tools.NarrativeEditor
{
    public partial class NarrativeEditor : EditorWindow
    {
	    public static EditorSettings Settings;
	    
	    private static NarrativePartGraph _currentPartGraph;
	    public static NarrativePartGraph CurrentPartGraph => _currentPartGraph;

	    private static NodeView[] _selectedNodeViews;
        private static NarrativeEditor _editor;

        public static int SelectedNodesCount => _selectedNodeViews == null ? -1 : _selectedNodeViews.Length;
        public static bool SelectedNodesContains(NodeView nodeView) => _selectedNodeViews?.Contains(nodeView) ?? false;

        private static float _zoomScale = 1f;
        private static float _barHeight = 50f;
        private static float _tabHeaderOffset = 18f;
        private static Rect _selectionRect;
        private static Rect _barAreaRect;
        private static Rect _workAreaRect;
        private static Rect _workAreaRectScaled;
        private static Vector2 _pivotPoint;
        private static Vector2 _pivotPointScaled;

        private static bool _editorStylesInitialized;
        private static bool _nodesStylesInitialized;
        private static GUIStyle _style; 
        private static GUIStyle _activeStyle;
        private static GUIStyle _groupStyle; 
        private static GUIStyle _groupActiveStyle;
        private static GUIStyle _groupHighlightStyle;
        private static GUIStyle _headerLineStyle;
        
		
        private static Vector2 _mousePosition;
        public static Vector2 MousePosition => _mousePosition;
        
        private static Vector2 _mEventPosBefore, _mEventPosAfter, _mEventPosAfterInverted, _mAreaPosBefore, _mAreaPosAfter, _mLastRightClick;
        
        /*private static Vector2 _contextMenuOffset;
        public static Vector2 ContextMenuOffset
        {
	        //get { return _contextMenuOffset = -0.5f * (_workAreaRectScaled.position - (_barHeight + _tabHeaderOffset) * Vector2.up) / _zoomScale; }
	        get { return _contextMenuOffset = -0.5f * (_workAreaRectScaled.position - (_barHeight + _tabHeaderOffset) * Vector2.up) / _zoomScale; }
        }*/

        private static int _hoveredGroupIndex;
        private static Vector2 _dragGridStartPosition; 
        private static Vector2 _dragSelectionBoxStartPosition; 
        private static Vector2[] _dragWindowOffsets;
        private static bool _editorDirtied;
        private static bool _isCurveDrawing;
        private static bool _isWindowsDragging;
        private static bool _needGroupsParentingHandle;
        private static bool _isPanning;
        private static bool _isSelectionBoxDrawing;
        private static bool _needShowContextMenu;
        private static bool _isLeaveWindow;
        private static bool _onLeaveWindow;
        private static bool _isEnterWindow;
        private static bool _navigationMapOn;
        private static bool _onEnterWindow;
        private static int _totalOperationsCount;
        private static int _progressOperationsCount;
        private static int _nodesToDelete;
        private static int _lastClickedWindowIndex;
        private static int _rightClickedWindowIndex;
        private static int _leftClickedWindowIndex;
        private static int _framesToShowContextMenu;
        private static int _culledNodesCount;
        private static int _culledLinksCount;
       
        private static HoverPortInfo _hoverPortInfo;
		private static HoverPortInfo _storedHoverPortInfo;

		private static NavigatorView _navigationMap;

		private IDisposable _disposables;
		private Matrix4x4 _mBeforeZoom;
		private Matrix4x4 _mAfterZoom;
		private Matrix4x4 _mDefault;
		
		//keyboard
		private static bool _keyShiftPressed;
		private static bool _keyAltPressed;
		
		//navigation map
		private static Vector2[] _navigationMapSize;
		private static RectOffset _navigationMapPaddings;
		private static int _navigationMapSizeIndex;
		

		private class HoverPortInfo
        {
	        public NodeView Parent;
	        public InputPort InputPort;
	        public OutputPort OutputPort;
        }

		[MenuItem("Narrative Editor/Go To Editor")]
        private static void ShowEditor()
        {
            _editor = GetWindow<NarrativeEditor>();
            _editor.minSize = new Vector2(800, 600);
            _editor.wantsMouseMove = true;
            _editor.titleContent = new GUIContent("Narrative Editor");
        }

        private void OnEnable()
        {
            Settings = Resources.Load("EditorSettings") as EditorSettings;

            if (Settings is null) return;

            _currentPartGraph = Settings.currentPartGraph;
            _zoomScale = _currentPartGraph == null ? 1 : _currentPartGraph.zoomScale;
            
            CheckOperationsAndEnd();
            ResetSelection();
            _lastClickedWindowIndex = -1;
            
            TryResetEditorStyles();
            TryResetNodeStyles();

            _navigationMap = _navigationMap ? _navigationMap : CreateInstance<NavigatorView>();
            _navigationMapOn = true;
            
            _navigationMapSize = new []
            {
	            new Vector2(150, 90), 
	            new Vector2(300, 180), 
	            new Vector2(450, 270)
            };
            _navigationMapPaddings = new RectOffset(0, 15, 15, 0);
            _navigationMapSizeIndex = 0;

            float t = Settings.autoSaveTime;

            _disposables = new CompositeDisposable();
            Observable.Timer (System.TimeSpan.FromSeconds (t)) 
	            .Repeat () 
	            .Subscribe (_ => { 
		            TimerUpdate();
	            }).AddTo((ICollection<IDisposable>) _disposables);
        }

        private void OnDisable()
        {
	        _disposables?.Dispose ();
        }

        private void TimerUpdate()
        {
	        if (Application.isPlaying)
		        return;

	        if (_editorDirtied)
			{
				if (_isWindowsDragging || _isCurveDrawing || _isSelectionBoxDrawing || _isPanning)
					return;
				
				_editorDirtied = false;
				EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
			}
		}
        
        public static void TryResetEditorStyles(bool force = false)
        {
	        if (_editorStylesInitialized && force == false)
		        return;
	        
	        _style = EditorResourcesProvider.skin.GetStyle("window");
	        _activeStyle = EditorResourcesProvider.activeSkin.GetStyle("window");
	        _groupStyle = EditorResourcesProvider.groupSkin.GetStyle("window");
	        _groupActiveStyle = EditorResourcesProvider.groupActiveSkin.GetStyle("window");
	        _groupHighlightStyle = EditorResourcesProvider.groupHighlightSkin.GetStyle("window");

	        _style.overflow.left = Mathf.RoundToInt(EditorResourcesProvider.PortWidth * -0.4f);
	        _style.overflow.right = Mathf.RoundToInt(EditorResourcesProvider.PortWidth * -0.4f);
	        _style.padding.left = 10 - _style.overflow.left;
	        _style.padding.right = 10 - _style.overflow.right;

	        _activeStyle.overflow.left = Mathf.RoundToInt(EditorResourcesProvider.PortWidth * -0.4f);
	        _activeStyle.overflow.right = Mathf.RoundToInt(EditorResourcesProvider.PortWidth * -0.4f);
	        _activeStyle.padding.left = 10 - _style.overflow.left;
	        _activeStyle.padding.right = 10 - _style.overflow.right;

	        _headerLineStyle = new GUIStyle();
	        _headerLineStyle.normal.background = EditorResourcesProvider.ColorTexture(Color.black * 0.3f);
	        _headerLineStyle.margin = new RectOffset(0, 0, 0, 0);
	        _headerLineStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        private void OnGUI()
		{
			if (_currentPartGraph != Settings.currentPartGraph)
			{
				ResetSelection();
				_currentPartGraph = Settings.currentPartGraph;
				_zoomScale = _currentPartGraph.zoomScale;
			}
			
			TryResetEditorStyles();
			TryResetNodeStyles();
			
			Event e = Event.current;
            
            _mBeforeZoom = GUI.matrix;
            _mEventPosBefore = e.mousePosition;
            _mousePosition = NodeEditorUtil.WindowToGridPosition(_mEventPosBefore, position, (_barHeight + _tabHeaderOffset) * Vector2.down,1/_zoomScale);
            _mEventPosAfter = _mousePosition;
            _mEventPosAfterInverted = NodeEditorUtil.GridToWindowPosition(_mEventPosAfter, position, (_barHeight + _tabHeaderOffset) * Vector2.down,1/_zoomScale);

            var r = new Rect(Vector2.zero, position.size);
            _isLeaveWindow = r.Contains(e.mousePosition) == false;


            
            /*#region debugCoordsCalc
            _pivotPoint = new Vector2(Screen.width / 2, (Screen.height - _barHeight) / 2 + _barHeight);
            _pivotPointScaled = NodeEditorUtil.LocalToWorldVector(_pivotPoint, Vector2.zero, _zoomScale);
            _workAreaRect = new Rect(0, _barHeight, Screen.width, Screen.height - _barHeight);
            
            _workAreaRectScaled = new Rect(0, 0, Screen.width, Screen.height);
            _workAreaRectScaled = NodeEditorUtil.LocalToWorldRect(_workAreaRect, _pivotPoint, _zoomScale * _zoomScale);
            var deltaHeight = _workAreaRectScaled.size.y - _barHeight*_zoomScale;
            _workAreaRectScaled = new Rect(_workAreaRectScaled.x, _barHeight*_zoomScale, _workAreaRectScaled.width,
	            _workAreaRectScaled.height - _barHeight*_zoomScale + deltaHeight);
            #endregion*/
            var _testWorkAreaRect = new Rect(0, _barHeight, Screen.width, Screen.height - _barHeight);

            if(_currentPartGraph != null)
				DrawGrid(position, 1 / _zoomScale, _currentPartGraph.panningOffset);

            _barAreaRect = new Rect(0, 0, Screen.width, _barHeight + _tabHeaderOffset);

            GUILayout.BeginArea(_barAreaRect);
            var headerResult = DrawHeader();
            GUILayout.EndArea();
            
            if (headerResult == false)
	            return;
			
            
            // debug blue area rect
			//EditorGUI.DrawRect(new Rect(_workAreaRectScaled.position.x + 4, _workAreaRectScaled.position.y + 4, 
			//     _workAreaRectScaled.size.x - 8, _workAreaRectScaled.size.y - 8), Color.blue * 0.5f);
			


			BeginZoomed(position, 1 / _zoomScale, _currentPartGraph.panningOffset, _barHeight + _tabHeaderOffset);
            _mAfterZoom = GUI.matrix;

            var areaRect = new Rect(0, 0 , Screen.width / _zoomScale, (Screen.height - _barHeight) / _zoomScale);
            GenerateCullingData(areaRect);
            
            BeginWindows();

            #region debug GUI
            //test rect that must be fill a work area with paddigs
            //EditorGUI.DrawRect(new Rect(_workAreaRectScaled.position.x + 4, _workAreaRectScaled.position.y + 4, 
	        //    _workAreaRectScaled.size.x - 8, _workAreaRectScaled.size.y - 8), Color.blue * 0.5f);
            
            
            //test mouse position rects
            //var beforeRect = new Rect(_mEventPosBefore, 8 * Vector2.one);
            //var beforeRect = new Rect(e.mousePosition, 6 / _zoomScale * Vector2.one);
            //EditorGUI.DrawRect(beforeRect, Color.red);
            //var centerConverted = new Rect(_workAreaRectScaled.center, 6 / _zoomScale * Vector2.one);
            //EditorGUI.DrawRect(centerConverted, Color.magenta);
            //var beforeRectConverted = NodeEditorUtil.GridToWindowRect(beforeRect, position, _barHeight * Vector2.down, 1 / _zoomScale);
            //EditorGUI.DrawRect(beforeRectConverted, Color.yellow);
            
            //test rect that must be in center of work area
            //EditorGUI.DrawRect(new Rect(NodeEditorUtil.WorldToLocalVector(_pivotPoint, Vector2.zero, _zoomScale), 4*Vector2.one), Color.red);
			
            //if(_neededStartDrag != -1)  StartDragWindow(_neededStartDrag);
            #endregion
            
            Controls(e);
            
            DrawWindows();
            
            //Controls(e);
            CheckPortIfHover(e.mousePosition);

	        EndWindows();

	        DrawSelectionBox(e.mousePosition);
            EndZoomed(position, 1 / _zoomScale, _currentPartGraph.panningOffset, _barHeight + _tabHeaderOffset);
            DrawNavigationMap(_workAreaRectScaled, new Rect(Vector2.zero, _workAreaRectScaled.size));

            TryShowContextMenuAtNextFrame(e);
            GUI.matrix = _mBeforeZoom;
            
            
            //DrawNavigationMap(_workAreaRectScaled, new Rect(Vector2.zero, _workAreaRectScaled.size));
		}
        
        private bool DrawHeader()
		{
			GUILayout.Space(4);
			EditorGUILayout.BeginHorizontal(_headerLineStyle);
			GUILayout.Space(15);
			EditorGUILayout.LabelField("Assign Graph:", GUILayout.Width(100));

			EditorGUI.BeginChangeCheck();
			Settings.currentPartGraph = (NarrativePartGraph) EditorGUILayout.ObjectField(Settings.currentPartGraph, typeof(NarrativePartGraph),
				false, GUILayout.Width(200));
			if (EditorGUI.EndChangeCheck())
			{
				DoObjectAsDirty(Settings);
			}
			
			_currentPartGraph = Settings.currentPartGraph;
			
			if (_currentPartGraph == null)
			{
				return false;
			}

			EditorGUILayout.LabelField("", GUILayout.Width(8));
			EditorGUILayout.LabelField($"ID : {CurrentPartGraph.ID}", GUILayout.Width(50));
			GUILayout.FlexibleSpace();
			DrawClearAllButton();
			DrawSaveButton();
			GUILayout.Space(15);
			EditorGUILayout.EndHorizontal();

			//var smallText = new GUIStyle(_headerLineStyle); 
			//smallText.fontSize = 10;
			EditorGUILayout.BeginHorizontal(_headerLineStyle);

			//Debug data
			if (false)
			{
				_workAreaRect = new Rect(0, _barHeight, Screen.width, Screen.height - _barHeight);
				EditorGUILayout.LabelField(//$"pivot: [{_pivotPoint.x} ,{_pivotPoint.y}]    " +
					$"workarea: [{_workAreaRect.x.ToString("f0")}  {_workAreaRect.y.ToString("f0")}  {_workAreaRect.width.ToString("f0")}  {_workAreaRect.height.ToString("f0")}]     " +
					$"zoomedArea: [{_workAreaRectScaled.x.ToString("f0")}  {_workAreaRectScaled.y.ToString("f0")}  {_workAreaRectScaled.width.ToString("f1")}  {_workAreaRectScaled.height.ToString("f1")}]     " +
					$"editor: [{position.x.ToString("f0")}  {position.y.ToString("f0")}  {position.width.ToString("f1")}  {position.height.ToString("f1")}]     " +
					//$"m_Before: [{_mEventPosBefore.x.ToString("f0")}  {_mEventPosBefore.y.ToString("f0")}]  " +
					//$"m_After: [{_mEventPosAfter.x.ToString("f0")}  {_mEventPosAfter.y.ToString("f0")}]     " +
					//$"m_Inv: [{_mEventPosAfterInverted.x.ToString("f0")}  {_mEventPosAfterInverted.y.ToString("f0")}]     " +
					//$"m_Zoom: [{_mAreaPosBefore.x.ToString("f0")}  {_mAreaPosBefore.y.ToString("f0")}]  [{_mAreaPosAfter.x.ToString("f0")}  {_mAreaPosAfter.y.ToString("f0")}]     " +
					$"zoom: [{_zoomScale.ToString("F2")}]    " +
					//$"shift: [{_keyShiftPressed}]    " +
					//$"alt: [{_keyAltPressed}]    " +
					$"isLeave: {_isLeaveWindow}");
			}
			else
			{
				EditorGUILayout.LabelField($"culledNodes: [{_culledNodesCount}]    " +
				                           $"culledLinks: [{_culledLinksCount}]", GUILayout.Width(230));
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField($"zoom: [{_zoomScale.ToString("F2")}]", GUILayout.Width(90));
			}
			
			EditorGUILayout.EndHorizontal();

			return true;
		}
		private void DrawClearAllButton()
		{
			if (GUILayout.Button("Clear Current", GUILayout.Width(120)))
			{
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();

				CurrentPartGraph.ClearAll();
				
				stopWatch.Stop();
				TimeSpan ts = stopWatch.Elapsed;
				//Debug.Log($"Cleared in <color=yellow><b> {ts.TotalSeconds}</b> sek</color>");
			}
		}
		private void DrawSaveButton()
		{
			if (GUILayout.Button("Save Game Data", GUILayout.Width(120)))
			{
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();

				Settings.currentGraph.SaveGameData();

				stopWatch.Stop();
				TimeSpan ts = stopWatch.Elapsed;
				//Debug.Log($"Saved in <color=yellow><b> {ts.TotalSeconds}</b> sek</color>");
			}
		}

		void DrawWindows()
        {
	        DrawNodeCurves();
	        
	        if (_keyAltPressed)
		        _hoveredGroupIndex = CalculateIndexOfHighlightedGroup();

	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
            {
				NodeView view = CurrentPartGraph.Nodes[i];
                NodeData data = view.nodeData;


                GUIStyle targetStyle = view is GroupNodeView ? _groupStyle : _style;

                if (_selectedNodeViews.Contains(view))
                {
	                targetStyle = view is GroupNodeView ? _groupActiveStyle : _activeStyle;
                }
                else if (_keyAltPressed)
                {
	                if (view is GroupNodeView && _selectedNodeViews.Length > 0)
	                {
		                targetStyle = _hoveredGroupIndex == i ? _groupHighlightStyle : _groupStyle;
	                }
                }

                try
                {
	                data.WindowRect = GUI.Window(i, data.WindowRect, DrawNodeWindow, data.WindowTitle, targetStyle);
                }
                catch (Exception e)
                {
	                Debug.Log($"exception index [{i}]");
                }

                //view.DrawPorts();
            }


	        if (_keyAltPressed == false)
	        {
		        if (_needGroupsParentingHandle)
		        {
			        HandleHoverGroups();
			        CurrentPartGraph.ReorderNodes(); 
			        _hoveredGroupIndex = -1;
			        _needGroupsParentingHandle = false;
		        }
		        
		        UpdateGroupsBounds();
	        }

	        ReorderWindows();
        }



		void DrawNodeWindow(int index)
        {
           //this condition for correct switching Parts (without NullExeption)
            if (index >= _currentPartGraph.Nodes.Count)
	            return;
            
            var node = _currentPartGraph.Nodes[index];
            

            if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
            {
	            //Debug.Log($"DrawNodeWindow MouseDown  <b>index: {index}   reordered: {CurrentPartGraph.NodesOrder.IndexOf(index)}   lastIndex: {_lastClickedWindowIndex} </b>");
	            //Debug.Log($"keyboardControl:{GUIUtility.keyboardControl}  hot: {EditorGUIUtility.hotControl}  nameOfFControl:{GUI.GetNameOfFocusedControl()}");
	            //GUIUtility.keyboardControl = 0;
	            //GUI.FocusWindow(index);
	            if (_lastClickedWindowIndex != index)
	            {
		            SelectNode(Event.current, index); 
		            //GUIUtility.hotControl = -1;
		            //GUI.FocusWindow(index);
	            }
	            
            }

            if (_isWindowsDragging && (_selectedNodeViews.Contains(node) || _selectedNodeViews.Contains(node.ParentGroup)))
            {
	            var indexInSelected = -1;

	            var r = node.nodeData.WindowRect;
	            r.position = _mousePosition - _dragWindowOffsets[index];
	            node.nodeData.WindowRect = r;
            }
            

            if (node.nodeData.IsCulled == false)
            {
	            EditorGUI.BeginChangeCheck();
	            
	            node.DrawWindow();

	            if (EditorGUI.EndChangeCheck())
		            DoObjectAsDirty(node);

	            if (_isCurveDrawing)
		            GUI.UnfocusWindow();
	            
	            node.DrawPorts();
            }
        }

		private void StartDragMultipleWindows(NodeView[] nodes)
		{
			if (nodes != null)
			{
				var partNodes = CurrentPartGraph.Nodes;
				_isWindowsDragging = true;
				_dragWindowOffsets = new Vector2[partNodes.Count];

				for (var index = 0; index < partNodes.Count; index++)
				{
					var node = partNodes[index];
					var needNodeMoving = nodes.Contains(node) || nodes.Contains(node.ParentGroup);
					//Debug.Log($"{index} : {nodes.Contains(node.ParentGroup)}");
					
					_dragWindowOffsets[index] = needNodeMoving ? 
						_mousePosition - node.nodeData.WindowRect.position : Vector2.zero;
				}
			}
		}
		private void EndDragMultipleWindows()
		{
			if (_isWindowsDragging == false) return;
			
			_isWindowsDragging = false;
			_dragWindowOffsets = null;
		}

		private void StartDrawSelectionBox(Vector2 mousePosition)
		{
			if (_isSelectionBoxDrawing)
				return;
			
			_dragSelectionBoxStartPosition = mousePosition;
			//_dragSelectionBoxStartPosition = NodeEditorUtil.WindowToGridPosition(_dragSelectionBoxStartPosition, position, _barHeight * Vector2.down - _currentPartGraph.panningOffset,1/_zoomScale);;
			//_dragSelectionBoxStartPosition = NodeEditorUtil.WindowToGridPosition(_dragSelectionBoxStartPosition, position, _barHeight * Vector2.down,1/_zoomScale);;
			_isSelectionBoxDrawing = true;
			
			CreateNodesRectSelection();
		}
		
		private void EndDrawSelectionBox()
		{
			if (_isSelectionBoxDrawing == false)
				return;
			
			_isSelectionBoxDrawing = false;
		}
		
		private static void DrawNodeCurves()
		{
			if (_isCurveDrawing && _storedHoverPortInfo != null)
			{
				var startPortPosition = _storedHoverPortInfo.OutputPort.WorldCenter;
				NodeEditorUtil.BuildingLinkDraw(startPortPosition, Event.current.mousePosition, EditorResourcesProvider.CurveDrawingColor, EditorResourcesProvider.Thickness);
			}
			
			if (CurrentPartGraph == null) return;
			
			foreach (Link link in CurrentPartGraph.Links)
			{
				if(link.IsCulled)
					continue;
				
				NodeEditorUtil.LinkDraw(link, EditorResourcesProvider.CurveMainColor, EditorResourcesProvider.Thickness);
			}
		}

        void Controls(Event e)
        {
            if (CurrentPartGraph == null)
                return;

            if (e.button == 0)
            {
	            /*if (_isLeaveWindow)
		            OnLeaveWindow(e);
	            else
		            OnEnterWindow(e);*/

	            if (e.type == EventType.MouseDown)
	            {
		            LeftClick(e);
		            
		            if(_lastClickedWindowIndex == -1)
			            ResetSelection();
	            }
	            else if (e.rawType == EventType.MouseUp)
	            {
		            CheckOperationsAndEnd();
	            }
	            else if (e.type == EventType.MouseDrag)
	            {
		            CreateNodesRectSelection();
	            }
            }

            if(e.button == 1)
            {
	            if (_isCurveDrawing) return;

	            if (e.type == EventType.MouseDown)
	            {
		            RightClick(e);
		            
		            if(_lastClickedWindowIndex == -1)
			            ResetSelection();
		            //e.Use();
	            }
            }

            if (e.button == 2)
			{
				if (e.type == EventType.MouseDown)
				{
					_dragGridStartPosition = e.mousePosition;
					_isPanning = true;
					//e.Use();
				}
				else if (e.type == EventType.MouseUp)
				{
					_isPanning = false;
				} 
				
				if (e.type == EventType.MouseDrag)
				{
					if(_isPanning)
						DoPanning(e);
				}
				
            } 
            
            if (e.type == EventType.ScrollWheel)
            {
	            if (_isPanning)
		            return;
	            
                DoZoom(e);
            }

            if (e.type == EventType.KeyDown)
            {
	            if (EditorGUIUtility.editingTextField == false)
	            {
		            if (e.keyCode == KeyCode.LeftShift || e.keyCode == KeyCode.RightShift)
		            {
			            _keyShiftPressed = true;
		            }
		            
		            if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
		            {
			            _keyAltPressed = true;
			            _needGroupsParentingHandle = true;
		            }

		            if (e.keyCode == KeyCode.Delete)
		            {
			            if(CanToDeleteSelection())
							DeleteSelection();
		            }

		            if (e.keyCode == KeyCode.M)
		            {
			            _navigationMapOn = !_navigationMapOn;
		            }

		            if ((e.keyCode == KeyCode.Alpha1 || e.keyCode == KeyCode.Keypad1) && _navigationMapOn)
		            {
			            _navigationMapSizeIndex = 0;
		            }
		            if ((e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.Keypad2) && _navigationMapOn)
		            {
			            _navigationMapSizeIndex = 1;
		            }
		            if ((e.keyCode == KeyCode.Alpha3 || e.keyCode == KeyCode.Keypad3) && _navigationMapOn)
		            {
			            _navigationMapSizeIndex = 2;
		            }
	            }
            }
            
            if (e.type == EventType.KeyUp)
            {
	            if (EditorGUIUtility.editingTextField == false)
	            {
		            if (e.keyCode == KeyCode.LeftShift || e.keyCode == KeyCode.RightShift)
		            {
			            _keyShiftPressed = false;
		            }
		            
		            if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
		            {
			            _keyAltPressed = false;
		            }
	            }
            }
        }

        private void CheckOperationsAndEnd()
        {
	        if (_isCurveDrawing) EndCurveDrawing();
	        if (_isWindowsDragging) EndDragMultipleWindows();
	        if (_isSelectionBoxDrawing) EndDrawSelectionBox();
        }

        private void StartCurveDrawing(Link assignedLink = null)
        {
	        _isCurveDrawing = true;
	        _storedHoverPortInfo = new HoverPortInfo();

	        if (assignedLink == null)
	        {
		        _storedHoverPortInfo.OutputPort = _hoverPortInfo.OutputPort;
		        _storedHoverPortInfo.Parent = _hoverPortInfo.Parent;
	        }
	        else
	        {
		        _storedHoverPortInfo.OutputPort = assignedLink.nodeOutputPort;
		        CurrentPartGraph.RemoveLink(assignedLink);
	        }

	        ResetSelection();
	        GUI.UnfocusWindow();
	        GUIUtility.keyboardControl = 0;
        }

        private void EndCurveDrawing()
        {
	        if (_hoverPortInfo != null)
		        if(_hoverPortInfo.InputPort != null)
					CreateLink(_storedHoverPortInfo.OutputPort, _hoverPortInfo.InputPort);

	        _storedHoverPortInfo = null;
	        _isCurveDrawing = false;
        }

        public static void CreateLink(OutputPort outputPort, InputPort inputPort)
        {
	        if (outputPort.AssignedLinksCount() > 0)
		        for (var index = outputPort.AssignedLinksCount() - 1; index >= 0; index--)
		        {
			        var link = outputPort.AssignedLinkAt(index);
			        CurrentPartGraph.RemoveLink(link);
		        }

	        CurrentPartGraph.AddLink(outputPort, inputPort);
        }


        private static void WindowsTranslate(Vector2 diff)
        {
	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
	        {
		        NodeData data = CurrentPartGraph.Nodes[i].nodeData;
		        
		        var r = data.WindowRect;
		        r.position += diff;
		        data.WindowRect = r;
	        }
        }
        
        void DoPanning(Event e)
		{
			Vector2 diff = e.mousePosition - _dragGridStartPosition;
			diff *= .6f / _zoomScale;
			_dragGridStartPosition = e.mousePosition;
			
			_currentPartGraph.panningOffset += diff;
			DoObjectAsDirty(_currentPartGraph);

			WindowsTranslate(diff);
			Repaint();
		}

        private static void ResetScroll()
		{
			WindowsTranslate(-1 * _currentPartGraph.panningOffset);

			_currentPartGraph.panningOffset = Vector2.zero;
			DoObjectAsDirty(Settings);
			
			_zoomScale = 1;
		}
        
        private static void DeleteSelection()
        {
	       CurrentPartGraph.DeleteMultipleNodes(_selectedNodeViews);
	       ResetSelection();
        }

        private static void ExcludeSelectionFromGroups()
        {
	        CurrentPartGraph.ExcludeMultipleNodesFromGroups(_selectedNodeViews);
        }
        
        private static void IncludeSelectionToGroup(int groupIndex)
        {
	        CurrentPartGraph.IncludeMultipleNodesToGroup(_selectedNodeViews, CurrentPartGraph.Nodes[groupIndex] as GroupNodeView);
        }

        private void DoZoom(Event e)
        {
            //Vector2 diff = e.delta.y * Vector2.up;
            var oldZoomScale = _zoomScale;
            
            Vector2 diff = new Vector2();
            _dragGridStartPosition = _mousePosition;
             
            float zoomScaleDelta = - e.delta.y * 0.01f;
            _zoomScale = Mathf.Clamp(_zoomScale * (1f + zoomScaleDelta), 0.20f, 1.0f);
            
            diff = -1 * Mathf.Sign(_zoomScale - oldZoomScale) * Mathf.Sqrt(Mathf.Abs((_zoomScale - oldZoomScale) * .25f)) * 
                   (e.mousePosition - _workAreaRect.size * 0.5f); 
            
            CurrentPartGraph.panningOffset += diff;
            CurrentPartGraph.zoomScale = _zoomScale;
            DoObjectAsDirty(CurrentPartGraph);
            
            WindowsTranslate(diff);
            Repaint();
        }

        private void RightClick(Event e)
        {
	        if (_isCurveDrawing) return;

	        _rightClickedWindowIndex = SelectNode(e);

	        //GUI.matrix = _m;
	        _needShowContextMenu = true;
	        _framesToShowContextMenu = 3;
	        //GUI.matrix = _mZoom;
        }

        private void LeftClick(Event e)
        {
	        _leftClickedWindowIndex = SelectNode(e);
	        bool portHover = CheckPortIfHover(e.mousePosition);

	        if (_leftClickedWindowIndex == -1 && _hoverPortInfo == null)
	        {
		        GUIUtility.keyboardControl = 0;
		        StartDrawSelectionBox(e.mousePosition);
	        }
	        else 
	        {
		        if (_hoverPortInfo != null)
		        {
			        bool selfWindowHover = false;
			        if (_leftClickedWindowIndex != -1)
			        {
				        selfWindowHover = (_hoverPortInfo.Parent == CurrentPartGraph.Nodes[_leftClickedWindowIndex]);
			        }

			        if (_leftClickedWindowIndex != -1 && selfWindowHover == false)
			        {
				        StartDragMultipleWindows(_selectedNodeViews);
			        }
			        else
			        {
				        if (_isCurveDrawing == false)
				        {
					        if (_hoverPortInfo.OutputPort != null)
					        {
						        StartCurveDrawing();
					        }

					        var port = _hoverPortInfo.InputPort;
					        if (port != null)
					        {
						        if (port.AssignedLinksCount() != 0)
						        {
							        //var link = links[links.Count - 1];
							        var link = port.AssignedLinkAt(port.AssignedLinksCount() - 1);
							        if (link != null)
								        StartCurveDrawing(link);
						        }
					        }
				        }
			        }
		        }
		        else
		        {
			        StartDragMultipleWindows(_selectedNodeViews);
		        }
	        }
        }

        

        private int SelectNode(Event e, int windowHandlerIndex = -1)
        {
	        var mousePosition = e.mousePosition;
	        
	        var clickedIndex = -1;
	        var sameElementClicked = false;
	        var count = CurrentPartGraph.Nodes.Count;
	        
	        for (int index = count - 1; index >= 0; index--)
            {
	            var orderedIndex = CurrentPartGraph.NodesOrder[index];

	            var normalWindowRect = CurrentPartGraph.Nodes[orderedIndex].nodeData.WindowRect;
	            
	            //if we know windowID that comes from DrawWindow handler, then we skip other window IDs
	            if (normalWindowRect.Contains(mousePosition) || windowHandlerIndex == orderedIndex)
	            {
		            clickedIndex = orderedIndex;

		            if (clickedIndex == _lastClickedWindowIndex)
		            {
			            sameElementClicked = true;
			            EditorGUIUtility.editingTextField = false;
			            //Debug.Log("<color=RED> same element click </color>");
		            }

		            if (e.button == 0)
		            {
			            if (_keyShiftPressed)
			            {
				            //Debug.Log($"shift clicked, selectedViews  {_selectedNodeViews.Length}");
				            
				            var groupsInSelection = _selectedNodeViews.Count(node => node is GroupNodeView);
				            if (groupsInSelection > 0)
					            continue;

				            if (CurrentPartGraph.Nodes[orderedIndex] is GroupNodeView)
					            continue;

				            var tempList = new List<NodeView>();
				            foreach (var view in _selectedNodeViews)
				            {
					            tempList.Add(view);
				            }
			            
				            if(tempList.Contains(CurrentPartGraph.Nodes[orderedIndex]) == false)
					            tempList.Add(CurrentPartGraph.Nodes[orderedIndex]);
				            
				            _selectedNodeViews = tempList.ToArray();
				            //Debug.Log($"shift clicked, selectedViews  {_selectedNodeViews.Length}");
			            }
			            else
			            {
				            if (_selectedNodeViews.Contains(CurrentPartGraph.Nodes[orderedIndex]) == false)
				            {
					            _selectedNodeViews = new []{CurrentPartGraph.Nodes[orderedIndex]};
				            }
			            }
			            
			            Selection.objects = _selectedNodeViews == null ? null : new[]{_selectedNodeViews[_selectedNodeViews.Length - 1]};
		            }
		            else if (e.button == 1)
		            {
			            if (_selectedNodeViews.Contains(CurrentPartGraph.Nodes[orderedIndex]) == false)
			            {
				            _selectedNodeViews = new []{CurrentPartGraph.Nodes[orderedIndex]};
			            }
			            
			            Selection.objects = _selectedNodeViews == null ? null : new[]{ CurrentPartGraph.Nodes[orderedIndex] };
		            }

		            if (sameElementClicked == false)
		            {
			            CurrentPartGraph.ReorderNodes(clickedIndex);
		            }

		            break;
	            }
            }
 
            _lastClickedWindowIndex = clickedIndex;

            return clickedIndex;
        }

        private bool CheckPortIfHover(Vector2 mousePosition)
        {
	        _hoverPortInfo = null;

	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
            {
                var node = CurrentPartGraph.Nodes[i];
                
                BasePort hoveredPort = node.GetHoverInputPort(mousePosition);
                
                if ((hoveredPort is null) == false)
                {
	                _hoverPortInfo = new HoverPortInfo();
	                _hoverPortInfo.Parent = node;
	                _hoverPortInfo.InputPort = (InputPort) hoveredPort;
	                break;
                }

                hoveredPort = node.GetHoverOutputPort(mousePosition);
                if ((hoveredPort is null) == false)
                {
	                _hoverPortInfo = new HoverPortInfo();
	                _hoverPortInfo.Parent = node;
	                _hoverPortInfo.OutputPort = (OutputPort) hoveredPort;
					break;
                }
                   
            }
	        Repaint();
	        return _hoverPortInfo != null;
        }

        public static void DoObjectAsDirty(UnityEngine.Object obj)
        {
	        EditorUtility.SetDirty(obj);
	        _editorDirtied = true;
	        DoSceneAsDirty();
        }
        
        private static void DoSceneAsDirty()
        {
	        var scene = SceneManager.GetActiveScene();
	        if(scene.isDirty || Application.isPlaying)
		        return;
	        EditorSceneManager.MarkSceneDirty(scene);
        }
        
        public static void BeginZoomed(Rect rect, float zoom, Vector2 panOffset, float topPadding) {
            GUI.EndClip();

            //Vector2 pivot = _zoomPivot;
            Vector2 pivot = rect.size * 0.5f;  
            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, pivot);
            //Vector4 padding = new Vector4(0, topPadding, 0, 0);
            //padding *= zoom;

            //outer variable is for test
            _pivotPoint = pivot;
            _workAreaRect = rect;
            _workAreaRectScaled = new Rect(
	            -((rect.width * zoom) - rect.width) * 0.5f, 
	            -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
	            rect.width * zoom, 
	            rect.height * zoom);
            _workAreaRect = new Rect(_workAreaRect.position + panOffset * zoom, _workAreaRect.size);

            /*_workAreaRectScaled = new Rect(-((rect.width * zoom) - rect.width) * pivot.x / rect.size.x, -(((rect.height * zoom) - rect.height) * pivot.y / rect.size.y) + (topPadding * zoom),
	            rect.width * zoom, rect.height * zoom);*/
            GUI.BeginClip(_workAreaRectScaled);
        }

        public static void EndZoomed(Rect rect, float zoom, Vector2 panOffset, float topPadding) {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0);
            offset += (Vector3)(-1 * panOffset * zoom);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }
        
        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

	        rect.position = Vector2.zero;

	        Vector2 center = rect.size / 2f;
	        Texture2D gridTex = EditorResourcesProvider.gridTexture;
	        Texture2D crossTex = EditorResourcesProvider.crossTexture;

	        // Offset from origin in tile units
	        float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
	        float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

	        Vector2 tileOffset = new Vector2(xOffset, yOffset);

	        // Amount of tiles
	        float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
	        float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

	        Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

	        // Draw tiled background
	        GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
	        GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }
        
        public void DrawSelectionBox(Vector2 mousePosition)
        {
	        if (_isSelectionBoxDrawing == false)
		        return;

	        Vector2 curPos = Event.current.mousePosition;
	        //curPos = NodeEditorUtil.WindowToGridPosition(curPos, position, _barHeight * Vector2.down,1/_zoomScale);
	        
	        Vector2 size = curPos - _dragSelectionBoxStartPosition;
	        Rect r = new Rect(_dragSelectionBoxStartPosition, size);

	        if (r.width < 0)
	        {
		        r.x -= Mathf.Abs(r.width);
		        r.width = -1 * r.width;
	        }
	        if (r.height < 0)
	        {
		        r.y -= Mathf.Abs(r.height);
		        r.height = -1 * r.height;
	        }
	        //r.position = NodeEditorUtil.GridToWindowPosition(r.position, position, _barHeight * Vector2.down,1/_zoomScale);
	        //r.size *= _zoomScale;
	        Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));

	        _selectionRect = r;
        }

        public void CreateNodesRectSelection()
        {
	        if (_isSelectionBoxDrawing == false)
		        return;

	        //Debug.Log("CreateNodesRectSelection");
	        var selection = new List<NodeView>();
	        
	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
	        {
		        NodeView view = CurrentPartGraph.Nodes[i];
		        NodeData data = view.nodeData;

		        //var groupsInSelection = _selectedNodeViews.Count(node => node is GroupNodeView);

		        //if (selection.Contains(view) == false && _selectionRect.Contains(data.WindowRect) && (view is GroupNodeView) == false)
		        if (selection.Contains(view) == false && _selectionRect.FullyContains(data.WindowRect) && (view is GroupNodeView) == false)
		        {
			        selection.Add(view);
		        }
	        }

	        _selectedNodeViews = selection == null ? null : selection.ToArray();
        }
        
        private static void ResetSelection()
        {
	        //Debug.Log($"Reset Selectio {Random.Range(10000, 99999)}");
	        _selectedNodeViews = new NodeView[0];
        }
        
        public static bool CheckAllSelectionWithinOfGroup()
        {
	        foreach (var node in _selectedNodeViews)
	        {
		        if (node.ParentGroup == null)
			        return false;
	        }

	        return true;
        }
        
        public static bool CheckAllSelectionOutsideOfGroup()
        {
	        foreach (var node in _selectedNodeViews)
	        {
		        if (node.ParentGroup != null)
			        return false;
	        }

	        return true;
        }

        public static bool CanToDeleteSelection()
        {
	        return CanToDeleteNodes(_selectedNodeViews);
        }
        public static bool CanToDeleteNodes(NodeView[] nodes)
        {
	        if (nodes.Length == 0)
		        return false;
	        
	        var groupNodes = CurrentPartGraph.Nodes.Where(node => node is GroupNodeView).ToArray();
	        foreach (var node in groupNodes)
	        {
		        var check = ((GroupNodeView) node).CanToDeleteNodes(nodes);
		        //Debug.Log($"Nodes in Group[{node.id}] can to delete check " + check);
		        if (check == false)
		        {
			        return false;
		        }
	        }

	        return true;
        }
        
        public static bool CanToExcludeSelection()
        {
	        return CanToExcludeNodes(_selectedNodeViews);
        }
        public static bool CanToExcludeNodes(NodeView[] nodes, bool allowFreeNodes = false)
        {
	        if (nodes.Length < 1)
		        return false;
	        
	        var freeNodes = nodes.Where(node => node.ParentGroup is null).ToArray();
	        if (freeNodes.Length > 0 && allowFreeNodes == false)
		        return false;
	        
	        var groupNodes = CurrentPartGraph.Nodes.Where(node => node is GroupNodeView).ToArray();
	        foreach (var node in groupNodes)
	        {
		        var check = ((GroupNodeView) node).CanToExcludeNodes(nodes);
		        //Debug.Log("can to exclude check " + check);
		        if (check == false)
		        {
			        return false;
		        }
	        }

	        return true;
        }
        
        private static void UpdateGroupsBounds()
        {
	        CurrentPartGraph.Nodes.Where(node => node is GroupNodeView).ToList()
		        .ForEach(node => ((GroupNodeView) node).UpdateBounds());
        }

        private static Vector2 CalculateAverageCenter(NodeView[] nodes)
        {
	        var result = Vector2.zero;
	        
	        foreach (var node in nodes)
	        {
		        result += node.nodeData.WindowRect.position + node.nodeData.WindowRect.size * 0.5f;
	        }

	        return result / nodes.Length;
        }
        
        private static int CalculateIndexOfHighlightedGroup()
        {
	        var hoveredGroupIndex = -1;
	        var selectionContainsGroups = _selectedNodeViews.Count(node => node is GroupNodeView) > 0;
	        
	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
	        {
		        NodeView view = CurrentPartGraph.Nodes[i];

		        if ((_selectedNodeViews.Contains(view) == false) && (view is GroupNodeView) &&
		            (_selectedNodeViews.Length > 0) && selectionContainsGroups == false)
		        {
			        var selectedNodesAveragePosition = CalculateAverageCenter(_selectedNodeViews);
			        if (view.nodeData.WindowRect.Contains(selectedNodesAveragePosition))
			        {
				        hoveredGroupIndex = i;
			        }
		        }
	        }

	        return hoveredGroupIndex;
        }
        
        private void HandleHoverGroups()
        {
	        if (_hoveredGroupIndex == -1)
	        {
		        //trying exclude selection from group, selection nodes could be stay in old group or free
		        if (CanToExcludeNodes(_selectedNodeViews, true))
		        {
			        ExcludeSelectionFromGroups();
		        }
	        }
	        else
	        {
		        var targetGroup = CurrentPartGraph.Nodes[_hoveredGroupIndex] as GroupNodeView;
		        var trimOutsideGroupSelection = _selectedNodeViews.Where(node => node.ParentGroup != targetGroup).ToList();

		        if (CanToExcludeNodes(trimOutsideGroupSelection.ToArray(), true))
		        {
			        IncludeSelectionToGroup(_hoveredGroupIndex);
		        }
	        }
        }

        private void ReorderWindows()
        {
	        for (int i = 0; i < CurrentPartGraph.NodesOrder.Count; i++)
	        {
		        GUI.BringWindowToFront(CurrentPartGraph.NodesOrder[i]);
	        }
        }

        public static void TryResetNodeStyles(bool force = false)
        {
	        if (_nodesStylesInitialized && force == false || CurrentPartGraph == null)
		        return;
	        
	        for (int i = 0; i < CurrentPartGraph.Nodes.Count; i++)
	        {
		        var node = CurrentPartGraph.Nodes[i];
		        if (node is NarrativePointNodeView)
		        {
			        ((NarrativePointNodeView)node).ResetStyles();
		        };
	        }

	        _nodesStylesInitialized = true;
        }

        public void GenerateCullingData(Rect areaRect)
        {
	        var count = 0;
	        
	        foreach (var node in CurrentPartGraph.Nodes)
	        {
		        node.nodeData.IsCulled = areaRect.PartiallyContains(node.nodeData.WindowRect) == false 
		                                 && SelectedNodesContains(node) == false
		                                 && SelectedNodesContains(node.ParentGroup) == false;
		        count += node.nodeData.IsCulled ? 1 : 0;
	        }
	        _culledNodesCount = count;
	        
	        count = 0;
	        foreach (var link in CurrentPartGraph.Links)
	        {
		        var linkRect = link.GetRect();
		        if (linkRect is null)
		        {
			        link.IsCulled = true;
		        }
		        else
		        {
			        link.IsCulled = areaRect.PartiallyContains(linkRect.Value) == false;
		        }
		        
		        count += link.IsCulled ? 1 : 0;
	        }

	        _culledLinksCount = count;
        }

        private void DrawNavigationMap(Rect editorRect, Rect viewRect)
        {
	        if (_navigationMapOn == false)
		        return;

	        var matrix = GUI.matrix;
	        GUI.matrix = _mBeforeZoom;
	        
	        var mapRect = new Rect();
	        mapRect.size = _navigationMapSize[_navigationMapSizeIndex];
	        mapRect.x = editorRect.x + editorRect.width - mapRect.width - _navigationMapPaddings.right;
	        mapRect.y = -editorRect.y + (_barHeight + _tabHeaderOffset) + _navigationMapPaddings.top;

	        var nodesRects = CurrentPartGraph.Nodes
		        .Where(node => (node is GroupNodeView) == false)
		        .Select(node => node.nodeData.WindowRect)
		        .ToArray();
	        _navigationMap.DrawWindow(mapRect, viewRect, nodesRects);

	        GUI.matrix = matrix;
        }
    }
}
