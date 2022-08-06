Correct vtfcmd usage:
 -file <path>             (Input file path.)
 -folder <path>           (Input directory search string.)
 -output <path>           (Output directory.)
 -prefix <string>         (Output file prefix.)
 -postfix <string>        (Output file postfix.)
 -version <string>        (Ouput version.)
 -format <string>         (Ouput format to use on non-alpha textures.)
 -alphaformat <string>    (Ouput format to use on alpha textures.)
 -flag <string>           (Output flags to set.)
 -resize                  (Resize the input to a power of 2.)
 -rmethod <string>        (Resize method to use.)
 -rfilter <string>        (Resize filter to use.)
 -rsharpen <string>       (Resize sharpen filter to use.)
 -rwidth <integer>        (Resize to specific width.)
 -rheight <integer>       (Resize to specific height.)
 -rclampwidth <integer>   (Maximum width to resize to.)
 -rclampheight <integer>  (Maximum height to resize to.)
 -gamma                   (Gamma correct image.)
 -gcorrection <single>    (Gamma correction to use.)
 -nomipmaps               (Don't generate mipmaps.)
 -mfilter <string>        (Mipmap filter to use.)
 -msharpen <string>       (Mipmap sharpen filter to use.)
 -normal                  (Convert input file to normal map.)
 -nkernel <string>        (Normal map generation kernel to use.)
 -nheight <string>        (Normal map height calculation to use.)
 -nalpha <string>         (Normal map alpha result to set.)
 -nscale <single>         (Normal map scale to use.)
 -nwrap                   (Wrap the normal map for tiled textures.)
 -bumpscale <single>      (Engine bump mapping scale to use.)
 -nothumbnail             (Don't generate thumbnail image.)
 -noreflectivity          (Don't calculate reflectivity.)
 -shader <string>         (Create a material for the texture.)
 -param <string> <string> (Add a parameter to the material.)
 -recurse                 (Process directories recursively.)
 -exportformat <string>   (Convert VTF files to the format of this extension.)
 -silent                  (Silent mode.)
 -pause                   (Pause when done.)
 -help                    (Display vtfcmd help.)

Example vtfcmd usage:
vtfcmd.exe -file "C:\texture1.bmp" -file "C:\texture2.bmp" -format "dxt1"
vtfcmd.exe -file "C:\texture.bmp" -format "bgr888" -normal -postfix "normal_"
vtfcmd.exe -folder "C:\input\*.tga" -output "C:\output" -recurse -pause
vtfcmd.exe -folder "C:\output\*.vtf" -output "C:\input" -exportformat "jpg"

Formats: RGBA8888, ABGR8888, RGB888, BGR888, RGB565, I8, IA88, A8,
         RGB888_BLUESCREEN, BGR888_BLUESCREEN, ARGB8888, BGRA8888, DXT1,
         DXT3, DXT5, BGRX8888, BGR565, BGRX5551, BGRA4444,DXT1_ONEBITALPHA,
         BGRA5551, UV88, UVWQ8888, RGBA16161616F, RGBA16161616, UVLX8888

Flags:   POINTSAMPLE, TRILINEAR, CLAMPS, CLAMPT, ANISOTROPIC, HINT_DXT5,
         NORMAL, NOMIP, NOLOD, MINMIP, PROCEDURAL, RENDERTARGET,
         DEPTHRENDERTARGET, NODEBUGOVERRIDE, SINGLECOPY, NODEPTHBUFFER
         CLAMPU, VERTEXTEXTURE, SSBUMP, BORDER
Resize Method:  NEAREST, BIGGEST, SMALLEST

Resize Filter:  POINT, BOX, TRIANGLE, QUADRATIC, CUBIC, CATROM, MITCHELL
                GAUSSIAN, SINC, BESSEL, HANNING, HAMMING, BLACKMAN, KAISER

Sharpen Filter: NONE, NEGATIVE, LIGHTER, DARKER, CONTRASTMORE, CONTRASTLESS,
                SMOOTHEN, SHARPENSOFT, SHARPENMEDIUM, SHARPENSTRONG,
                FINDEDGES, CONTOUR, EDGEDETECT, EDGEDETECTSOFT, EMBOSS
                MEANREMOVAL, UNSHARP, XSHARPEN, WARPSHARP

Normal Kernal:  4X, 3X3, 5X5, 7X7, 9X9, DUDV

Normal Height:  ALPHA, AVERAGERGB, BIASEDRGB, RED, GREEN, BLUE, MAXRGB,
                COLORSPACE

Normal Alpha:   NOCHANGE, HEIGHT, BLACK, WHITE