using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace CompScenes
{
    internal static class VisualExtensions
    {
        internal async static Task<CompositionVisualSurface> ToVisualSurfaceAsync(this Visual visual)
        {
            var compositor = visual.Compositor;
            var surface = compositor.CreateVisualSurface();

            // This only works if the visual comes from XAML and ElementCompositionPreview.SetIsTranslationEnabled was called on it, so we wrap it in a try-catch to prevent crashes 
            try { await visual.WarmUpAsync(); } catch { }

            surface.SourceVisual = visual;
            surface.SourceSize = visual.Size;

            return surface;
        }

        // This is a helper fuction to make XAML visuals return actual properties not the default values
        internal static async Task WarmUpAsync(this Visual visual)
        {
            var warmUpAnimation = visual.Compositor.CreateVector3KeyFrameAnimation();
            warmUpAnimation.InsertExpressionKeyFrame(0.0f, "Vector3(this.Target.Translation.X + 100f, this.Target.Translation.Y, 0f)");
            warmUpAnimation.InsertKeyFrame(1f, new Vector3(0f));
            warmUpAnimation.Duration = TimeSpan.FromMilliseconds(1);

            visual.StartAnimation("Translation", warmUpAnimation);

            await Task.Delay(1);
        }
    }
}
