using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
using HelixToolkit.Wpf.SharpDX.Animations;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX.Model;
using System.Windows;
using System.Windows.Controls;

namespace TestForDIByAlexP
{
    public class TestTask
    {
        public string Name { get; set; }
        public UserControl PanelView { get; set; }
    }

    class ViewModel : BaseViewModel
    {
        public ObservableCollection<TestTask> TasksNames { get; set; }

        public Vector3D UpDirection { set; get; } = new Vector3D(0, 1, 0);
        public Color AmbientLightColor { get; private set; }
        public Color DirectionalLightColor { get; private set; }

        private bool _isLoading = false;
        public bool IsLoading
        {
            private set => SetValue(ref _isLoading, value);
            get => _isLoading;
        }

        public ObservableCollection<Animation> Animations { get; } = new ObservableCollection<Animation>();
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

        public ICommand OpenFileCommand { get; set; }
        public ICommand ClearSceneCommand { get; set; }

        private HelixToolkitScene scene;

        #region TestGround

        public MeshGeometry3D Model { get; private set; }
        public PhongMaterial RedMaterial { get; private set; }
        public Transform3D Model1Transform { get; private set; }
        public LineGeometry3D Lines { get; private set; }

        private string _bounds;

        private void FillTestGround()
        {
            // scene model3d
            var b1 = new MeshBuilder();
            b1.AddSphere(new Vector3(0, 0, 0), 100);
            //b1.AddBox(new Vector3(0, 0, 0), 1, 0.5, 2, BoxFaces.All);

            var meshGeometry = b1.ToMeshGeometry3D();
            meshGeometry.Colors = new Color4Collection(meshGeometry.TextureCoordinates.Select(x => x.ToColor4()));
            Model = meshGeometry;

            // model trafos
            Model1Transform = new Media3D.TranslateTransform3D(0, 0, 0);

            // model materials
            RedMaterial = PhongMaterials.Red;

            // Bounding box
            LineGeometry3D bbox = LineBuilder.GenerateBoundingBox(Model);
            //Lines = bbox;
            Lines = LineBuilder.GenerateBoundingBox(new[] { new Vector3(100, 100, 0), new Vector3(0, 0, 0) });
        }

        #endregion

        public ViewModel()
        {
            var pw34 = new Tasks3and4PanelView();
            var pw5 = new Task5PanelView();
            TasksNames = new ObservableCollection<TestTask>() {
            new TestTask(){Name = "Task 3", PanelView = pw34 },
            new TestTask(){Name = "Task 4", PanelView = pw34 },
            new TestTask(){Name = "Task 5", PanelView = pw5 }
            };

            EffectsManager = new DefaultEffectsManager();

            // camera setup
            Camera = new PerspectiveCamera()
            {
                Position = new Point3D(300, 300, 300),
                LookDirection = new Vector3D(-3, -3, -5),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000
            };
            

            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;

            OpenFileCommand = new DelegateCommand(OpenFile);
            //ClearSceneCommand = new DelegateCommand(() => GroupModel.Clear());
            ClearSceneCommand = new DelegateCommand(() => MessageBox.Show(_bounds));

            // Test ground
            FillTestGround();
        }

        private void OpenFile()
        {
            if (_isLoading)
            {
                return;
            }
            string path = OpenFileDialog("Wavefront .obj file | *.obj");
            if (path == null)
            {
                return;
            }
            // StopAnimation();

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

                    if (scene == null)
                    {
                        MessageBox.Show("Scene == null");
                        return;
                    }

                    if (scene.Root == null)
                    {
                        MessageBox.Show("Scene.Root == null");
                        return;
                    }
                    
                    Animations.Clear();
                    GroupModel.Clear();
                        GroupModel.AddNode(scene.Root);
                        //BBox = LineBuilder.GenerateBoundingBox(scene.Root.Items[0].Bounds);

                    _bounds = string.Format("Original bounds (center, size):  {0}, {1}\n", 
                        scene.Root.Bounds.Center, scene.Root.OriginalBounds.Size);

                    foreach (var node in scene.Root.Traverse())
                    {
                        _bounds += string.Format("Node is: {0}\n", node.ToString());

                        var n = node as GeometryNode;
                        if (n == null)
                            continue;

                        _bounds += string.Format("Original bounds (center, size):  {0}, {1}\n",
                        n.Bounds.Center, n.Bounds.Size);

                        BBox = LineBuilder.GenerateBoundingBox(n.Geometry);
                        
                    }
                }
                else if (result.IsFaulted && result.Exception != null)
                {
                    MessageBox.Show(result.Exception.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
