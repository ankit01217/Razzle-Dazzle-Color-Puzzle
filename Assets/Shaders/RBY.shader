// References:
// http://forum.unity3d.com/threads/how-to-get-position-of-current-pixel-in-screen-space-in-framgment-shader-function.219843/
// http://forum.unity3d.com/threads/problem-with-grabpass-and-screenpos.172866/
// http://forum.unity3d.com/threads/use-grabpass-to-determine-color-of-pixel-before-rendering-the-current-pixel.178379/
// https://mispy.me/unity-alpha-blending-overlap/
// http://forum.unity3d.com/threads/stencil-readmask-as-parameter-dont-work.208256/
// http://docs.unity3d.com/Manual/SL-Stencil.html
// https://mispy.me/unity-alpha-blending-overlap/
// ReadMask determines which bits are read during stencil comparison (OLD - now using GrabPass for color replacement)
// WriteMask determines which bits can be written as a result of a stencil operation (OLD - now using GrabPass for color replacement)
//
// Bug note: Outline issue is always color of shape with Order in Layer 2 and appears around shape with order in Layer 3
Shader "Custom/RBY" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		
		//
		// COLORS
		//
		_Red       ("Red",    Color)    = (1,   0,     0,   1) // RGBA for Red
		_Blue      ("Blue",   Color)    = (0,   0,     1,   1) // RGBA for Blue
		_Yellow    ("Yellow", Color)    = (1,   1,     0,   1) // RGBA for Yellow
		_Purple    ("Purple", Color)    = (0.5, 0,     0.5, 1) // RGBA when Red and Blue combine
		//_PurpleBug ("PurpleBug", Color) = (92.0, 0,     204, 1) // RGBA when Red and Blue combine
		_Orange    ("Orange", Color)    = (1,   0.647, 0,   1) // RGBA when Red and Yellow combine
		_Green     ("Green",  Color)    = (0,   1,     0,   1) // RGBA when Blue and Yellow combine
		_AllColors ("AllColors", Color) = (0,   0,     0,   1) // RGBA when Red, Blue, and Yellow combine
	}

	CGINCLUDE
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		half4 _Red;
		half4 _Blue;
		half4 _Yellow;
		half4 _Purple;
		//half4 _PurpleBug;
		half4 _Orange;
		half4 _Green;
		half4 _AllColors;

		uniform sampler2D _MainTex;
		sampler2D _GrabTexture;

		struct v2f {
			half4 pos : POSITION;
			half2 uv : TEXCOORD0;
			float2 screenPos:TEXCOORD1;
		};

		bool areColorsEqual(half4 colorA, half4 colorB, bool isAlphaSensitive) {
			// Testing total equality is unreliable due to color space issues
			if (abs(colorA.r - colorB.r) > .02 ||
				abs(colorA.g - colorB.g) > .02 ||
				abs(colorA.b - colorB.b) > .02 ||
				(isAlphaSensitive && abs(colorA.a - colorB.a) > .02)) {
				return false;
			}
			else {
				return true;
			}
		}

		bool isBuggyEdgeCase(half4 pixelAbove, half4 pixelBelow, half4 pixelLeft, half4 pixelRight) {
			if ((areColorsEqual(pixelAbove, _Green, false) && areColorsEqual(pixelBelow, _AllColors, false)) ||
				(areColorsEqual(pixelBelow, _Green, false) && areColorsEqual(pixelAbove, _AllColors, false)) ||
				(areColorsEqual(pixelLeft, _Green, false) && areColorsEqual(pixelRight, _AllColors, false)) ||
				(areColorsEqual(pixelRight, _Green, false) && areColorsEqual(pixelLeft, _AllColors, false))) {
				return true;
			}
			else {
				return false;
			}
		}

		v2f vert(appdata_img v) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.screenPos = ComputeScreenPos(o.pos);

			//half2 uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
			o.uv = v.texcoord;//uv;

			//half2 uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
			//o.uv = uv;

			//o.pos = UnityPixelSnap (o.pos);

			return o;
		}
	ENDCG

	SubShader {
		Tags{ "Queue" = "Transparent" }

		// Save current screen contents into a texture
		GrabPass{}

		// Perform color replacement
		Pass {
			CGPROGRAM
				half4 frag(v2f i) : COLOR{
					half4 sourceColor = tex2D(_MainTex, i.uv);
					half4 destinationColor = tex2D(_GrabTexture, half2(i.screenPos.x, i.screenPos.y));

					half4 pixelAbove = tex2D(_GrabTexture, half2(i.screenPos.x, i.screenPos.y+2));
					half4 pixelBelow = tex2D(_GrabTexture, half2(i.screenPos.x, i.screenPos.y-2));
					half4 pixelLeft = tex2D(_GrabTexture, half2(i.screenPos.x+2, i.screenPos.y));
					half4 pixelRight = tex2D(_GrabTexture, half2(i.screenPos.x-2, i.screenPos.y));

					if (areColorsEqual(sourceColor, _Red, false) && sourceColor.a > 0) {
						if (areColorsEqual(destinationColor, _Blue, false)) {
							destinationColor = _Purple;
						} else if (areColorsEqual(destinationColor, _Yellow, false)) {
							destinationColor = _Orange;
						} else if (areColorsEqual(destinationColor, _Green, false)) {
							destinationColor = _AllColors;
						} else {
							destinationColor = sourceColor;
						}
					} else if (areColorsEqual(sourceColor, _Blue, false) && sourceColor.a > 0) {
						if (areColorsEqual(destinationColor, _Red, false)) {
							destinationColor = _Purple;
						} else if (areColorsEqual(destinationColor, _Yellow, false)) {
							destinationColor = _Green;
						} else if (areColorsEqual(destinationColor, _Orange, false)) {
							destinationColor = _AllColors;
						} else {
							destinationColor = sourceColor;
						}
					} else if (areColorsEqual(sourceColor, _Yellow, false) && sourceColor.a > 0) {
						if (areColorsEqual(destinationColor, _Red, false)) {
							destinationColor = _Orange;
						} else if (areColorsEqual(destinationColor, _Blue, false)) {
							destinationColor = _Green;
						} else if (areColorsEqual(destinationColor, _Purple, false)) {
							destinationColor = _AllColors;
						} else {
							destinationColor = sourceColor;
						}
					} else {
						discard;
					}
					return destinationColor;
				}
			ENDCG
		}
	}

	Fallback off
}