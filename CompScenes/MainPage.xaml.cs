using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition.Scenes;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.DirectX;
using System.ComponentModel;
using Windows.Media.Playback;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CompScenes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void compPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new();
            picker.FileTypeFilter.Add(".obj");

            if (await picker.PickSingleFileAsync() is StorageFile file)
            {
                var textContent = await FileIO.ReadTextAsync(file);
                var attributes = BasicObjReader.ReadFromString(textContent, new(0, 0, -25.4f) /* Assets/ExampleModel.obj is not centered on the Z axis, center it */);
                //var attributes = GenerateSmoothSphereAttributes(25, 128, 128);

                Compositor compositor = Window.Current.Compositor;

                var sceneNode = SceneNode.Create(compositor);

                var sceneVisual = SceneVisual.Create(compositor);
                sceneVisual.Root = sceneNode;
                sceneVisual.Size = new(255);
                sceneVisual.RelativeOffsetAdjustment = new(0.5f, 0.5f, 0.0f);

                var mesh = SceneMesh.Create(compositor);

                mesh.PrimitiveTopology = DirectXPrimitiveTopology.TriangleList;

                mesh.FillMeshAttribute(
                    SceneAttributeSemantic.Vertex,
                    DirectXPixelFormat.R32G32B32Float,
                    attributes.Vertices.ToMemoryBuffer());

                mesh.FillMeshAttribute(
                    SceneAttributeSemantic.Index,
                    DirectXPixelFormat.R16UInt,
                    attributes.Indices.ToMemoryBuffer());

                mesh.FillMeshAttribute(
                    SceneAttributeSemantic.TexCoord0,
                    DirectXPixelFormat.R32G32Float,
                    attributes.UV.ToMemoryBuffer());

                var material = SceneMetallicRoughnessMaterial.Create(compositor);
                material.BaseColorFactor = new Vector4(1.0f);
                //material.IsDoubleSided = true; // Uncomment when using GenerateSmoothSphereAttributes

                /*MediaPlayer player = new();
                player.AutoPlay = true;
                player.Source = MediaSource.CreateFromUri(new("https://archive.org/download/Rick_Astley_Never_Gonna_Give_You_Up/Rick_Astley_Never_Gonna_Give_You_Up.mp4"));
                player.SetSurfaceSize(new(1080, 720));

                material.BaseColorInput = player.GetSurface(compositor).CompositionSurface.ToMaterialInput(compositor, new(1080, 720));*/

                ElementCompositionPreview.SetIsTranslationEnabled(webview, true);
                material.BaseColorInput = (await ElementCompositionPreview.GetElementVisual(webview).ToVisualSurfaceAsync()).ToMaterialInput(compositor, new(1080, 720), true);

                var meshRendererComponent = SceneMeshRendererComponent.Create(compositor);
                meshRendererComponent.Mesh = mesh;
                meshRendererComponent.Material = material;

                sceneNode.Components.Add(meshRendererComponent);
                sceneNode.Transform.Scale = new(8);
                sceneNode.Transform.RotationAxis = new(0, 1, 0);
                sceneNode.Transform.Orientation = Quaternion.CreateFromAxisAngle(new(1, 0, 0), 1.5f);

                var anim = compositor.CreateScalarKeyFrameAnimation();
                anim.Duration = TimeSpan.FromSeconds(4);
                anim.StopBehavior = AnimationStopBehavior.SetToInitialValue;
                anim.IterationBehavior = AnimationIterationBehavior.Forever;
                anim.InsertKeyFrame(1.0f, 360f, compositor.CreateLinearEasingFunction());

                sceneNode.Transform.StartAnimation(nameof(SceneModelTransform.RotationAngleInDegrees), anim);

                ElementCompositionPreview.SetElementChildVisual(compPresenter, sceneVisual);
            }
        }

        // TODO: Improve UV calculation
        private (Vector3[] Vertices, uint[] Indices, Vector2[] UV) GenerateSmoothSphereAttributes(int radius, int latitudes = 32, int longitudes = 32)
        {
            int bufferLength = (longitudes + 1) * (latitudes + 1);
            Vector3[] vertices = new Vector3[bufferLength];
            Vector2[] uvs = new Vector2[bufferLength];
            uint[] indices = new uint[longitudes * latitudes * 3 * 2];

            float latSpacing = 1.0f / (float)latitudes;
            float lonSpacing = 1.0f / (float)longitudes;

            for (int lat = 0; lat <= latitudes; ++lat)
            {
                float theta = lat * MathF.PI / latitudes;
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int lon = 0; lon <= longitudes; ++lon)
                {
                    int index = lat * (longitudes + 1) + lon;

                    float phi = lon * 2 * MathF.PI / longitudes;
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    Vector3 vertex;
                    vertex.X = radius * x;
                    vertex.Y = radius * y;
                    vertex.Z = radius * z;

                    vertices[index] = vertex;

                    Vector2 uv;
                    uv.X = lon * lonSpacing;
                    uv.Y = 1.0f - ((float)lat + 1) * latSpacing;

                    uvs[index] = uv;
                }
            }

            int idx = 0;
            for (int lat = 0; lat < latitudes; ++lat)
            {
                for (int lon = 0; lon < longitudes; ++lon)
                {
                    uint current = (uint)(lat * (longitudes + 1) + lon);
                    uint next = (uint)(current + longitudes + 1);

                    indices[idx++] = current;
                    indices[idx++] = next;
                    indices[idx++] = current + 1;

                    indices[idx++] = current + 1;
                    indices[idx++] = next;
                    indices[idx++] = next + 1;
                }
            }

            return (vertices, indices, uvs);
        }
    }
}
