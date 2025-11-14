// Importiamo le librerie necessarie.
// System.Drawing ci serve per le strutture 'PointF' e 'Rectangle',
// che sono perfette per gestire coordinate 2D.
using System.Drawing;
// System ci serve per 'Random' e 'Math'.
using System;

namespace TaskbarLavaLamp
{
    // Questa è la nostra classe "modello". Non è un oggetto visivo,
    // ma una struttura dati (un "Plain Old C# Object" o POCO)
    // che definisce lo STATO di un singolo punto di "energia" della lava.
    public class LavaPoint
    {
        // --- STATO DELLA PARTICELLA ---

        // 'public' perché il nostro 'Form1' (il motore di rendering)
        // ha bisogno di leggere queste posizioni ad ogni frame.
        // Usiamo PointF (float) invece di Point (int) per un
        // movimento molto più fluido, non "a scatti" sui pixel.
        public PointF Position; // Vettore Posizione (x, y)
        public PointF Velocity; // Vettore Velocità (vx, vy)

        // Questo non è il raggio *visivo*, ma il raggio di *influenza*.
        // Sarà usato nella formula delle metaball (r^2 / dist^2)
        // per determinare la "forza" di questo punto.
        public float Radius;

        // Un'istanza privata di Random.
        private Random _rand;

        // --- PARAMETRI DI SIMULAZIONE ---

        // Queste 'const' sono le "leggi della fisica" del nostro micro-universo.
        // Tenerle come costanti in cima le rende facili da "sintonizzare"
        // per cambiare il "feel" della simulazione.
        private const float MaxSpeedY = 1.0f;     // Limite di velocità verticale
        private const float MaxSpeedX = 0.5f;     // Limite di velocità orizzontale
        private const float Acceleration = 0.02f; // Forza di "galleggiamento" (convezione)
        private const float Drag = 0.98f;         // Attrito/resistenza del fluido (un valore < 1)

        // Questa è la forza che genera il "wiggle" laterale.
        // Simula una sorta di moto Browniano.
        private const float WanderForce = 0.035f;

        // --- COSTRUTTORE ---
        // Questo è il metodo "spawner". Viene chiamato una sola volta
        // quando la particella viene creata.
        public LavaPoint(Rectangle bounds, Random rand)
        {
            // Nota importante: stiamo usando la "Dependency Injection" per 'rand'.
            // Invece di creare un 'new Random()' qui dentro (che darebbe lo stesso seme
            // a tutte le particelle create nello stesso millisecondo),
            // riceviamo un'istanza 'Random' già inizializzata dall'esterno.
            // Fondamentale per una randomizzazione corretta in un loop.
            _rand = rand;

            // Imposta uno stato iniziale casuale per la particella.
            // Dimensione (raggio di influenza)
            this.Radius = (float)(rand.NextDouble() * 5 + 5); // Valore tra 5.0 e 10.0

            // Posizione (un punto casuale all'interno dei bordi)
            this.Position = new PointF(
                rand.Next(bounds.Left, bounds.Right),
                rand.Next(bounds.Top, bounds.Bottom)
            );

            // Velocità iniziale (un piccolo "push" casuale in qualsiasi direzione)
            this.Velocity = new PointF(
                (float)(rand.NextDouble() * 0.5 - 0.25), // Tra -0.25 e +0.25
                (float)(rand.NextDouble() * 1.0 - 0.5)    // Tra -0.5 e +0.5
            );
        }

        // --- METODO DI UPDATE (IL CUORE) ---
        // Questo metodo viene chiamato ad ogni "tick" del nostro timer
        // di animazione (es. 30 volte al secondo).
        // Aggiorna lo stato (Posizione, Velocità) della particella.
        public void Update(Rectangle bounds)
        {
            // --- 1. SIMULAZIONE FISICA (FORZE) ---

            // Calcola la posizione Y normalizzata (0.0 = cima, 1.0 = fondo)
            float relativeY = this.Position.Y / (float)bounds.Height;

            // Aggiungiamo un fattore di "caos" alla nostra accelerazione
            // per rendere il movimento meno robotico.
            float randomFactor = (float)(_rand.NextDouble() * 0.5 + 0.8); // (da 0.8x a 1.3x)

            // Questa è la simulazione della CONVEZIONE (caldo/freddo)
            if (relativeY > 0.75) // Se è nella "zona calda" (fondo)
            {
                // Applica una forza verso l'alto (accelerazione negativa)
                this.Velocity.Y -= Acceleration * randomFactor;
            }
            else if (relativeY < 0.25) // Se è nella "zona fredda" (cima)
            {
                // Applica una forza verso il basso (accelerazione positiva)
                this.Velocity.Y += Acceleration * randomFactor;
            }

            // Applica la forza di "Wander" (movimento laterale)
            // Scegli un valore casuale tra -WanderForce e +WanderForce
            this.Velocity.X += (float)(_rand.NextDouble() * (WanderForce * 2) - WanderForce);

            // --- 2. AGGIORNAMENTO STATO (INTEGRAZIONE) ---

            // Applica l'attrito (Drag). Moltiplicare per < 1.0 rallenta
            // gradualmente la particella, simulando la resistenza del fluido.
            this.Velocity.X *= Drag;
            this.Velocity.Y *= Drag;

            // "Clamping" della velocità. Impedisce alle forze di accumularsi
            // all'infinito e far "esplodere" la simulazione.
            if (this.Velocity.Y < -MaxSpeedY) this.Velocity.Y = -MaxSpeedY;
            if (this.Velocity.Y > MaxSpeedY) this.Velocity.Y = MaxSpeedY;
            if (this.Velocity.X < -MaxSpeedX) this.Velocity.X = -MaxSpeedX;
            if (this.Velocity.X > MaxSpeedX) this.Velocity.X = MaxSpeedX;

            // Integrazione di Eulero (la forma più semplice):
            // Nuova Posizione = Vecchia Posizione + (Velocità * DeltaTempo)
            // (Dato che il nostro DeltaTempo è "1 tick", lo omettiamo)
            this.Position.X += this.Velocity.X;
            this.Position.Y += this.Velocity.Y;

            // --- 3. GESTIONE DEI BORDI (BOUNDARY HANDLING) ---

            // Asse X (Orizzontale): Logica di "Rimbalzo"
            if (this.Position.X - Radius < bounds.Left || this.Position.X + Radius > bounds.Right)
            {
                // Inverti la velocità orizzontale
                this.Velocity.X = -this.Velocity.X;

                // Questa riga è un "clamp" di sicurezza. Impedisce alla particella
                // di rimanere "incastrata" oltre il bordo se la sua velocità
                // è troppo alta. La forza a tornare dentro i limiti.
                this.Position.X = Math.Max(bounds.Left + Radius, Math.Min(bounds.Right - Radius, this.Position.X));
            }

            // Asse Y (Verticale): Logica di "Wrap-Around" (Teletrasporto)
            // Questo crea un loop infinito e fluido, molto più
            // "organico" di un semplice rimbalzo.
            if (this.Position.Y > bounds.Bottom + Radius) // Se esce dal fondo...
            {
                this.Position.Y = -Radius; // ...riappare in cima.
            }
            else if (this.Position.Y < -Radius) // Se esce dalla cima...
            {
                this.Position.Y = bounds.Bottom + Radius; // ...riappare sul fondo.
            }
        }
    }
}