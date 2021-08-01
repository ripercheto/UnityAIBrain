using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIBrain))]
public class AIBrainEditor : Editor
{
    private AIBrain brain;
    private GenericMenu createStateMenu;
    private GenericMenu rightClickMenu;
    private Type[] types;
    private Rect[] typeRects;
    private Rect statesRect;
    private List<Editor> cachedEditors;

    private int rightClickStartState;
    private int rightClickedState;

    private int draggedIndex = -1;
    private const float DRAG_PADDING = 50;

    public int DraggedIndex
    {
        get => draggedIndex; set
        {
            draggedIndex = value;
            Repaint();
        }
    }

    private void OnEnable()
    {
        brain = target as AIBrain;
        brain.onCurrentStateChanged.AddListener(Repaint);
        createStateMenu = new GenericMenu();
        cachedEditors = new List<Editor>();
        UpdateRectsSize();

        types = typeof(BaseState).Assembly.GetTypes().Where(t => t.BaseType == typeof(BaseState)).ToArray();
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            createStateMenu.AddItem(new GUIContent(type.ToString()), false, OnCreateMenuItemClicked, i);
        }

        rightClickMenu = new GenericMenu();
        //rightClickMenu.AddItem(new GUIContent("Add Switch Condition/Test"), false, OnRightClickMenuItemClicked, 1);
        rightClickMenu.AddItem(new GUIContent("Remove"), false, RemoveRightClickedState);
    }
    private void OnDisable()
    {
        brain.onCurrentStateChanged.RemoveListener(Repaint);
    }
    private void OnCreateMenuItemClicked(object data)
    {
        var index = (int)data;
        var type = types[index];
        var comp = brain.gameObject.AddComponent(type) as BaseState;
        comp.hideFlags = HideFlags.HideInInspector;
        brain.states.Add(comp);
        UpdateRectsSize();
    }

    void RemoveRightClickedState()
    {
        var state = brain.states[rightClickedState];
        RemoveState(state);
    }
    void RemoveState(BaseState state)
    {
        brain.states.Remove(state);
        DestroyImmediate(state);
        UpdateRectsSize();
    }
    private void OnRightClickMenuItemClicked(object data)
    {
        var i = (int)data;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUILayout.Label("States", EditorStyles.boldLabel);
            DrawAddStateButton();
            DrawShowStatesFoldout();
            GUILayout.Space(8);

            if (brain.editorShowStates)
            {
                EditorGUI.indentLevel++;
                {
                    UpdateEditorsSize(brain.states.Count);
                    for (int i = 0; i < brain.states.Count; i++)
                    {
                        BaseState state = brain.states[i];
                        if (state == null)
                        {
                            continue;
                        }

                        DrawState(state, i);
                    }
                    DrawNoneLabel();
                }
                EditorGUI.indentLevel--;
            }
        }
        EditorGUILayout.EndVertical();
        statesRect = GUILayoutUtility.GetLastRect();

        HandleEvent();

        void DrawAddStateButton()
        {
            if (GUILayout.Button("Add State"))
            {
                createStateMenu.ShowAsContext();
            }
        }

        void DrawShowStatesFoldout()
        {
            EditorGUI.indentLevel++;
            {
                brain.editorShowStates = EditorGUILayout.Foldout(brain.editorShowStates, "State Machine", true);
            }
            EditorGUI.indentLevel--;
        }

        void UpdateEditorsSize(int target)
        {
            while (cachedEditors.Count < target)
            {
                cachedEditors.Add(null);
            }
        }

        void DrawState(BaseState state, int i)
        {
            var c = Color.white;
            if (i == DraggedIndex)
            {
                c = Color.red;
            }
            else if (Application.isPlaying && i == brain.EditorCurrentStateIndex)
            {
                c = Color.green;
            }
            GUI.backgroundColor = c;

            //start
            var isRemoved = false;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var editor = cachedEditors[i];
                CreateCachedEditor(state, typeof(BaseStateEditor), ref editor);
                cachedEditors[i] = editor;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUILayout.ObjectField(state.GetType().Name, editor.serializedObject.FindProperty("m_Script").objectReferenceValue, typeof(UnityEngine.Object), false);
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("X", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                    {
                        RemoveState(state);
                        isRemoved = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (!isRemoved)
                {
                    editor.OnInspectorGUI();
                }
            }
            EditorGUILayout.EndVertical();

            //end
            if (i >= 0 && i < typeRects.Length)
            {
                typeRects[i] = GUILayoutUtility.GetLastRect();
            }
            GUI.backgroundColor = Color.white;
        }

        void DrawNoneLabel()
        {
            if (brain.states.Count == 0)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.LabelField("None");
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    private void HandleEvent()
    {
        var e = Event.current;
        HandleDragging();
        HandleRightclick();

        void HandleDragging()
        {
            if (!e.isMouse)
            {
                return;
            }
            if (e.button != 0)
            {
                return;
            }

            if (e.type == EventType.MouseDown && brain.states.Count > 1)
            {
                DraggedIndex = GetRectUnderMouse(e);
            }
            if (e.type == EventType.MouseDrag)
            {
                if (!statesRect.Contains(e.mousePosition))
                {
                    DraggedIndex = -1;
                }

                if (DraggedIndex < 0)
                {
                    return;
                }
                var index = GetRectUnderMouse(e);
                if (index >= 0 && index != DraggedIndex)
                {
                    var mPos = Event.current.mousePosition.y;
                    var r = typeRects[index];

                    if (DraggedIndex < index && mPos > r.position.y + r.height - DRAG_PADDING)
                    {
                        var draggedState = brain.states[DraggedIndex];
                        brain.states.Insert(index + 1, draggedState);
                        brain.states.RemoveAt(DraggedIndex);
                        DraggedIndex = index;
                    }
                    else if (DraggedIndex > index && mPos < r.position.y + DRAG_PADDING)
                    {
                        var draggedState = brain.states[DraggedIndex];
                        brain.states.Insert(index, draggedState);
                        brain.states.RemoveAt(DraggedIndex + 1);
                        DraggedIndex = index;
                    }
                }

            }
            if (e.type == EventType.MouseUp)
            {
                DraggedIndex = -1;
            }
        }

        void HandleRightclick()
        {
            if (!e.isMouse)
            {
                return;
            }
            if (e.button != 1)
            {
                return;
            }
            if (e.type == EventType.MouseDown)
            {
                rightClickStartState = GetRectUnderMouse(e);
            }
            if (e.type == EventType.MouseUp && rightClickStartState >= 0)
            {
                rightClickedState = GetRectUnderMouse(e);
                if (rightClickStartState < 0)
                {
                    return;
                }
                if (rightClickStartState != rightClickedState)
                {
                    return;
                }

                rightClickMenu.ShowAsContext();
            }
        }

    }
    private void UpdateRectsSize()
    {
        typeRects = new Rect[brain.states.Count];
    }

    private int GetRectUnderMouse(Event e)
    {
        for (int i = 0; i < typeRects.Length; i++)
        {
            Rect rect = typeRects[i];
            if (rect.Contains(e.mousePosition))
            {
                return i;
            }
        }
        return -1;
    }
}
