using Android.Graphics.Drawables;
using Microsoft.Maui.Controls.Handlers;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace ValdemoroEn1.Extensions;

public static partial class HandlersExtensions
{
    private static void RegisterAndroidMappers()
    {
        ShellHandler.Mapper.AppendToMapping("ValdemoroEn1.FlyoutStatusBarBackground", (handler, _) =>
        {
            AView configuredFlyout = null;
            LayerDrawable layers = null;

            void Update()
            {
                var flyout = handler.PlatformView.FlyoutView;
                if (flyout is null)
                {
                    return;
                }

                if (!ReferenceEquals(configuredFlyout, flyout))
                {
                    configuredFlyout = flyout;
                    layers = new LayerDrawable([
                        flyout.Background ?? new ColorDrawable(AColor.White),
                        new ColorDrawable(AColor.ParseColor("#043465"))
                    ]);
                    layers.SetLayerGravity(1, Android.Views.GravityFlags.Top | Android.Views.GravityFlags.FillHorizontal);
                    flyout.Background = layers;
                }

                layers.SetLayerHeight(1, flyout.PaddingTop);
                flyout.Invalidate();
            }

            handler.PlatformView.LayoutChange += (_, _) => Update();
            Update();
        });
    }
}
