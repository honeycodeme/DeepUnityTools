using System.Collections;
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

        protected virtual void DoWindow(int win