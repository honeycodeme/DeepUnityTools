using UnityEngine;
using UnityEditor;

using UnityCNTK.Tools.StyleAndTexture;
using System.Collections.Generic;
using System.Threading.Tasks;
using CNTK;
using System;

namespace UnityCNTK.Editor
{
    public class StyleTransferWindow : EditorWindow
    {
        
        protected List<Action> _actions = new List<Action>(8);
        protected List<Action> _backlog = new List<Action>(8);
        protected bool _queued = false;


        protected bool IsRunningTransfer { get { return isRunningTransfer; } set
            {
                isRunningTransfer = value;
                contentWindow.enableButtons = !isRunningTransfer;
                styleWindow.enableButtons = !isRunningTransfer;
            }
        }
        protected bool isRunningTransfer = false;

        public Material showImageMat;//will be set in editor
        protected InputImageWindow contentWindow;
        protected InputImageWindow styleWindow;
        protected OutputImageWindow resultWindow;

        //values to use
        protected bool blendingFactorFoldout = false;
        protected List<UniversalStyleTransferModel.ParameterSet> styleTransferParams;

        protected Vector2Int contentSize;
        protected Vector2Int styleSize;

        protected bool resizeContent = true;
        protected bool resizeStyle = true;

        protected bool useOriginalAlpha = true;

        //dimensions for the layout

        protected static Rect windowDefaultRect = new Rect(30, 60, 960, 720);
        protected static float leftColumnMaxWidth = 230;
        protected static Rect contentWindowDefaultRect = new Rect(leftColumnMaxWidth + 40, 30, 300, 330);
        protected static Rect styleWindowDefaultRect = new Rect(leftColumnMaxWidth + 40, 370, 300, 330);
        protected static Rect resultWindowDefaultRect = 