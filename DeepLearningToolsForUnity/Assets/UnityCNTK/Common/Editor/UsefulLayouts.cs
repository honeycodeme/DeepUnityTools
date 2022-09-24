﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UnityCNTK.Editor
{

    public class ResizableSubWindow
    {

        public Rect WindowRect { get; set; } = new Rect(50, 50, 100, 100);
        public Vector2 MinWindowSize { get; set; } = new Vector2(75, 50);
        protected bool isResize = false;
        public int ID { get; set; }

        protected static GUIStyle styleWindowResize = null;
        protected static GUIContent gcDrag = new GUIContent("", "drag to resize");
        protected static Vector2 buttonSize = new Vector2(10, 10);
        protected static float dragableTopHeight = 20;


        public string Title { get; set; }

        public ResizableSubWindow(int id, Rect initWindowRect, string title = "")
        {
            WindowRect = initWindowRect;
            Title = title;
            ID = id;
        }

        public ResizableSubWindow(int id, string title = "")
        {
            Title = title;
            ID = id;
        }


        public void OnGUI()
        {

            WindowRect = GUILayout.Window(ID, WindowRect, DoWindow, Title, GUILayout.ExpandWidth(true));
        }

        protected virtual void DoWindow(int windowID)
        {
            WindowRect = ResizeWindow(WindowRect, ref isResize, MinWindowSize);
            GUILayout.Label("");

            var dragAreaRect = new Rect(0, 0, WindowRect.width, dragableTopHeight);
            GUI.DragWindow(dragAreaRect);
            //GUI.Button(dragAreaRect,"");
        }

        public static Rect ResizeWindow(Rect windowRect, ref bool isRezising, Vector2 minWindowSize)
        {
            // this is a custom style that looks like a // in the lower corner
            if (styleWindowResize == null)
            {
                styleWindowResize = GUI.skin.GetStyle("WindowBottomResize");
                //Debug.Log("resizer style");
            }
            Vector2 mouse = Event.current.mousePosition;
            //rectangle for the drag button
            Rect r = new Rect(windowRect.size - buttonSize, buttonSize);

            if (Event.current.type == EventType.MouseDown && r.Contains(mouse))
            {
                isRezising = true;
                //Debug.L