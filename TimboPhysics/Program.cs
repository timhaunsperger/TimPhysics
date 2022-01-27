
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
            gameWindowSettings.RenderFrequency = 100;
            gameWindowSettings.UpdateFrequency = 100;
            nativeWindowSettings.Title = "TimboPhysics";
            nativeWindowSettings.Size = new Vector2i(1600, 900);
            Game game = new Game(gameWindowSettings, nativeWindowSettings);
            game.Run();
        }
    }
}