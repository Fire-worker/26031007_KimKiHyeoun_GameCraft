using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using Vortice.DCommon;
using Vortice.DXGI;
using Vortice.Mathematics;

// 원본 PNG의 투명도를 사용해 런타임에서 흰색 실루엣을 만든다.
sealed class SwordLight : IDisposable
{
    private readonly ID2D1Bitmap _bitmap;
    public SwordLight(ID2D1RenderTarget target, string path)
    {
        using var source = new System.Drawing.Bitmap(G2Util.FindFilePath(path));
        byte[] pixels = new byte[source.Width * source.Height * 4];
        for (int y=0; y<source.Height; y++)
            for (int x=0; x<source.Width; x++)
            {
                byte alpha=source.GetPixel(x,y).A;
                int i=(y*source.Width+x)*4;
                pixels[i]=pixels[i+1]=pixels[i+2]=pixels[i+3]=alpha;
            }
        var pin=GCHandle.Alloc(pixels,GCHandleType.Pinned);
        try
        {
            _bitmap=target.CreateBitmap(new SizeI(source.Width,source.Height),pin.AddrOfPinnedObject(),
                (uint)source.Width*4,new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm,Vortice.DCommon.AlphaMode.Premultiplied)));
        }
        finally { pin.Free(); }
    }
    public void Draw(ID2D1RenderTarget target, Rect bounds, float opacity) =>
        target.DrawBitmap(_bitmap,bounds,opacity,BitmapInterpolationMode.Linear,new Rect(0,0,360,72));
    public void Dispose() => _bitmap.Dispose();
}
