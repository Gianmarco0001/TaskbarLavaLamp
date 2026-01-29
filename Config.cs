using System.Drawing;

namespace TaskbarLavaLamp
{
    public enum LavaShape { Circle, Square, Triangle }

    public class Config
    {
        public int X { get; set; } = 100;
        public int Y { get; set; } = 100;
        public int Width { get; set; } = 400;
        public int Height { get; set; } = 50;
        public int LavaColorArgb { get; set; } = Color.Turquoise.ToArgb();
        public int LavaColorArgb2 { get; set; } = Color.LimeGreen.ToArgb();
        public float SpeedMultiplier { get; set; } = 1.0f;
        public float SizeMultiplier { get; set; } = 1.0f;
        public LavaShape SelectedShape { get; set; } = LavaShape.Circle;
        public bool IsFrutigerAero { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;

        // Numero di particelle per pixel di area (es. 0.001 => 1 particella ogni ~1000 px^2)
        public double ParticleDensity { get; set; } = 0.0015;

        // Limite superiore per evitare allocazioni eccessive su schermi grandi
        public int MaxParticles { get; set; } = 2000;
    }
}