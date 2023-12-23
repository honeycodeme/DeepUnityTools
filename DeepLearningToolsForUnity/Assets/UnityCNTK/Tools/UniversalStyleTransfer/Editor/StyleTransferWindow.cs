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
        protected static Rect resultWindowDefaultRect = new Rect(leftColumnMaxWidth + 360, 200, 350, 350);

        //style transfer model
        protected UniversalStyleTransferModel styleTransferModel = null;
        public TextAsset styleTransferModelData;    //the data of the style transfer. need to be set in inspector to the data file

        protected DeviceDescriptor cpuDevice = null;
        protected DeviceDescriptor gpuDevice = null;

        //style related
        protected GUIStyle titleStyle;

        private void Awake()
        {
            contentWindow = new InputImageWindow(1, contentWindowDefaultRect, "Content Image");
            contentWindow.showImageMat = showImageMat;
            styleWindow = new InputImageWindow(2, styleWindowDefaultRect, "Style Image");
            styleWindow.showImageMat = showImageMat;
            resultWindow = new OutputImageWindow(3, resultWindowDefaultRect, "Result Image");
            resultWindow.showImageMat = showImageMat;

            contentSize = new Vector2Int(512, 512);
            styleSize = new Vector2Int(512, 512);

            //add the parameter sets
            styleTransferParams = new List<UniversalStyleTransferModel.ParameterSet>();
            for (int i = 0; i < 5; ++i)
            {
                styleTransferParams.Insert(0,new UniversalStyleTransferModel.ParameterSet((UniversalStyleTransferModel.PassIndex)i));
            }

            //get available device to run the style transfer on
            foreach(var d in DeviceDescriptor.AllDevices())
            {
                if (d.Type == DeviceKind.CPU && cpuDevice == null)
                {
                    cpuDevice = d;
                } else if (d.Type == DeviceKind.GPU && gpuDevice == null)
                {
                    gpuDevice = d;
                }
            }

            
        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/UnityDeeplearningTools/StyleTransferTool")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
        