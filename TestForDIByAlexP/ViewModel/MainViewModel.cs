using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Media3D = System.Windows.Media.Media3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using Vector3 = SharpDX.Vector3;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using HelixToolkit.Wpf.SharpDX.Assimp;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using System.Windows;
using System.Windows.Threading;

namespace TestForDIByAlexP.ViewModel
{
    public class TestTask : ObservableObject
    {
        public string Name { get; set; }
        public ICommand Command { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            set
            {
                SetValue(ref _isActive, value, nameof(IsActive));
            }
            get => _isActive;
        }
    }

    class MainViewModel : BaseViewModel
    {
        public List<TestTask> Exercizes { get; set; }

        private string _currEx;
        public string CurrentExercise
        {
            private set => SetValue(ref _currEx, value, nameof(CurrentExercise));
            get => _currEx;
        }

        private bool _isAllBBoxes;
        public bool IsAllBBoxesVisible
        {
            set
            {
                SetValue(ref _isAllBBoxes, value, nameof(IsAllBBoxesVisible));
            }
            get => _isAllBBoxes;
        }

        private bool _isMainBBox;
        public bool IsMainBBoxVisible
        {
            set => SetValue(ref _isMainBBox, value, nameof(IsMainBBoxVisible));
            get => _isMainBBox;
        }

        private double _minZ = -5;
        public double MinZRotation
        {
            set => SetValue(ref _minZ, value, nameof(MinZRotation));
            get => _minZ;
        }

        private double _maxZ = 5;
        public double MaxZRotation
        {
            set => SetValue(ref _maxZ, value, nameof(MaxZRotation));
            get => _maxZ;
        }


        private bool _isLoading = false;
        public bool IsLoading
        {
            private set => SetValue(ref _isLoading, value);
            get => _isLoading;
        }

        public SceneNodeGroupModel3D GroupModel { get; } = new SceneNodeGroupModel3D();

        private LineGeometry3D _bbox;
        public LineGeometry3D BBox
        {
            get => _bbox;
            set
            {
                _bbox = value;
                OnPropertyChanged(nameof(BBox));
            }
        }

        private LineGeometry3D _bboxAxes;
        public LineGeometry3D BBoxAxes
        {
            get => _bboxAxes;
            set
            {
                _bboxAxes = value;
                OnPropertyChanged(nameof(BBoxAxes));
            }
        }

        private MeshGeometry3D _bigBoundBox;
        public MeshGeometry3D BigBoundBox
        {
            get => _bigBoundBox;
            set
            {
                _bigBoundBox = value;
                OnPropertyChanged(nameof(BigBoundBox));
            }
        }

        private LineGeometry3D _smBBoxes;
        public LineGeometry3D SmallBBoxes
        {
            get => _smBBoxes;
            set
            {
                _smBBoxes = value;
                OnPropertyChanged(nameof(SmallBBoxes));
            }
        }

        public PBRMaterial BoxMaterial { get; private set; } = new PBRMaterial()
        {
            AlbedoColor = new SharpDX.Color4(0, 0, 0, 0.5f),
        };

        public ICommand OpenFileCommand { get; set; }
        public ICommand ClearSceneCommand { get; set; }
        public ICommand StartAnimationCommand { get; set; }
        public ICommand StopAnimationCommand { get; set; }
        
        public Vector3D UpDirection { set; get; } = new Vector3D(0, 1, 0);
        public Color AmbientLightColor { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Transform3D ModelTransform { get; private set; }
        public Media3D.AxisAngleRotation3D Rotation { get; set; }

        private DispatcherTimer timer;
        private double angleStep = 0.5;

        private HelixToolkitScene scene;

        public MainViewModel()
        {
            Exercizes = new List<TestTask>() {
            new TestTask(){Name = "Task 3", IsActive = true,
                Command = new DelegateCommand(() =>{
                    IsMainBBoxVisible = false;
                    IsAllBBoxesVisible = false;
                    timer.Stop();
                    CurrentExercise = "Task 3";
                    Exercizes.ForEach((e) => e.IsActive = false);
                    Exercizes[0].IsActive = true;
                }) },
            new TestTask(){Name = "Task 4", IsActive = false,
                Command = new DelegateCommand(() =>{
                    IsMainBBoxVisible = true;
                    timer.Stop();
                    CurrentExercise = "Task 4";
                    Exercizes.ForEach((e) => e.IsActive = false);
                    Exercizes[1].IsActive = true;
                }) },
            new TestTask(){Name = "Task 5", IsActive = false,
                Command = new DelegateCommand(() =>{
                    IsMainBBoxVisible = false;
                    IsAllBBoxesVisible = false;
                    CurrentExercise = "Task 5";
                    Exercizes.ForEach((e) => e.IsActive = false);
                    Exercizes[2].IsActive = true;
            }) }
            };
            CurrentExercise = "Task 3";

            EffectsManager = new DefaultEffectsManager();

            Camera = new PerspectiveCamera()
            {
                Position = new Point3D(300, 300, 300),
                LookDirection = new Vector3D(-3, -3, -5),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000
            };


            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;

            Rotation = new Media3D.AxisAngleRotation3D(UpDirection, 0);
            ModelTransform = new Media3D.RotateTransform3D(Rotation);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            
            timer.Tick += (o, e) =>
            {
                Rotation.Angle += angleStep;

                if (Rotation.Angle > MaxZRotation)
                    angleStep = -1 * Math.Abs(angleStep);

                if (Rotation.Angle < MinZRotation)
                    angleStep = Math.Abs(angleStep);
            };

            OpenFileCommand = new DelegateCommand(OpenFile);
            ClearSceneCommand = new DelegateCommand(ClearScene);
            StartAnimationCommand = new DelegateCommand(() => timer.Start());
            StopAnimationCommand = new DelegateCommand(() => timer.Stop());
        }

        private void ClearScene()
        {
            GroupModel.Clear();
            SmallBBoxes = null;
            BBox = null;
            BigBoundBox = null;
            BBoxAxes = null;
            Rotation.Angle = 0;
            timer.Stop();
        }

        private void OpenFile()
        {
            if (_isLoading)
                return;

            string path = OpenFileDialog("Wavefront .obj file | *.obj");
            if (path == null)
            {
                return;
            }

            IsLoading = true;
            Task.Run(() =>
            {
                var loader = new Importer();
                return loader.Load(path);

            }).ContinueWith((result) =>
            {
                IsLoading = false;
                if (result.IsCompleted)
                {
                    scene = result.Result;

                    if (scene == null || scene.Root == null)
                    {
                        MessageBox.Show("Empty scene!");
                        return;
                    }

                    ClearScene();
                    GroupModel.AddNode(scene.Root);

                    var _bBoxesList = new List<LineGeometry3D>();

                    foreach (var node in scene.Root.Traverse())
                    {
                        var n = node as GeometryNode;
                        if (n == null)
                            continue;

                        var bb = LineBuilder.GenerateBoundingBox(n.Geometry);
                        _bBoxesList.Add(bb);
                    }

                    BuildBigBBox(_bBoxesList);
                    if (_bBoxesList.Count > 1)
                        BuildSmallBBoxes(_bBoxesList);
                }
                else if (result.IsFaulted && result.Exception != null)
                {
                    MessageBox.Show(result.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BuildSmallBBoxes(List<LineGeometry3D> _bBoxesList)
        {
            var lb = new LineBuilder();

            foreach (LineGeometry3D bb in _bBoxesList)
            {
                lb.AddBox(bb.Bound.Center, bb.Bound.Width, bb.Bound.Height, bb.Bound.Depth);
            }

            SmallBBoxes = lb.ToLineGeometry3D();
        }

        private void BuildBigBBox(List<LineGeometry3D> _bBoxesList)
        {
            Vector3 min = _bBoxesList[0].Bound.Minimum;
            Vector3 max = _bBoxesList[0].Bound.Maximum;

            foreach (LineGeometry3D bb in _bBoxesList)
            {
                min.X = bb.Bound.Minimum.X < min.X ? bb.Bound.Minimum.X : min.X;
                min.Y = bb.Bound.Minimum.Y < min.Y ? bb.Bound.Minimum.Y : min.Y;
                min.Z = bb.Bound.Minimum.Z < min.Z ? bb.Bound.Minimum.Z : min.Z;

                max.X = bb.Bound.Maximum.X > max.X ? bb.Bound.Maximum.X : max.X;
                max.Y = bb.Bound.Maximum.Y > max.Y ? bb.Bound.Maximum.Y : max.Y;
                max.Z = bb.Bound.Maximum.Z > max.Z ? bb.Bound.Maximum.Z : max.Z;
            }

            BBox = LineBuilder.GenerateBoundingBox(new[] { min, max });

            var lb = new LineBuilder();
            float axln = (float)Math.Sqrt(Math.Pow(max.X - min.X, 2) + Math.Pow(max.Y - min.Y, 2) + Math.Pow(max.Z - min.Z, 2)) / 3;

            lb.AddLine(new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, max.Z),
                new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, max.Z + axln));
            lb.AddLine(new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, min.Z),
                new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, min.Z - axln));

            lb.AddLine(new Vector3((min.X + max.X) / 2, max.Y, (min.Z + max.Z) / 2),
                new Vector3((min.X + max.X) / 2, max.Y + axln, (min.Z + max.Z) / 2));
            lb.AddLine(new Vector3((min.X + max.X) / 2, min.Y, (min.Z + max.Z) / 2),
                new Vector3((min.X + max.X) / 2, min.Y - axln, (min.Z + max.Z) / 2));

            lb.AddLine(new Vector3(max.X, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2),
                new Vector3(max.X + axln, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2));
            lb.AddLine(new Vector3(min.X, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2),
                new Vector3(min.X - axln, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2));

            BBoxAxes = lb.ToLineGeometry3D();

            var mb = new MeshBuilder();
            mb.AddBox(BBox.Bound.Center, BBox.Bound.Width, BBox.Bound.Height, BBox.Bound.Depth);
            BigBoundBox = mb.ToMeshGeometry3D();
        }

        private string OpenFileDialog(string filter)
        {
            var d = new OpenFileDialog();
            d.CustomPlaces.Clear();

            d.Filter = filter;

            if (!d.ShowDialog().Value)
            {
                return null;
            }

            return d.FileName;
        }
    }
}
