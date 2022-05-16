namespace TimboPhysics;

public static class TextureCache
{
    private static Dictionary<string,Texture> _textures = new ();
    public static Texture GetTexture(string path)
    {
        if (!_textures.ContainsKey(path))
        {
            _textures[path] = new Texture(path);
        }
        return _textures[path];
    }
}