﻿using HandyControl.Controls;

using ICSharpCode.AvalonEdit;

using SharpVectors.Converters;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Runtime;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

using DpiScale = SharpVectors.Runtime.DpiScale;

namespace ESTControl.SvgViewer
{
    /// <summary>
    /// Interaction logic for DrawingPage.xaml
    /// </summary>
    public partial class DrawingPage : UserControl
    {
        public DrawingPage()
        {
            InitializeComponent();

            _saveXaml = true;
            _wpfSettings = new WpfDrawingSettings();
            _wpfSettings.CultureInfo = _wpfSettings.NeutralCultureInfo;

            _fileReader = new FileSvgReader(_wpfSettings);
            _fileReader.SaveXaml = _saveXaml;
            _fileReader.SaveZaml = false;

            _mouseHandlingMode = ZoomPanMouseHandlingMode.None;

            string workDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location), TemporalDirName);

            _workingDir = new DirectoryInfo(workDir);

            _embeddedImages = new List<EmbeddedImageSerializerArgs>();

            _embeddedImageVisitor = new EmbeddedImageSerializerVisitor(true);
            _wpfSettings.Visitors.ImageVisitor = _embeddedImageVisitor;

            _embeddedImageVisitor.ImageCreated += OnEmbeddedImageCreated;

            this.Loaded += OnPageLoaded;
            this.Unloaded += OnPageUnloaded;
            this.SizeChanged += OnPageSizeChanged;
        }


        /// <summary>
        /// 自定义画刷
        /// </summary>
        public Brush CustomBrush
        {
            get { return (Brush)GetValue(CustomBrushProperty); }
            set { SetValue(CustomBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBrushProperty =
            DependencyProperty.Register("CustomBrush", typeof(Brush), typeof(DrawingPage), new PropertyMetadata(default(DrawingPage)));


        /// <summary>
        ///  自定义颜色
        /// </summary>
        public string CustomColor
        {
            get { return (string)GetValue(CustomColorProperty); }
            set { SetValue(CustomColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomColorProperty =
            DependencyProperty.Register("CustomColor", typeof(string), typeof(DrawingPage), new PropertyMetadata("red"));


        public const string TemporalDirName = "_Drawings";


        // 新增方法
        /// <summary>
        /// 添加添加组件
        /// </summary>
        /// <param name="control"></param>
        /// <param name="point"></param>
        public void AddUIElement(FrameworkElement element,Point point)
        {
            AddFrameworkElement2Cavas(element, point);
        }
        /// <summary>
        /// svg容器初始化
        /// </summary>
        public void Initialization()
        {
            var list=new List<FrameworkElement>();
            foreach (FrameworkElement item in this.eastenMain.Children)
            {
                if (item.Name.StartsWith("est"))
                {
                    list.Add(item); 
                }
            }

            list?.ForEach(a =>
            {
                this.eastenMain.Children.Remove(a);
            });

           
        }
        /// <summary>
        /// 移出组件
        /// </summary>
        /// <param name="name"></param>
        public void RemoveUIElement(string name)
        {
            foreach (FrameworkElement item in this.eastenMain.Children)
            {
                if (item.Name == name)
                {
                    this.eastenMain.Children.Remove(item);
                    break;
                }
            }
        }

        /// <summary>
        /// 温度模板样式实体
        /// </summary>
        public ValueTemplateStyle ValueTemplateStyle { get; set; }
        /// <summary>
        /// 更新温度值  
        /// </summary>
        /// <param name="name">组件的name 值</param>
        /// <param name="code">传感器的编码</param>
        /// <param name="value">温度值</param>
        /// <param name="state">温度的状态 是否报警，是否正常等</param>
        /// <returns></returns>
        public Task UpdateValueAsync(string[] name,string code,double value,int state)
        {
            try
            {
               Dispatcher.BeginInvoke(new Action(()=>
               {
                   foreach (FrameworkElement item in this.eastenMain.Children)
                   {
                       // 通过反射获取组件的SetValue 方法
                       if (name.Contains(item.Name))
                       {
                           var methodInfos = item.GetType().GetMethods();
                           Object[] paras = new Object[] { item.Name, value, state };
                           for (int i = 0; i < methodInfos.Length; i++)
                           {
                               var md = methodInfos[i];
                               //方法名
                               string mothodName = md.Name;
                               //参数集合
                               ParameterInfo[] paramInfos = md.GetParameters();
                               //方法名相同且参数个数一样
                               if (mothodName == "SetValue" && paramInfos.Length == paras.Length)
                               {
                                   md.Invoke(item, paras);
                               }
                           }
                       }
                   }
               }));
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 初始化温度数据
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Task InitializationPoint(List<CustomUIElement> elements)
        {
            return Task.Run(() =>
            {
                //elements?.ForEach(a =>
                //{
                //    AddFrameworkElement2Cavas(a,a.Point);
                //});
            });
        }
        /// <summary>
        /// 将元素添加到画布上
        /// </summary>
        /// <param name="element"></param>
        private void AddFrameworkElement2Cavas(FrameworkElement element,Point point)
        {
            element.SetValue(Canvas.LeftProperty, point.X);
            element.SetValue(Canvas.TopProperty, point.Y);
            this.eastenMain.Children.Add(element);
        }
        /// <summary>
        /// 根据路径加载
        /// </summary>
        /// <param name="svgFilePath"></param>
        /// <returns></returns>
        public bool LoadDocument(string svgFilePath)
        {
            // 加载时比较耗时，添加一个进度条
            // <hc:LoadingLine Grid.Row="1" hc:Background="Red"/>
            if (string.IsNullOrWhiteSpace(svgFilePath) || !File.Exists(svgFilePath))
            {
                return false;
            }

            DirectoryInfo workingDir = _workingDir;
            if (_directoryInfo != null)
            {
                workingDir = _directoryInfo;
            }
            _svgFilePath = svgFilePath;

            string fileExt = System.IO.Path.GetExtension(svgFilePath);

            if (string.Equals(fileExt, SvgConverter.SvgExt, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileExt, SvgConverter.CompressedSvgExt, StringComparison.OrdinalIgnoreCase))
            {
                if (_fileReader != null)
                {
                    _fileReader.SaveXaml = _saveXaml;
                    _fileReader.SaveZaml = false;

                    _embeddedImageVisitor.SaveImages = !_wpfSettings.IncludeRuntime;
                    _embeddedImageVisitor.SaveDirectory = _drawingDir;
                    _wpfSettings.Visitors.ImageVisitor = _embeddedImageVisitor;

                    DrawingGroup drawing = _fileReader.Read(svgFilePath, workingDir);
                    _drawingDocument = _fileReader.DrawingDocument;
                    if (drawing != null)
                    {
                        svgViewer.UnloadDiagrams();
                        svgViewer.RenderDiagrams(drawing);

                        Rect bounds = svgViewer.Bounds;

                        if (bounds.IsEmpty)
                        {
                            bounds = new Rect(0, 0, zoomPanControl.ActualWidth, zoomPanControl.ActualHeight);
                        }

                        zoomPanControl.AnimatedZoomTo(bounds);
                        CommandManager.InvalidateRequerySuggested();

                        return true;
                    }
                }
            }
            else if (string.Equals(fileExt, SvgConverter.XamlExt, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileExt, SvgConverter.CompressedXamlExt, StringComparison.OrdinalIgnoreCase))
            {
                svgViewer.LoadDiagrams(svgFilePath);

                svgViewer.InvalidateMeasure();

                return true;
            }
            RemoveUIElement("test");
            _svgFilePath = null;
            return false;
        }
        /// <summary>
        /// 异步加载内容
        /// </summary>
        /// <param name="svgFilePath"></param>
        /// <returns></returns>
        public Task<bool> LoadDocumentAsync(string svgFilePath)
        {
            if (_isLoadingDrawing || string.IsNullOrWhiteSpace(svgFilePath) || !File.Exists(svgFilePath))
            {
#if DOTNET40
                return TaskEx.FromResult<bool>(false);
#else
                return Task.FromResult<bool>(false);
#endif
            }

            string fileExt = System.IO.Path.GetExtension(svgFilePath);

            if (!(string.Equals(fileExt, SvgConverter.SvgExt, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileExt, SvgConverter.CompressedSvgExt, StringComparison.OrdinalIgnoreCase)))
            {
                _svgFilePath = null;
#if DOTNET40
                return TaskEx.FromResult<bool>(false);
#else
                return Task.FromResult<bool>(false);
#endif
            }

            _isLoadingDrawing = true;

            this.UnloadDocument(true);

            DirectoryInfo workingDir = _workingDir;
            if (_directoryInfo != null)
            {
                workingDir = _directoryInfo;
            }

            _svgFilePath = svgFilePath;
            _embeddedImageVisitor.SaveImages = !_wpfSettings.IncludeRuntime;
            _embeddedImageVisitor.SaveDirectory = _drawingDir;
            _wpfSettings.Visitors.ImageVisitor = _embeddedImageVisitor;

            if (_fileReader == null)
            {
                _fileReader = new FileSvgReader(_wpfSettings);
                _fileReader.SaveXaml = _saveXaml;
                _fileReader.SaveZaml = false;
            }

            var drawingStream = new MemoryStream();

            // Get the UI thread's context
            var context = TaskScheduler.FromCurrentSynchronizationContext();

            return Task<bool>.Factory.StartNew(() =>
            {
                //                var saveXaml = _fileReader.SaveXaml;
                //                _fileReader.SaveXaml = true; // For threaded, we will save to avoid loading issue later...

                //Stopwatch stopwatch = new Stopwatch();

                //stopwatch.Start();

                //DrawingGroup drawing = _fileReader.Read(svgFilePath, workingDir);

                //stopwatch.Stop();

                //Trace.WriteLine(string.Format("FileName={0}, Time={1}", 
                //    Path.GetFileName(svgFilePath), stopwatch.ElapsedMilliseconds));

                Stopwatch watch = new Stopwatch();
                watch.Start();

                DrawingGroup drawing = _fileReader.Read(svgFilePath, workingDir);

                watch.Stop();

                Debug.WriteLine("{0}: {1}", System.IO.Path.GetFileName(svgFilePath), watch.ElapsedMilliseconds);

                //                _fileReader.SaveXaml = saveXaml;
                _drawingDocument = _fileReader.DrawingDocument;
                if (drawing != null)
                {
                    XamlWriter.Save(drawing, drawingStream);
                    drawingStream.Seek(0, SeekOrigin.Begin);
                    return true;
                }
                _svgFilePath = null;
                return false;
            }).ContinueWith((t) =>
            {
                try
                {
                    if (!t.Result)
                    {
                        _isLoadingDrawing = false;
                        _svgFilePath = null;
                        return false;
                    }
                    if (drawingStream.Length != 0)
                    {
                        // textEditor.Load(drawingStream);
                        DrawingGroup drawing = (DrawingGroup)XamlReader.Load(drawingStream);
                        svgViewer.UnloadDiagrams();
                        svgViewer.RenderDiagrams(drawing);
                        Rect bounds = svgViewer.Bounds;

                        if (bounds.IsEmpty)
                        {
                            bounds = new Rect(0, 0, svgViewer.ActualWidth, svgViewer.ActualHeight);
                        }

                        zoomPanControl.AnimatedZoomTo(bounds);
                        CommandManager.InvalidateRequerySuggested();

                        // The drawing changed, update the source...
                        _fileReader.Drawing = drawing;
                    }

                    _isLoadingDrawing = false;

                    return true;
                }
                catch
                {
                    _isLoadingDrawing = false;
                    throw;
                }
            }, context);
        }
        /// <summary>
        /// 不加载内容
        /// </summary>
        /// <param name="displayMessage"></param>
        public void UnloadDocument(bool displayMessage = false)
        {
            try
            {
                _svgFilePath = null;
                _drawingDocument = null;

                if (svgViewer != null)
                {
                    // svgViewer.UnloadDiagrams();

                    if (displayMessage)
                    {
                        var drawing = this.DrawText("加载中，请稍后");

                        svgViewer.RenderDiagrams(drawing);

                        Rect bounds = svgViewer.Bounds;
                        if (bounds.IsEmpty)
                        {
                            bounds = drawing.Bounds;
                        }

                        zoomPanControl.ZoomTo(bounds);
                        return;
                    }
                    else
                    {
                        var drawing = this.DrawText("");
                        svgViewer.RenderDiagrams(drawing);
                        return;
                    }
                }

                var drawRect = this.DrawRect();
                svgViewer.RenderDiagrams(drawRect);

                zoomPanControl.ZoomTo(drawRect.Bounds);
                ClearPrevZoomRect();
                ClearNextZoomRect();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (_embeddedImages != null && _embeddedImages.Count != 0)
                {
                    foreach (var embeddedImage in _embeddedImages)
                    {
                        try
                        {
                            if (embeddedImage.Image != null)
                            {
                                if (embeddedImage.Image.StreamSource != null)
                                {
                                    embeddedImage.Image.StreamSource.Dispose();
                                }
                            }

                            var imagePath = embeddedImage.ImagePath;
                            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                            {
                                File.Delete(imagePath);
                            }
                        }
                        catch (IOException ex)
                        {
                            Trace.TraceError(ex.ToString());
                            // Image this, WPF will typically cache and/or lock loaded images
                        }
                    }

                    _embeddedImages.Clear();
                }
            }
        }

        #region private fields
        private const double ZoomChange = 0.1;
        private bool _isLoadingDrawing;
        private bool _saveXaml;
        private string _drawingDir;
        private string _svgFilePath;
        private DirectoryInfo _directoryInfo;
        private FileSvgReader _fileReader;
        private WpfDrawingSettings _wpfSettings;
        private DirectoryInfo _workingDir;
        private DpiScale _dpiScale;
        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private ZoomPanMouseHandlingMode _mouseHandlingMode;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point _origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point _origContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton _mouseButtonDown;

        /// <summary>
        /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
        /// </summary>
        private Rect _prevZoomRect;

        /// <summary>
        /// Save the previous content scale, pressing the backspace key jumps back to this scale.
        /// </summary>
        private double _prevZoomScale;

        /// <summary>
        /// Set to 'true' when the previous zoom rect is saved.
        /// </summary>
        private bool _prevZoomRectSet;

        /// <summary>
        /// Saves the next zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
        /// </summary>
        private Rect _nextZoomRect;

        /// <summary>
        /// Save the next content scale, pressing the backspace key jumps back to this scale.
        /// </summary>
        private double _nextZoomScale;

        /// <summary>
        /// Set to 'true' when the previous zoom rect is saved.
        /// </summary>
        private bool _nextZoomRectSet;
        private Cursor _panToolCursor;
        private Cursor _panToolDownCursor;
        private DispatcherTimer _dispatcherTimer;
        private WpfDrawingDocument _drawingDocument;
        private EmbeddedImageSerializerVisitor _embeddedImageVisitor;
        private IList<EmbeddedImageSerializerArgs> _embeddedImages;
        #endregion


        private SvgViewModel viewerModel;

        public SvgViewModel ViewerModel
        {
            get { return viewerModel; }
            set {
                _mouseHandlingMode = value== SvgViewModel.Edit ? ZoomPanMouseHandlingMode.SelectPoint : ZoomPanMouseHandlingMode.Panning;
                viewerModel = value; 
            }
        }



        /// <summary>
        /// 是否未编辑模式
        /// </summary>
        private bool isEdit;
        /// <summary>
        /// 点击完成后的回调事件
        /// </summary>
        public event EventHandler<SvgElement> ElementSelect;
        public event EventHandler<Point> PointSelectedEvent;
        public event EventHandler<FrameworkElement> ElementUpdate;
        private TextEditor textEditor;
        System.Windows.Media.DrawingVisual _drawingVisual = null;
        [TypeConverter(typeof(BoolConvert))]
        
        // 编辑模式下鼠标移动变为点选。
        public bool IsEdit
        {
            get { return isEdit; }
            set { isEdit = value; }
        }

        #region Private Zoom Panel Handlers

        FrameworkElement targetElement;
        Point targetPoint;
        bool IsDrag = false;// 是否进行了元素拖动
        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void OnZoomPanMouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPanControl.Focus();
            Keyboard.Focus(zoomPanControl);

            _mouseButtonDown = e.ChangedButton;
            _origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomPanControl);
            _origContentMouseDownPoint = e.GetPosition(svgViewer);

            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
                // 选中点或者矩形框  实现拖动
                var selectElement = Mouse.DirectlyOver as FrameworkElement;
                if (selectElement != null&& selectElement.Name.StartsWith("est"))
                {
                    var userControl = VisualTreeHelp.FindParent<UserControl>(selectElement);
                    if(userControl != null)
                    {
                        targetElement = userControl;
                        targetPoint = e.GetPosition(targetElement);
                        targetElement.CaptureMouse();
                    }
                    //if(selectElement is TextBlock)
                    //{
                    //    targetElement = ((selectElement.Parent as FrameworkElement).Parent as FrameworkElement).Parent as FrameworkElement;
                    //    targetPoint = e.GetPosition(targetElement);
                    //    targetElement.CaptureMouse();
                    //}else if(selectElement is Border)
                    //{
                    //    targetElement = (selectElement as FrameworkElement).Parent as FrameworkElement;
                    //    targetPoint = e.GetPosition(targetElement);
                    //    targetElement.CaptureMouse();
                    //}
                }
                else
                {
                    targetElement = null;
                }
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                    (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
                {
                    // Shift + left- or right-down initiates zooming mode.
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.Zooming;

                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}
                }
                else if (_mouseButtonDown == MouseButton.Left)
                {
                    // Just a plain old left-down initiates panning mode.
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.Panning;
                }

                if (_mouseHandlingMode != ZoomPanMouseHandlingMode.None)
                {
                    // Capture the mouse so that we eventually receive the mouse up event.
                    zoomPanControl.CaptureMouse();
                    e.Handled = true;
                }
            }

        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void OnZoomPanMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
                // 添加编辑逻辑 // 编辑模式
                if (this.ViewerModel== SvgViewModel.Edit)
                {
                    if (_drawingDocument != null)
                    {
                        if (_drawingVisual != null)
                        {
                            svgViewer.HostVisual.Children.Remove(_drawingVisual);
                            _drawingVisual = null;
                        }

                        //  var bade = new Badge();
                        var point = e.GetPosition(svgViewer);
                        // 获取点击的点坐标 过滤自定义的内容
                        if (targetElement!=null)
                        {
                            // 提交更新的内容
                            ElementUpdate?.Invoke(point,targetElement);
                            Mouse.Capture(null);
                            targetElement = null;
                        }
                        else
                        {
                            PointSelectedEvent?.Invoke(_drawingDocument, point);
                        }
                        _drawingDocument.DisplayTransform = svgViewer.DisplayTransform;
                        //var hitResult = _drawingDocument.HitTest(point);
                        //if (hitResult != null && hitResult.IsHit)
                        //{
                        //    //this.contentControl1.SetResourceReference(ContentControl .TemplateProperty, "DesignerItemTemplate");
                        //    // 获取到选中的内容, 如果是自定义的元素，就返回，而且要支持拖动
                        //    var selecteElement = hitResult.Element;
                        //    if (selecteElement.Name.StartsWith("est"))
                        //    {
                        //        ElementSelect?.Invoke(hitResult.Point.Value, selecteElement);
                        //    }
                        //}
                       // ElementSelect?.Invoke(point);
                    }
                }
            }
            else
            {
                if (_mouseHandlingMode != ZoomPanMouseHandlingMode.None)
                {
                    if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Zooming)
                    {
                        if (_mouseButtonDown == MouseButton.Left)
                        {
                            // Shift + left-click zooms in on the content.
                            ZoomIn(_origContentMouseDownPoint);
                        }
                        else if (_mouseButtonDown == MouseButton.Right)
                        {
                            // Shift + left-click zooms out from the content.
                            ZoomOut(_origContentMouseDownPoint);
                        }
                    }
                    else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.DragZooming)
                    {
                        // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                        ApplyDragZoomRect();
                    }

                    zoomPanControl.ReleaseMouseCapture();
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.None;
                    e.Handled = true;
                }

                //if (zoomPanControl != null && _canvasCursor != null)
                //{
                //    zoomPanControl.Cursor = _canvasCursor;
                //}
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void OnZoomPanMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
                if (targetElement != null)
                {
                    // 编辑状态下移动
                    //确定鼠标左键处于按下状态并且有元素被选中
                    if (e.LeftButton == MouseButtonState.Pressed && targetElement != null)
                    {
                        var pCanvas = e.GetPosition(this.eastenMain);
                        var xx = pCanvas.X - targetPoint.X;
                        var yy = pCanvas.Y - targetPoint.Y;
                        targetElement.Tag = new Point(xx, yy);
                        Canvas.SetLeft(targetElement, xx);
                        Canvas.SetTop(targetElement,yy );
                    }
                }
            }
            else
            {
                if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Panning)
                {
                    if (zoomPanControl != null)
                    {
                        zoomPanControl.Cursor = _panToolCursor;
                    }

                    //
                    // The user is left-dragging the mouse.
                    // Pan the viewport by the appropriate amount.
                    //
                    Point curContentMousePoint = e.GetPosition(svgViewer);
                    Vector dragOffset = curContentMousePoint - _origContentMouseDownPoint;

                    zoomPanControl.ContentOffsetX -= dragOffset.X;
                    zoomPanControl.ContentOffsetY -= dragOffset.Y;

                    e.Handled = true;
                }
                else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Zooming)
                {
                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}

                    Point curZoomAndPanControlMousePoint = e.GetPosition(zoomPanControl);
                    Vector dragOffset = curZoomAndPanControlMousePoint - _origZoomAndPanControlMouseDownPoint;
                    double dragThreshold = 10;
                    if (_mouseButtonDown == MouseButton.Left &&
                        (Math.Abs(dragOffset.X) > dragThreshold ||
                         Math.Abs(dragOffset.Y) > dragThreshold))
                    {
                        //
                        // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                        // initiate drag zooming mode where the user can drag out a rectangle to select the area
                        // to zoom in on.
                        //
                        _mouseHandlingMode = ZoomPanMouseHandlingMode.DragZooming;

                        Point curContentMousePoint = e.GetPosition(svgViewer);
                        InitDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);
                    }

                    e.Handled = true;
                }
                else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.DragZooming)
                {
                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}

                    //
                    // When in drag zooming mode continously update the position of the rectangle
                    // that the user is dragging out.
                    //
                    Point curContentMousePoint = e.GetPosition(svgViewer);
                    SetDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        private void OnZoomPanMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            Point curContentMousePoint = e.GetPosition(svgViewer);
            //if (e.Delta > 0)
            //{
            //    ZoomIn(curContentMousePoint);
            //}
            //else if (e.Delta < 0)
            //{
            //    ZoomOut(curContentMousePoint);
            //}
            this.Zoom(curContentMousePoint, e.Delta);

            if (svgViewer.IsKeyboardFocusWithin)
            {
                Keyboard.Focus(zoomPanControl);
            }
        }

        /// <summary>
        /// Event raised by double-left click
        /// </summary>
        private void OnZoomPanMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                SavePrevZoomRect();

                zoomPanControl.AnimatedSnapTo(e.GetPosition(svgViewer));

                ClearNextZoomRect();

                e.Handled = true;
            }
        }

        /// <summary>
        /// The 'Pan' command (bound to the plus key) was executed.
        /// </summary>
        private void OnPanMode(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Determines whether the 'Pan' command can be executed.
        /// </summary>
        private void OnCanPanMode(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        /// <summary>
        /// The 'ZoomReset' command (bound to the plus key) was executed.
        /// </summary>
        private void OnZoomReset(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomPanControl.AnimatedZoomTo(1.0);

            ClearNextZoomRect();
        }

        /// <summary>
        /// Determines whether the 'ZoomReset' command can be executed.
        /// </summary>
        private void OnCanZoomReset(object sender, CanExecuteRoutedEventArgs e)
        {
            if (zoomPanControl == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = !zoomPanControl.ContentScale.Equals(1.0);
        }

        /// <summary>
        /// The 'ZoomFit/Fill' command (bound to the plus key) was executed.
        /// </summary>
        private void OnZoomFit(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            //zoomPanControl.AnimatedScaleToFit();
            zoomPanControl.AnimatedZoomTo(this.FitZoomValue);

            ClearNextZoomRect();
        }

        /// <summary>
        /// Determines whether the 'ZoomFit' command can be executed.
        /// </summary>
        private void OnCanZoomFit(object sender, CanExecuteRoutedEventArgs e)
        {
            if (zoomPanControl == null)
            {
                e.CanExecute = false;
                return;
            }

            var fitValue = this.FitZoomValue;

            e.CanExecute = !IsWithinOnePercent(zoomPanControl.ContentScale, fitValue)
                && fitValue >= zoomPanControl.MinContentScale;
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void OnZoomIn(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            ZoomIn(new Point(zoomPanControl.ContentZoomFocusX, zoomPanControl.ContentZoomFocusY));

            ClearNextZoomRect();
        }

        /// <summary>
        /// Determines whether the 'ZoomIn' command can be executed.
        /// </summary>
        private void OnCanZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            if (zoomPanControl == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = zoomPanControl.ContentScale < zoomPanControl.MaxContentScale;
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void OnZoomOut(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            ZoomOut(new Point(zoomPanControl.ContentZoomFocusX, zoomPanControl.ContentZoomFocusY));

            ClearNextZoomRect();
        }

        /// <summary>
        /// Determines whether the 'UndoZoom' command can be executed.
        /// </summary>
        private void OnCanZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            if (zoomPanControl == null)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = zoomPanControl.ContentScale > zoomPanControl.MinContentScale;
        }

        /// <summary>
        /// The 'UndoZoom' command was executed.
        /// </summary>
        private void OnUndoZoom(object sender, ExecutedRoutedEventArgs e)
        {
            UndoZoom();
        }

        /// <summary>
        /// Determines whether the 'UndoZoom' command can be executed.
        /// </summary>
        private void OnCanUndoZoom(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _prevZoomRectSet;
        }

        /// <summary>
        /// The 'RedoZoom' command was executed.
        /// </summary>
        private void OnRedoZoom(object sender, ExecutedRoutedEventArgs e)
        {
            RedoZoom();
        }

        /// <summary>
        /// Determines whether the 'RedoZoom' command can be executed.
        /// </summary>
        private void OnCanRedoZoom(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _nextZoomRectSet;
        }

        /// <summary>
        /// Jump back to the previous zoom level.
        /// </summary>
        private void UndoZoom()
        {
            SaveNextZoomRect();

            zoomPanControl.AnimatedZoomTo(_prevZoomScale, _prevZoomRect);

            ClearPrevZoomRect();
        }

        /// <summary>
        /// Jump back to the next zoom level.
        /// </summary>
        private void RedoZoom()
        {
            SavePrevZoomRect();

            zoomPanControl.AnimatedZoomTo(_nextZoomScale, _nextZoomRect);

            ClearNextZoomRect();
        }

        private void Zoom(Point contentZoomCenter, int wheelMouseDelta)
        {
            SavePrevZoomRect();

            // Found the division by 3 gives a little smoothing effect
            var zoomFactor = zoomPanControl.ContentScale + ZoomChange * wheelMouseDelta / (120 * 3);

            zoomPanControl.ZoomAboutPoint(zoomFactor, contentZoomCenter);

            ClearNextZoomRect();
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            SavePrevZoomRect();

            zoomPanControl.ZoomAboutPoint(zoomPanControl.ContentScale - ZoomChange, contentZoomCenter);

            ClearNextZoomRect();
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            SavePrevZoomRect();

            zoomPanControl.ZoomAboutPoint(zoomPanControl.ContentScale + ZoomChange, contentZoomCenter);

            ClearNextZoomRect();
        }

        /// <summary>
        /// Initialise the rectangle that the use is dragging out.
        /// </summary>
        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }

        /// <summary>
        /// Update the position and size of the rectangle that user is dragging out.
        /// </summary>
        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Deterine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

        /// <summary>
        /// When the user has finished dragging out the rectangle the zoom operation is applied.
        /// </summary>
        private void ApplyDragZoomRect()
        {
            //
            // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
            //
            SavePrevZoomRect();

            //
            // Retreive the rectangle that the user draggged out and zoom in on it.
            //
            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            zoomPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

            FadeOutDragZoomRect();

            ClearNextZoomRect();
        }

        //
        // Fade out the drag zoom rectangle.
        //
        private void FadeOutDragZoomRect()
        {
            ZoomPanAnimationHelper.StartAnimation(dragZoomBorder, OpacityProperty, 0.0, ZoomChange,
                delegate (object sender, EventArgs e)
                {
                    dragZoomCanvas.Visibility = Visibility.Collapsed;
                });
        }

        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SavePrevZoomRect()
        {
            _prevZoomRect = new Rect(zoomPanControl.ContentOffsetX, zoomPanControl.ContentOffsetY,
                zoomPanControl.ContentViewportWidth, zoomPanControl.ContentViewportHeight);
            _prevZoomScale = zoomPanControl.ContentScale;
            _prevZoomRectSet = true;
        }

        //
        // Record the next zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SaveNextZoomRect()
        {
            _nextZoomRect = new Rect(zoomPanControl.ContentOffsetX, zoomPanControl.ContentOffsetY,
                zoomPanControl.ContentViewportWidth, zoomPanControl.ContentViewportHeight);
            _nextZoomScale = zoomPanControl.ContentScale;
            _nextZoomRectSet = true;
        }

        /// <summary>
        /// Clear the memory of the previous zoom level.
        /// </summary>
        private void ClearPrevZoomRect()
        {
            _prevZoomRectSet = false;
        }

        /// <summary>
        /// Clear the memory of the next zoom level.
        /// </summary>
        private void ClearNextZoomRect()
        {
            _nextZoomRectSet = false;
        }

        public double FitZoomValue
        {
            get
            {
                if (zoomPanControl == null)
                {
                    return 1;
                }

                var content = zoomPanControl.ContentElement;

                return FitZoom(ActualWidth, ActualHeight, content?.ActualWidth, content?.ActualHeight);
            }
        }

        private static bool IsWithinOnePercent(double value, double testValue)
        {
            return Math.Abs(value - testValue) < .01 * testValue;
        }

        private static double FitZoom(double actualWidth, double actualHeight, double? contentWidth, double? contentHeight)
        {
            if (!contentWidth.HasValue || !contentHeight.HasValue) return 1;
            return Math.Min(actualWidth / contentWidth.Value, actualHeight / contentHeight.Value);
        }

        #endregion

        #region Constructors and Destructor


        private void OnEmbeddedImageCreated(object sender, EmbeddedImageSerializerArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (_embeddedImages == null)
            {
                _embeddedImages = new List<EmbeddedImageSerializerArgs>();
            }
            _embeddedImages.Add(args);
        }

        #endregion

        #region Public Properties
       

        public void ChangeTemperature(string name, decimal value)
        {
            foreach (FrameworkElement item in this.eastenMain.Children)
            {
                if (item.Name == name)
                {
                    //
                    break;
                }
            }
        }


        public bool IsLoadingDrawing
        {
            get
            {
                return _isLoadingDrawing;
            }
        }

        public string WorkingDrawingDir
        {
            get
            {
                return _drawingDir;
            }
            set
            {
                _drawingDir = value;

                if (!string.IsNullOrWhiteSpace(_drawingDir))
                {
                    _directoryInfo = new DirectoryInfo(_drawingDir);

                    if (_fileReader != null)
                    {
                        _fileReader.SaveXaml = Directory.Exists(_drawingDir);
                    }
                }
            }
        }

        public bool SaveXaml
        {
            get
            {
                return _saveXaml;
            }
            set
            {
                _saveXaml = value;
                if (_fileReader != null)
                {
                    _fileReader.SaveXaml = _saveXaml;
                    _fileReader.SaveZaml = false;
                }
            }
        }

        public WpfDrawingSettings ConversionSettings
        {
            get
            {
                return _wpfSettings;
            }
            set
            {
                if (value != null)
                {
                    _wpfSettings = value;

                    // Recreated the conveter
                    _fileReader = new FileSvgReader(_wpfSettings);
                    _fileReader.SaveXaml = true;
                    _fileReader.SaveZaml = false;

                    if (!string.IsNullOrWhiteSpace(_drawingDir) &&
                        Directory.Exists(_drawingDir))
                    {
                        _fileReader.SaveXaml = Directory.Exists(_drawingDir);
                    }
                }
            }
        }
        //
        // Definitions for dependency properties.
        //
        /// <summary>
        /// This allows the same property name to be used for direct and indirect access to the ZoomPanelControl control.
        /// </summary>
        public ZoomPanControl ZoomPanContent
        {
            get
            {
                return zoomPanControl;
            }
        }

        /// <summary>
        /// This allows the same property name to be used for direct and indirect access to the SVG Canvas control.
        /// </summary>
        public SvgDrawingCanvas Viewer
        {
            get
            {
                return svgViewer;
            }
        }

        #endregion

        #region Public Methods
      

        public bool SaveDocument(string fileName)
        {

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (_fileReader == null || _fileReader.Drawing == null)
            {
                return false;
            }
            return _fileReader.Save(fileName, true, false);
        }

        public void PageSelected(bool isSelected)
        {
            if (isSelected)
            {
                svgViewer.Focus();

                if (zoomPanControl.IsKeyboardFocusWithin)
                {
                    Keyboard.Focus(zoomPanControl);
                }
            }
        }

        public void SaveZoom()
        {
            if (zoomPanControl != null)
            {
                SavePrevZoomRect();

                ClearNextZoomRect();
            }
        }

        /// <summary>
        /// 更新节点id
        /// </summary>
        /// <param name="svgElement"></param>
        /// <param name="id"></param>
        public void UpdateDrawingId(SvgElement svg, string id)
        {
            if (_drawingDocument != null)
            {
                var outerXml = svg.OuterXml;
                _drawingDocument.GetSvgByUniqueId(svg.UniqueId).Id = id;
                var res = XmlReader.Create(_svgFilePath);
                _fileReader.Drawing = _drawingDocument.Drawing;
                var stribuild = new StringBuilder();
                svgViewer.RenderDiagrams(_fileReader.DrawingDocument.Drawing);
                textEditor.Load(_svgFilePath);
                if (!string.IsNullOrEmpty(outerXml))
                {
                    var alltext = textEditor.Document.Text;
                    var rp1 = GetXmlString(outerXml, svg.UniqueId);
                    if (alltext.Contains(rp1))
                    {
                        var newTest = _drawingDocument.GetSvgByUniqueId(svg.UniqueId).OuterXml;
                        var rp2 = GetXmlString(newTest, svg.UniqueId);
                        textEditor.Document.Text = alltext.Replace(rp1, rp2);
                        textEditor.Save(_svgFilePath);
                    }
                }
            }

        }
        private string GetXmlString(string xml, string unid)
        {
            return xml.Replace(@" xmlns=""http://www.w3.org/2000/svg""", "")
                         .Replace(@$" uniqueId=""{unid}""", "");
        }
        #endregion

        #region Protected Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        #endregion

        #region Private Event Handlers (Page)

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            textEditor = new TextEditor();

            if (string.IsNullOrWhiteSpace(_svgFilePath) || !File.Exists(_svgFilePath))
            {
                zoomPanControl.ContentScale = 1.0;

                if (zoomPanControl != null)
                {
                    zoomPanControl.IsMouseWheelScrollingEnabled = true;
                }

                if (string.IsNullOrWhiteSpace(_svgFilePath))
                {
                    this.UnloadDocument();
                }
            }

            try
            {
                if (_panToolCursor == null)
                {
                    var panToolStream = Application.GetResourceStream(new Uri("Resources/PanTool.cur", UriKind.Relative));
                    using (panToolStream.Stream)
                    {
                        _panToolCursor = new Cursor(panToolStream.Stream);
                    }
                }
                if (_panToolDownCursor == null)
                {
                    var panToolDownStream = Application.GetResourceStream(new Uri("Resources/PanToolDown.cur", UriKind.Relative));
                    using (panToolDownStream.Stream)
                    {
                        _panToolDownCursor = new Cursor(panToolDownStream.Stream);
                    }
                }

                //  DispatcherTimer setup
                if (_dispatcherTimer == null)
                {
                    _dispatcherTimer = new DispatcherTimer();
                    _dispatcherTimer.Tick += OnUpdateUITick;
                    _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            if (zoomPanControl != null && zoomPanControl.ScrollOwner == null)
            {
                if (canvasScroller != null)
                {
                    zoomPanControl.ScrollOwner = canvasScroller;
                }
            }

            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Start();
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
            }
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (zoomPanControl != null && svgViewer != null)
            {
                svgViewer.InvalidateMeasure();
                svgViewer.UpdateLayout();

                Rect bounds = svgViewer.Bounds;

                if (bounds.IsEmpty)
                {
                    bounds = new Rect(0, 0, svgViewer.ActualWidth, svgViewer.ActualHeight);
                }

                //zoomPanControl.AnimatedZoomTo(bounds);
                zoomPanControl.AnimatedZoomTo(this.FitZoomValue);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Updates the current seconds display and calls InvalidateRequerySuggested on the 
        /// CommandManager to force the Command to raise the CanExecuteChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateUITick(object sender, EventArgs e)
        {
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Private Methods

        private DrawingGroup DrawRect()
        {
            // Create a new DrawingGroup of the control.
            DrawingGroup drawingGroup = new DrawingGroup();

            // Open the DrawingGroup in order to access the DrawingContext.
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 280, 300));
            }
            // Return the updated DrawingGroup content to be used by the control.
            return drawingGroup;
        }

        // Convert the text string to a geometry and draw it to the control's DrawingContext.
        private DrawingGroup DrawText(string textString)
        {
            // Create a new DrawingGroup of the control.
            DrawingGroup drawingGroup = new DrawingGroup();

            drawingGroup.Opacity = 0.8;

            if (_dpiScale == null)
            {
                _dpiScale = DpiUtilities.GetWindowScale(this);
            }

            // Open the DrawingGroup in order to access the DrawingContext.
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                // Create the formatted text based on the properties set.
                FormattedText formattedText = null;
#if DOTNET40 || DOTNET45 || DOTNET46
                formattedText = new FormattedText(textString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, 
                    FontWeights.Normal, FontStretches.Normal), 72, Brushes.Black);
#else
                formattedText = new FormattedText(textString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Tahoma"), FontStyles.Normal,
                    FontWeights.Normal, FontStretches.Normal), 72, Brushes.Black, _dpiScale.PixelsPerDip);
#endif

                // Build the geometry object that represents the text.
                Geometry textGeometry = formattedText.BuildGeometry(new Point(20, 0));

                drawingContext.DrawRoundedRectangle(Brushes.Transparent, null,
                    new Rect(new Size(formattedText.Width + 50, formattedText.Height + 5)), 5.0, 5.0);

                // Draw the outline based on the properties that are set.
                drawingContext.DrawGeometry(null, new Pen(Brushes.DarkGray, 1.5), textGeometry);
            }

            // Return the updated DrawingGroup content to be used by the control.
            return drawingGroup;
        }

        #endregion

    }

    public class BoolConvert : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return (bool)value;
        }
    }
}
