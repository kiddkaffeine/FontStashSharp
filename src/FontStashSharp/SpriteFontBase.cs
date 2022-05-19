﻿using FontStashSharp.Interfaces;
using System.Collections.Generic;
using System.Text;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp
{
	public abstract partial class SpriteFontBase
	{
		internal static readonly Vector2 DefaultScale = new Vector2(1.0f, 1.0f);
		internal static readonly Vector2 DefaultOrigin = new Vector2(0.0f, 0.0f);

		/// <summary>
		/// Font Size
		/// </summary>
		public int FontSize { get; private set; }

		/// <summary>
		/// Line Height in pixels
		/// </summary>
		public int LineHeight { get; private set; }

		protected float RenderFontSizeMultiplicator { get; set; } = 1f;

		protected SpriteFontBase(int fontSize, int lineHeight)
		{
			FontSize = fontSize;
			LineHeight = lineHeight;
		}

#if MONOGAME || FNA || STRIDE
		protected internal abstract FontGlyph GetGlyph(GraphicsDevice device, int codepoint);
#else
		protected internal abstract FontGlyph GetGlyph(ITexture2DManager device, int codepoint);
#endif

		protected abstract void PreDraw(string str, out int ascent, out int lineHeight);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
#if MONOGAME || FNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			if (string.IsNullOrEmpty(text)) return 0.0f;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			var originOffset = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			for (int i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(text, i);
				if (codepoint == '\n')
				{
					originOffset.X = 0.0f;
					originOffset.Y += lineHeight;
					prevGlyph = null;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint);
#endif
				if (glyph == null)
				{
					continue;
				}

				if (!glyph.IsEmpty)
				{
					var renderOffset = new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y) + originOffset;
					renderer.Draw(glyph.Texture,
						position,
						glyph.TextureRectangle,
						color,
						rotation,
						origin - renderOffset,
						scale,
						layerDepth);
				}

				originOffset.X += GetXAdvance(glyph, prevGlyph);
				prevGlyph = glyph;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors,
								Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			if (string.IsNullOrEmpty(text)) return 0.0f;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			var originOffset = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			var pos = 0;
			for (int i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originOffset.X = 0.0f;
					originOffset.Y += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint);
#endif
				if (glyph == null)
				{
					++pos;
					continue;
				}

				if (!glyph.IsEmpty)
				{
					var renderOffset = new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y) + originOffset;
					renderer.Draw(glyph.Texture,
						position,
						glyph.TextureRectangle,
						colors[pos],
						rotation,
						origin - renderOffset,
						scale,
						layerDepth);
				}

				originOffset.X += GetXAdvance(glyph, prevGlyph);
				prevGlyph = glyph;
				++pos;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		protected abstract void PreDraw(StringBuilder str, out int ascent, out int lineHeight);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			if (text == null || text.Length == 0) return 0.0f;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			var originOffset = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			for (int i = 0; i < text.Length; i += StringBuilderIsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originOffset.X = 0.0f;
					originOffset.Y += lineHeight;
					prevGlyph = null;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint);
#endif
				if (glyph == null)
				{
					continue;
				}

				if (!glyph.IsEmpty)
				{
					var renderOffset = new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y) + originOffset;
					renderer.Draw(glyph.Texture,
						position,
						glyph.TextureRectangle,
						color,
						rotation,
						origin - renderOffset,
						scale,
						layerDepth);
				}

				originOffset.X += GetXAdvance(glyph, prevGlyph);
				prevGlyph = glyph;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			if (text == null || text.Length == 0) return 0.0f;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			var originOffset = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			var pos = 0;
			for (int i = 0; i < text.Length; i += StringBuilderIsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originOffset.X = 0.0f;
					originOffset.Y += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint);
#endif
				if (glyph == null)
				{
					++pos;
					continue;
				}

				if (!glyph.IsEmpty)
				{
					var renderOffset = new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y) + originOffset;
					renderer.Draw(glyph.Texture,
						position,
						glyph.TextureRectangle,
						colors[pos],
						rotation,
						origin - renderOffset,
						scale,
						layerDepth);
				}

				originOffset.X += GetXAdvance(glyph, prevGlyph);
				prevGlyph = glyph;
				++pos;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		protected virtual void InternalTextBounds(string str, Vector2 position, ref Bounds bounds)
		{
			if (string.IsNullOrEmpty(str)) return;

			int ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);
				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(null, codepoint);
				if (glyph == null)
				{
					continue;
				}

				var x0 = x + glyph.RenderOffset.X;
				if (x0 < minx)
					minx = x0;
				x += GetXAdvance(glyph, prevGlyph);
				if (x > maxx)
					maxx = x;

				var y0 = y + glyph.RenderOffset.Y;
				var y1 = y0 + glyph.Size.Y;
				if (y0 < miny)
					miny = y0;
				if (y1 > maxy)
					maxy = y1;

				prevGlyph = glyph;
			}

			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;
		}

		public void TextBounds(string str, Vector2 position, ref Bounds bounds, Vector2 scale)
		{
			InternalTextBounds(str, position, ref bounds);
			bounds.ApplyScale(scale / RenderFontSizeMultiplicator);
		}

		public void TextBounds(string str, Vector2 position, ref Bounds bounds)
		{
			TextBounds(str, position, ref bounds, DefaultScale);
		}

		protected virtual void InternalTextBounds(StringBuilder str, Vector2 position, ref Bounds bounds)
		{
			if (str == null || str.Length == 0) return;

			int ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(null, codepoint);
				if (glyph == null)
				{
					continue;
				}

				var x0 = x + glyph.RenderOffset.X;
				if (x0 < minx)
					minx = x0;
				x += GetXAdvance(glyph, prevGlyph);
				if (x > maxx)
					maxx = x;

				var y0 = y + glyph.RenderOffset.Y;
				var y1 = y0 + glyph.Size.Y;
				if (y0 < miny)
					miny = y0;
				if (y1 > maxy)
					maxy = y1;

				prevGlyph = glyph;
			}

			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;
		}

		public void TextBounds(StringBuilder str, Vector2 position, ref Bounds bounds, Vector2 scale)
		{
			InternalTextBounds(str, position, ref bounds);
			bounds.ApplyScale(scale / RenderFontSizeMultiplicator);
		}

		public void TextBounds(StringBuilder str, Vector2 position, ref Bounds bounds)
		{
			TextBounds(str, position, ref bounds, DefaultScale);
		}

		private static Rectangle ApplyScale(Rectangle rect, Vector2 scale)
		{
			return new Rectangle((int)Math.Round(rect.X * scale.X),
				(int)Math.Round(rect.Y * scale.Y),
				(int)Math.Round(rect.Width * scale.X),
				(int)Math.Round(rect.Height * scale.Y));
		}

		public List<Rectangle> GetGlyphRects(string str, Vector2 position, Vector2 origin, Vector2 scale)
		{
			List<Rectangle> rects = new List<Rectangle>();
			if (string.IsNullOrEmpty(str)) return rects;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var originOffset = new Vector2(-origin.X, -origin.Y + ascent);

			FontGlyph prevGlyph = null;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				var rect = new Rectangle((int)originOffset.X, (int)originOffset.Y - LineHeight, 0, LineHeight);
				if (codepoint == '\n')
				{
					originOffset.X = -origin.X;
					originOffset.Y += lineHeight;
					prevGlyph = null;
				}
				else
				{
					var glyph = GetGlyph(null, codepoint);
					if (glyph != null)
					{
						rect = glyph.RenderRectangle;
						rect.Offset((int)originOffset.X, (int)originOffset.Y);

						originOffset.X += GetXAdvance(glyph, prevGlyph);
						prevGlyph = glyph;
					}
				}

				rect = ApplyScale(rect, scale);
				rect.Offset((int)position.X, (int)position.Y);

				rects.Add(rect);
			}

			return rects;
		}

		public List<Rectangle> GetGlyphRects(string str, Vector2 position) => GetGlyphRects(str, position, Vector2.Zero, DefaultScale);

		public List<Rectangle> GetGlyphRects(StringBuilder str, Vector2 position, Vector2 origin, Vector2 scale)
		{
			List<Rectangle> rects = new List<Rectangle>();
			if (str == null || str.Length == 0) return rects;

			scale /= RenderFontSizeMultiplicator;

			int ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var originOffset = new Vector2(-origin.X, -origin.Y + ascent);

			FontGlyph prevGlyph = null;
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				var rect = new Rectangle((int)originOffset.X, (int)originOffset.Y - LineHeight, 0, LineHeight);
				if (codepoint == '\n')
				{
					originOffset.X = -origin.X;
					originOffset.Y += lineHeight;
					prevGlyph = null;
				}
				else
				{
					var glyph = GetGlyph(null, codepoint);
					if (glyph != null)
					{
						rect = glyph.RenderRectangle;
						rect.Offset((int)originOffset.X, (int)originOffset.Y);

						originOffset.X += GetXAdvance(glyph, prevGlyph);
						prevGlyph = glyph;
					}
				}

				rect = ApplyScale(rect, scale);
				rect.Offset((int)position.X, (int)position.Y);

				rects.Add(rect);
			}

			return rects;
		}

		public List<Rectangle> GetGlyphRects(StringBuilder str, Vector2 position) => GetGlyphRects(str, position, Vector2.Zero, DefaultScale);

		public Vector2 MeasureString(string text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(text, Utility.Vector2Zero, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(string text)
		{
			return MeasureString(text, DefaultScale);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(text, Utility.Vector2Zero, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			return MeasureString(text, DefaultScale);
		}

		protected static bool StringBuilderIsSurrogatePair(StringBuilder sb, int index)
		{
			if (index + 1 < sb.Length)
				return char.IsSurrogatePair(sb[index], sb[index + 1]);
			return false;
		}

		protected static int StringBuilderConvertToUtf32(StringBuilder sb, int index)
		{
			if (!char.IsHighSurrogate(sb[index]))
				return sb[index];

			return char.ConvertToUtf32(sb[index], sb[index + 1]);
		}

		internal abstract float GetXAdvance(FontGlyph glyph, FontGlyph prevGlyph);
	}
}