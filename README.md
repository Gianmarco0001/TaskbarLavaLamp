# TaskbarLavaLamp

TaskbarLavaLamp is a lightweight C# .NET application that renders a pixel-art lava lamp in the empty space on your Windows taskbar.
I created this project to explore C# Windows Forms, Win32 API calls (P/Invoke), and custom GDI+ rendering for educational purposes.

TaskbarLavaLamp è una leggera applicazione C# .NET che renderizza una lava lamp in pixel-art nello spazio vuoto della tua barra delle applicazioni Windows.
Ho creato questo progetto per esplorare C# Windows Forms, le chiamate all'API Win32 (P/Invoke) e il rendering GDI+ personalizzato a scopo educativo.

---

![DEMOclip-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/1b85648e-ab84-4237-b1cb-e6309752199b)

---


## Features / Funzionalità

- Pixel-art metaball simulation for a fluid lava effect.
- Custom positioning and sizing via a dedicated "Setup Mode".
- Runs as a "click-through" overlay that doesn't block your taskbar.
- Control via System Tray icon (near the clock).
- Settings panel to change the lava color.
- All settings (position, size, color) are saved locally in `lavalamp.config.json`.

---
- Simulazione metaball in pixel-art per un effetto lava fluido.
- Posizionamento e dimensione personalizzati tramite una "Setup Mode" dedicata.
- Gira come un overlay "fantasma" che non blocca i click sulla taskbar.
- Controllo tramite icona nella System Tray (vicino all'orologio).
- Pannello impostazioni per cambiare il colore della lava.
- Tutte le impostazioni (posizione, dimensione, colore) sono salvate localmente in `lavalamp.config.json`.

---

## Installation / Installazione

This app does not require installation. / Questa app non richiede installazione.

1.  Go to the **[Releases](https://github.com/Gianmarco0001/TaskbarLavaLamp/releases)** page of this repository. / Vai alla pagina **[Releases](https://github.com/Gianmarco0001/TaskbarLavaLamp/releases)** di questo repository.
2.  Download the latest `.zip` file. / Scarica l'ultimo file `.zip`.
3.  Extract **all files** (`.exe` and `.dll`) to a new folder. / Estrai **tutti i file** (`.exe` e `.dll`) in una nuova cartella.
4.  Run `TaskbarLavaLamp.exe` to start. / Avvia `TaskbarLavaLamp.exe`.

---

## Usage / Utilizzo

### First Launch (Setup Mode) / Primo Avvio (Setup Mode)

On the first run, a red, semi-transparent box will appear. This is the Setup Mode. / Al primo avvio, apparirà un riquadro rosso semi-trasparente. Questa è la Setup Mode.

1.  **Drag** the box with your mouse to position it in an empty space on your taskbar. / **Trascina** il riquadro col mouse per posizionarlo in uno spazio vuoto sulla barra.
2.  Use **Arrow Keys** for pixel-perfect moving. / Usa i **Tasti Freccia** per movimenti di precisione.
3.  Use **SHIFT + Arrow Keys** to resize the box. / Usa **SHIFT + Tasti Freccia** per ridimensionare il riquadro.
4.  Press **ENTER** to save the position and start the lamp. / Premi **INVIO** per salvare la posizione e avviare la lampada.

### Controlling the Lamp / Controllare la Lampada

Right-click the lava lamp icon in your System Tray (near the clock, you may need to click the `^` arrow) to access the menu. / Fai clic destro sull'icona della lampada nella System Tray (vicino all'orologio, potresti dover cliccare la freccetta `^`) per accedere al menu.

-   **Riposiziona Lampada...**: Restarts Setup Mode (keeps your saved color). / Riavvia la Setup Mode (mantiene il colore salvato).
-   **Impostazioni...**: Opens the settings panel to change the color. / Apre il pannello impostazioni per cambiare il colore.
-   **Esci**: Closes the application. / Chiude l'applicazione.

---

## ⚠ Disclaimer

This is an experimental project created **for educational purposes**. / Questo è un progetto sperimentale creato **a scopo educativo**.
It is **not intended for production use** and may contain bugs. / Non è **destinato all'uso in produzione** e potrebbe contenere bug.

**Windows SmartScreen will show a warning** because the app is not digitally signed. This is normal for small, independent projects.
You must click **"More info"** -> **"Run anyway"** to start the app.

**Windows SmartScreen mostrerà un avviso** perché l'app non è firmata digitalmente. È normale per piccoli progetti indipendenti.
Devi cliccare **"Ulteriori informazioni"** -> **"Esegui comunque"** per avviare l'app.

---

## Building from Source / Compilare da Sorgente

1.  Clone the repository / Clona il repository:
    ```bash
    git clone https://github.com/Gianmarco0001/TaskbarLavaLamp.git
    cd TaskbarLavaLamp
    ```

2.  Open `TaskbarLavaLamp.sln` with **Visual Studio**. / Apri `TaskbarLavaLamp.sln` con **Visual Studio**.

3.  Ensure the **".NET Desktop Development"** workload is installed. / Assicurati di avere il workload **".NET Desktop Development"** installato.

4.  Install the `Newtonsoft.Json` package from the NuGet Package Manager. / Installa il pacchetto `Newtonsoft.Json` dal Gestore Pacchetti NuGet.

5.  Build or Run the project. / Compila o Avvia il progetto.

---

## License / Licenza

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.
Questo progetto è rilasciato sotto la **MIT License** – vedi il file [LICENSE](LICENSE) per i dettagli.

![License](https://img.shields.io/badge/license-MIT-green)
![C#](https://img.shields.io/badge/C%23-WinForms-blueviolet)
![.NET](https://img.shields.io/badge/.NET-Framework-purple)
