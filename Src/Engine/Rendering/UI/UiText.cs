using SpatialEngine.Rendering;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StbTrueTypeSharp.StbTrueType;


namespace SpatialEngine.Rendering
{
    public class UiText : IDisposable
    {
        public string text;
        public Vector2 position;
        int width;
        public int height;
        public float scale;
        public float rotation;
        public int textHeight;
        public int numLines;
        public Vector3 color;

        Texture texture;
        byte[] bitmap;
        int elementIndex;

        public UiText()
        {
            
        }

        public UiText(string text, Vector2 pos, float scaleImage, float rotation, int textHeight, int numLines)
        {
            this.text = text;
            this.position = pos;
            this.width = (int)Window.size.X;
            this.height = textHeight;
            this.scale = scaleImage;
            this.rotation = rotation;
            this.textHeight = textHeight;
            this.numLines = numLines;
            
            CreateText();
        }
        
        public unsafe void CreateText()
        {
            int imageWidth = width;
            int imageHeight = height;

            float tempScale = stbtt_ScaleForPixelHeight(UiTextHandler.font, textHeight);

            height = textHeight * numLines;

            bitmap = new byte[width * height];

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(UiTextHandler.font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * tempScale);
            descent = (int)(descent * tempScale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* how wide is this character */
                    int ax;
                    int lsb;
                    stbtt_GetCodepointHMetrics(UiTextHandler.font, text[i], &ax, &lsb);

                    if (x + (ax * tempScale) > width)
                        continue;
                    /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                    /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                    int c_x1, c_y1, c_x2, c_y2;
                    stbtt_GetCodepointBitmapBox(UiTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights) */
                    int y = ascent + c_y1;

                    /* render character (stride and offset is important here) */
                    int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                    stbtt_MakeCodepointBitmap(UiTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                    /* advance x */
                    x += (int)(ax * tempScale);

                    /* add kerning */
                    int kern;
                    if(i + 1 != text.Length)
                    {
                        kern = stbtt_GetCodepointKernAdvance(UiTextHandler.font, text[i], text[i + 1]);
                        x += (int)(kern * tempScale);
                    }
                }
                
            }

            color = Vector3.One;
            texture = new Texture();
            texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            elementIndex = UiRenderer.uiElements.Count;
            //add on the length from the actual text to center it to a position
            UiRenderer.AddElement(texture, new Vector2(position.X + (width - x), position.Y), rotation, scale, new Vector2(imageWidth, imageHeight), UiElementType.text);
        }
        
        public unsafe void CreateText(string text, Vector2 pos, float scaleImage, float rotation, int textHeight, int numLines)
        {
            this.text = text;
            this.position = pos;
            this.width = (int)Window.size.X;
            this.height = textHeight;
            this.scale = scaleImage;
            this.rotation = rotation;
            this.textHeight = textHeight;
            this.numLines = numLines;
            
            int imageWidth = width;
            int imageHeight = height;

            float tempScale = stbtt_ScaleForPixelHeight(UiTextHandler.font, textHeight);

            height += textHeight * (numLines - 1);

            bitmap = new byte[width * height];

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(UiTextHandler.font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * tempScale);
            descent = (int)(descent * tempScale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* how wide is this character */
                    int ax;
                    int lsb;
                    stbtt_GetCodepointHMetrics(UiTextHandler.font, text[i], &ax, &lsb);

                    if (x + (ax * tempScale) > width)
                        continue;
                    /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                    /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                    int c_x1, c_y1, c_x2, c_y2;
                    stbtt_GetCodepointBitmapBox(UiTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights) */
                    int y = ascent + c_y1;

                    /* render character (stride and offset is important here) */
                    int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                    stbtt_MakeCodepointBitmap(UiTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                    /* advance x */
                    x += (int)(ax * tempScale);

                    /* add kerning */
                    int kern;
                    if(i + 1 != text.Length)
                    {
                        kern = stbtt_GetCodepointKernAdvance(UiTextHandler.font, text[i], text[i + 1]);
                        x += (int)(kern * tempScale);
                    }
                }
                
            }
            
            color = Vector3.One;
            texture = new Texture();
            texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            elementIndex = UiRenderer.uiElements.Count;
            //add on the length from the actual text to center it to a position
            UiRenderer.AddElement(texture, new Vector2(position.X + (width - x), position.Y), rotation, scale, new Vector2(imageWidth, imageHeight), UiElementType.text);
        }
        
        public unsafe void UpdateText(string text, Vector2 pos, float scaleImage, float rotation, int textHeight, int numLines)
        {
            this.text = text;
            this.position = pos;
            this.width = (int)Window.size.X;
            this.height = textHeight;
            this.scale = scaleImage;
            this.rotation = rotation;
            this.textHeight = textHeight;
            this.numLines = numLines;
            
            UiRenderer.uiElements[elementIndex].width = width;
            UiRenderer.uiElements[elementIndex].height = height;

            float tempScale = stbtt_ScaleForPixelHeight(UiTextHandler.font, textHeight);

            height += textHeight * (numLines - 1);

            bitmap = new byte[width * height];

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(UiTextHandler.font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * tempScale);
            descent = (int)(descent * tempScale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* how wide is this character */
                    int ax;
                    int lsb;
                    stbtt_GetCodepointHMetrics(UiTextHandler.font, text[i], &ax, &lsb);

                    if (x + (ax * tempScale) > width)
                        continue;
                    /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                    /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                    int c_x1, c_y1, c_x2, c_y2;
                    stbtt_GetCodepointBitmapBox(UiTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights) */
                    int y = ascent + c_y1;

                    /* render character (stride and offset is important here) */
                    int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                    stbtt_MakeCodepointBitmap(UiTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                    /* advance x */
                    x += (int)(ax * tempScale);

                    /* add kerning */
                    int kern;
                    if(i + 1 != text.Length)
                    {
                        kern = stbtt_GetCodepointKernAdvance(UiTextHandler.font, text[i], text[i + 1]);
                        x += (int)(kern * tempScale);
                    }
                }
                
            }

            UiRenderer.uiElements[elementIndex].color = color;
            UiRenderer.uiElements[elementIndex].scale = scaleImage;
            UiRenderer.uiElements[elementIndex].position = new Vector2(pos.X + (width - x), pos.Y);
            UiRenderer.uiElements[elementIndex].rotation = rotation;
            UiRenderer.uiElements[elementIndex].texture.UpdateTexture(bitmap, width, height);
        }
        
        public void Dispose()
        {
            UiRenderer.DeleteElement(elementIndex);
        }
    }
    
    
    public static class UiTextHandler
    {
        public static stbtt_fontinfo font;
        static byte[] fontData;
        public unsafe static void Init()
        {
            fontData = File.ReadAllBytes(SpatialEngine.Resources.FontPath + "JetBrainsMono-Regular.ttf");
            font = new stbtt_fontinfo();
            
            fixed (byte* fontDataPtr = fontData)
            {
                if (stbtt_InitFont(font, fontDataPtr, 0) == 0)
                {
                    throw new Exception("Error occured while loading font");
                }
            }
        }
    }
}

