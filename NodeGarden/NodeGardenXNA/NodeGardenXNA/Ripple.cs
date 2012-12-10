using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework.Graphics;
using NodeGardenXNA;

namespace NodeGardenXaml
{
    public class Ripple
    {
        protected const float StrokeWeightMin = 1.0f;
        protected const float StrokeWeightMax = 20.0f;
        const int VELOCITY = 3;
        float _radius;
        float thickness;
        Color _color;
        Color _shade;

        public Ripple(Color color) 
        {
            Init(color);
        }

        public bool IsVisible { get; set; }

        public void Update()
        {
            _radius += VELOCITY;
            thickness = (int)Global.Map(_radius, 0, Global.MinDist, StrokeWeightMax, StrokeWeightMin);
            _shade = _color * Global.Map(_radius, 0, 800, 1.0f, 0);

            if (_radius > 1000)
            {
                IsVisible = false;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 center, float minRadius)
        {
            if (_radius > minRadius)
            {
                Primitives2D.DrawCircle(sb, center, _radius, 180, _shade, thickness);
            }
        }

        public void Init(Color color)
        {
            IsVisible = true;
            _radius = 0;
            _color = color;
            _shade = color;
        }
    }
}
