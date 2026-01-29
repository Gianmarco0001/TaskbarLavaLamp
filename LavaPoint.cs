using System.Drawing;
using System.Drawing.Drawing2D;
using System;
 
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
            // Parametri fisici base - valori più moderati per minor accumulo in alto
            float baseBuoyancy = 0.018f * speedMult; // forza base che genera la salita
            float wander = 0.03f * speedMult;
            float drag = 0.975f;
 
            // Normalizzazioni coordinate
            float normalizedY = Math.Max(0f, Math.Min(1f, this.Position.Y / (float)Math.Max(1, bounds.Height))); // 0..1 (top..bottom)
            float xNorm = Math.Max(0f, Math.Min(1f, this.Position.X / (float)Math.Max(1, bounds.Width)));
 
            // --- Convezione: colonne ascensionali/descensionali variabili nel tempo per realismo ---
            // convFreq = quante "colonne" orizzontali appaiono
            float convFreq = 3.0f;
            float convStrength = 0.035f * speedMult; // intensità della corrente di convezione
            float conv = (float)Math.Sin(xNorm * convFreq * (float)(Math.PI * 2) + _wobblePhase * 0.6f) * convStrength;
            // conv positivo -> spinta verso l'alto, negativo -> verso il basso (crea celle convettive)
 
            // --- Buoyancy: più forte verso il fondo, morbida verso la superficie ---
            float buoyancy = baseBuoyancy * (0.6f + normalizedY * 1.4f);
            // Combina buoyancy + convezione (la convezione può sopraffare localmente la spinta base)
            this.Velocity.Y -= (buoyancy + conv);
 
            // --- Repulsione morbida verso il basso quando troppo vicino alla cima ---
            float topThreshold = 0.10f;
            if (normalizedY < topThreshold)
            {
                // spinta verso il basso proporzionale alla distanza dalla soglia
                this.Velocity.Y += (topThreshold - normalizedY) * 0.06f * speedMult;
            }
 
            // --- Leggera accelerazione verso il basso quando molto in basso (sink naturale) ---
            float bottomThreshold = 0.92f;
            if (normalizedY > bottomThreshold)
            {
                this.Velocity.Y += (normalizedY - bottomThreshold) * 0.06f * speedMult;
            }
 
            // --- Turbolenza casuale verticale per rompere aggregazioni ---
            float turbulenceY = (float)(_rand.NextDouble() * 0.05 - 0.025) * speedMult; // ridotta rispetto a prima
            this.Velocity.Y += turbulenceY;
 
            // --- Flusso orizzontale coerente + wander casuale per evitare accumuli laterali ---
            float lateralFlow = (float)Math.Cos(xNorm * convFreq * (float)(Math.PI * 2) + _wobblePhase * 0.4f) * (0.012f * speedMult);
            this.Velocity.X += lateralFlow;
            this.Velocity.X += (float)(_rand.NextDouble() * (wander * 2) - wander);
 
            // Piccola repulsione locale basata sulla dimensione per prevenire clustering visivo senza neighbor checks costosi
            this.Velocity.X += (float)(_rand.NextDouble() - 0.5) * 0.01f * (this.Radius / 6f) * speedMult;
 
            // Applicazione del drag
            this.Velocity.X *= drag;
            this.Velocity.Y *= drag;
 
            // Aggiornamento posizione
            this.Position.X += this.Velocity.X;
            this.Position.Y += this.Velocity.Y;
 
            _wobblePhase += _wobbleSpeed * speedMult;
 
            // Margine aumentato per evitare che il bagliore "salti" ai bordi
            float margin = Radius * 2;
            if (this.Position.Y > bounds.Bottom + margin) this.Position.Y = -margin;
            else if (this.Position.Y < -margin) this.Position.Y = bounds.Bottom + margin;
 
            // Avvolgimento orizzontale semplice per evitare accumulo ai lati della finestra
            if (this.Position.X < -margin) this.Position.X = bounds.Right + margin;
            else if (this.Position.X > bounds.Right + margin) this.Position.X = -margin;
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