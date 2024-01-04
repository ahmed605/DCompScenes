using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;

namespace CompScenes
{
    internal static class CompositionSurfaceExtensions
    {
        internal static SceneSurfaceMaterialInput ToMaterialInput(this ICompositionSurface surface, Compositor compositor, Vector2 size, bool invert = false)
        {
            var visual = compositor.CreateSpriteVisual();
            var surfaceBrush = compositor.CreateSurfaceBrush(surface);
            var visualSurface = compositor.CreateVisualSurface();
            var surfaceMaterial = SceneSurfaceMaterialInput.Create(compositor);

            surfaceBrush.Stretch = CompositionStretch.UniformToFill;

            if (invert)
                surfaceBrush.TransformMatrix = Matrix3x2.CreateScale(-1, 1, size / 2.0f);

            visual.Size = size;
            visual.Brush = surfaceBrush;

            visualSurface.SourceVisual = visual;
            visualSurface.SourceSize = size;

            surfaceMaterial.Surface = visualSurface;
            surfaceMaterial.WrappingUMode = SceneWrappingMode.MirroredRepeat;
            surfaceMaterial.WrappingVMode = SceneWrappingMode.MirroredRepeat;

            return surfaceMaterial;
        }
    }
}
