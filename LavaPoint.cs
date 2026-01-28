using System.Drawing;
using System.Drawing.Drawing2D;
using System;
// using System.Runtime.InteropServices; // not needed here

namespace TaskbarLavaLamp
{
    public class LavaPoint
    {
        public PointF Position;
        public PointF Velocity;
        public float Radius;
        public Color PointColor;
        public LavaShape Shape;

        private Random _rand;
        private float _wobblePhase;
        private float _wobbleSpeed;
        private float[] _vertexOffsets;

        public LavaPoint(Rectangle bounds, Random rand, Color color, LavaShape shape, float sizeMult)
        {
            _rand = rand;
            this.PointColor = color;
            this.Shape = shape;
            // Raggio base leggermente ridotto per far spazio al bagliore esterno
            this.Radius = (float)(rand.NextDouble() * 7 + 4) * sizeMult;
            this.Position = new PointF(rand.Next(bounds.Left, bounds.Right), rand.Next(bounds.Top, bounds.Bottom));

            _wobblePhase = (float)(rand.NextDouble() * Math.PI * 2);
            _wobbleSpeed = (float)(rand.NextDouble() * 0.1 + 0.05);
            _vertexOffsets = new float[] { (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() };
        }

        public void Update(Rectangle bounds, float speedMult)
        {
            // (La fisica del movimento rimane identica)
            float acceleration = 0.02f * speedMult;
            float wander = 0.035f * speedMult;
            float drag = 0.98f;

            float relativeY = this.Position.Y / (float)bounds.Height;
            if (relativeY > 0.75) this.Velocity.Y -= acceleration;
            else if (relativeY < 0.25) this.Velocity.Y += acceleration;

            this.Velocity.X += (float)(_rand.NextDouble() * (wander * 2) - wander);
            this.Velocity.X *= drag;
            this.Velocity.Y *= drag;

            this.Position.X += this.Velocity.X;
            this.Position.Y += this.Velocity.Y;

            _wobblePhase += _wobbleSpeed * speedMult;

            // Margine aumentato per evitare che il bagliore "salti" ai bordi
            float margin = Radius * 2;
            if (this.Position.Y > bounds.Bottom + margin) this.Position.Y = -margin;
            else if (this.Position.Y < -margin) this.Position.Y = bounds.Bottom + margin;
        }

        // --- NUOVO METODO HELPER PER GENERARE LA FORMA ---
        private GraphicsPath GeneratePath(float currentRadius)
        {
            GraphicsPath path = new GraphicsPath();
            if (this.Shape == LavaShape.Circle)
            {
                float deformX = (float)Math.Sin(_wobblePhase) * (currentRadius * 0.2f);
                path.AddEllipse(Position.X - currentRadius, Position.Y - currentRadius, (currentRadius * 2) + deformX, currentRadius * 2);
            }
            else if (this.Shape == LavaShape.Triangle)
            {
                PointF[] pts = new PointF[3];
                for (int i = 0; i < 3; i++)
                {
                    double angle = (Math.PI * 2 / 3 * i) - Math.PI / 2;
                    // La deformazione è proporzionale al raggio attuale
                    float deformedRadius = currentRadius + (float)Math.Sin(_wobblePhase + _vertexOffsets[i] * 5) * (currentRadius * 0.3f);
                    pts[i] = new PointF(
                        Position.X + (float)Math.Cos(angle) * deformedRadius,
                        Position.Y + (float)Math.Sin(angle) * deformedRadius
                    );
                }
                path.AddPolygon(pts);
            }
            else // Square
            {
                path.AddRectangle(new RectangleF(Position.X - currentRadius, Position.Y - currentRadius, currentRadius * 2, currentRadius * 2));
            }
            return path;
        }

        public void Draw(Graphics g)
        {
            // --- PASSAGGIO 1: IL BAGLIORE (GLOW) ---
            // Generiamo una forma più grande (1.4 volte il raggio normale)
            using (GraphicsPath glowPath = GeneratePath(Radius * 1.4f))
            using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
            {
                // Colore centro del bagliore (semi-trasparente)
                Color glowCenter = Color.FromArgb(100,
                    Math.Min(255, PointColor.R + 100),
                    Math.Min(255, PointColor.G + 100),
                    Math.Min(255, PointColor.B + 100));

                // Usare un surround semi-trasparente con gli stessi RGB del centro (alpha=0)
                // evita che i pixel di bordo vengano miscelati col colore di sfondo (TransparencyKey) e causino aloni verdi
                Color glowSurround = Color.FromArgb(0,
                    Math.Min(255, PointColor.R + 100),
                    Math.Min(255, PointColor.G + 100),
                    Math.Min(255, PointColor.B + 100));

                glowBrush.CenterColor = glowCenter;
                glowBrush.SurroundColors = new Color[] { glowSurround };

                // Il bagliore è concentrato al centro e sfuma rapidamente
                glowBrush.FocusScales = new PointF(0.3f, 0.3f);

                g.FillPath(glowBrush, glowPath);
            }

            // --- PASSAGGIO 2: IL NUCLEO ETEREO (CORE) ---
            // Generiamo la forma di dimensione normale
            using (GraphicsPath corePath = GeneratePath(Radius))
            using (PathGradientBrush coreBrush = new PathGradientBrush(corePath))
            {
                // Centro del nucleo: più solido (come prima)
                Color coreCenter = Color.FromArgb(230,
                    Math.Min(255, PointColor.R + 60),
                    Math.Min(255, PointColor.G + 60),
                    Math.Min(255, PointColor.B + 60));

                // Surround con alpha=0 ma stessi RGB del centro per evitare aloni del colore di background
                Color coreSurround = Color.FromArgb(0,
                    Math.Min(255, PointColor.R + 60),
                    Math.Min(255, PointColor.G + 60),
                    Math.Min(255, PointColor.B + 60));

                coreBrush.CenterColor = coreCenter;
                coreBrush.SurroundColors = new Color[] { coreSurround };
                coreBrush.FocusScales = new PointF(0.7f, 0.7f);

                g.FillPath(coreBrush, corePath);
            }
        }
    }

    // Native interop is centralized in NativeMethods.cs
}