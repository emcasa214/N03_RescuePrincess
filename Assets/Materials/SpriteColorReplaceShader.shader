Shader "Custom/SpriteColorReplaceShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorToReplace ("Color to Replace", Color) = (1, 0.5, 0, 1) // Màu cam
        _ReplacementColor ("Replacement Color", Color) = (0.5, 1, 1, 1) // Màu xanh nhạt
        _Range ("Color Match Range", Range(0, 0.2)) = 0.1 // Ngưỡng màu
        _EnableReplacement ("Enable Replacement", Float) = 0 // Bật/tắt thay thế
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorToReplace;
            fixed4 _ReplacementColor;
            float _Range;
            float _EnableReplacement;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu từ texture
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                // Tính khoảng cách màu
                float distance = sqrt(
                    pow(c.r - _ColorToReplace.r, 2) +
                    pow(c.g - _ColorToReplace.g, 2) +
                    pow(c.b - _ColorToReplace.b, 2)
                );

                // Nếu bật thay thế và màu nằm trong ngưỡng, thay bằng màu mới
                if (_EnableReplacement > 0.5 && distance < _Range)
                {
                    c.rgb = _ReplacementColor.rgb;
                    c.a = c.a * _ReplacementColor.a; // Giữ alpha của texture, nhưng áp dụng alpha của màu thay thế nếu cần
                }

                return c;
            }
            ENDCG
        }
    }
}