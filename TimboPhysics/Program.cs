
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TimboPhysics
{
    class Program
    {
        public static void Main(string[] args)
        {
            GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            gameWindowSettings.RenderFrequency = 200;
            gameWindowSettings.UpdateFrequency = 200;
            nativeWindowSettings.Title = "TimboPhysics";
            nativeWindowSettings.Size = new Vector2i(1000, 1000);
            Game game = new Game(gameWindowSettings, nativeWindowSettings);
            game.Run();
        }
    }
}