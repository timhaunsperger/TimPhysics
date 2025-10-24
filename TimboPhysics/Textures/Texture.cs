﻿using OpenTK.Graphics.OpenGL4;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TextureUnit = OpenTK.Graphics.OpenGL4.TextureUnit;

namespace TimboPhysics;

public class Texture
{
    public int Handle;
    public Texture(string path)
    {
        Handle = GL.GenTexture();
        Use(TextureUnit.Texture0);
        
        Image<Rgba32> image = Image.Load<Rgba32>(path);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        
        var pixels = new byte[4 * image.Width * image.Height];
        for (int y = 0; y < image.Height; y++) {
            var row = image.GetPixelRowSpan(y);

            for (int x = 0; x < image.Width; x++) {
                pixels[y*image.Width*4 + 4*x + 0] = row[x].R;
                pixels[y*image.Width*4 + 4*x + 1] = row[x].G;
                pixels[y*image.Width*4 + 4*x + 2] = row[x].B;
                pixels[y*image.Width*4 + 4*x + 3] = 0;
            }
        }
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.Rgba, 
            image.Width, 
            image.Height, 
            0, 
            PixelFormat.Rgba, 
            PixelType.UnsignedByte, 
            pixels.ToArray());
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Use(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
}