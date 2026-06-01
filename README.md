# OpenSpeed

**Precision speed and length measurement for model trains.**

OpenSpeed measures the scale speed of every DCC speed step and the physical length of any consist — fully automated, no modifications to the locomotive required.

> ⚠️ **Requires a Roco / Fleischmann Z21 command station.** No other DCC system is supported.

---

## How it works

Two IR sensors are mounted above a straight section of track at a fixed distance. As a train passes, the ESP32 firmware measures the transit time between the sensors (speed) and how long the second sensor stays blocked (length). The desktop app drives the locomotive through each speed step via the Z21, collects the results, and plots them live.

## What you need

- Roco / Fleischmann Z21 (LAN)
- ESP32 DevKit + 2× TCRT5000 IR sensors
- Windows 10/11 PC on the same network

## Features

- Automatic sweep of all DCC speed steps — forward and backward pass each
- Train length measurement (averaged over two passes)
- Live speed chart with Excel export
- Dark and light mode, English and German UI

## Documentation & wiring guide

👉 **[openspeed.jaak0b.github.io/OpenSpeed](https://jaak0b.github.io/OpenSpeed)**

Full wiring diagrams, firmware setup, sensor distance configuration, and first-run instructions are on the documentation site.

## License

MIT — see [LICENSE](LICENSE).
