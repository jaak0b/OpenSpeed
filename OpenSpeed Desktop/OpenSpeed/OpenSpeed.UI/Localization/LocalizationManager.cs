using System.Collections.Generic;
using System.ComponentModel;
using OpenSpeed.Core.Models;

namespace OpenSpeed.UI.Localization
{
  public class LocalizationManager : INotifyPropertyChanged
  {
    public static LocalizationManager Instance { get; } = new();

    private Language _current = Language.English;

    private static readonly Dictionary<string, string> English = new()
    {
      ["Connection"] = "CONNECTION",
      ["Z21Address"] = "Z21 Address",
      ["SensorAddress"] = "Speed Sensor Address",
      ["Locomotive"] = "LOCOMOTIVE",
      ["DecoderAddress"] = "Decoder Address",
      ["DccSpeedMode"] = "DCC Speed Mode",
      ["Scale"] = "Scale (1:N)",
      ["TabSpeed"] = "Speed",
      ["TabLength"] = "Length",
      ["MaxSpeed"] = "Max Speed (km/h)",
      ["SpeedStepInterval"] = "Speed Step Interval",
      ["StartingSpeedStep"] = "Starting Speed Step",
      ["SpeedStep"] = "Speed Step",
      ["LengthLabel"] = "Length: ",
      ["BtnMeasureLength"] = "Measure Length",
      ["BtnCancelLength"] = "Cancel Length Measurement",
      ["Results"] = "RESULTS",
      ["ColStep"] = "Step",
      ["ColForward"] = "Fwd (km/h)",
      ["ColBackward"] = "Bwd (km/h)",
      ["BtnStartSpeed"] = "Start Speed Measurement",
      ["BtnCancel"] = "Cancel Measurement",
      ["Language"] = "Language",
      ["MsgSetCv"] = "Please set CV3 and CV4 of the locomotive decoder to 0!",
      ["MsgInputRequired"] = "Input required",
      ["MsgError"] = "Error",
      ["MsgPositionLoco"] = "Position the locomotive before the sensors in the forward direction.\nPlease set CV3 and CV4 of the locomotive decoder to 0!",
      ["PlotTitle"] = "Speeds",
      ["PlotForward"] = "Speed Forwards",
      ["PlotBackward"] = "Speed Backwards",
      ["BtnExport"] = "Export to Excel",
      ["AxisSpeedStep"] = "Speed Step",
      ["AxisSpeed"] = "Speed (km/h)",
      ["MsgNoData"] = "No measurements to export.",
      ["TtMinimize"] = "Minimize",
      ["TtMaximize"] = "Maximize",
      ["TtClose"] = "Close"
    };

    private static readonly Dictionary<string, string> German = new()
    {
      ["Connection"] = "VERBINDUNG",
      ["Z21Address"] = "Z21-Adresse",
      ["SensorAddress"] = "Sensor-Adresse",
      ["Locomotive"] = "LOKOMOTIVE",
      ["DecoderAddress"] = "Decoder-Adresse",
      ["DccSpeedMode"] = "DCC-Fahrstufenmodus",
      ["Scale"] = "Maßstab (1:N)",
      ["TabSpeed"] = "Geschwindigkeit",
      ["TabLength"] = "Länge",
      ["MaxSpeed"] = "Höchstgeschwindigkeit (km/h)",
      ["SpeedStepInterval"] = "Fahrstufen-Intervall",
      ["StartingSpeedStep"] = "Start-Fahrstufe",
      ["SpeedStep"] = "Fahrstufe",
      ["LengthLabel"] = "Länge: ",
      ["BtnMeasureLength"] = "Länge messen",
      ["BtnCancelLength"] = "Längenmessung abbrechen",
      ["Results"] = "ERGEBNISSE",
      ["ColStep"] = "Stufe",
      ["ColForward"] = "Vor (km/h)",
      ["ColBackward"] = "Zurück (km/h)",
      ["BtnStartSpeed"] = "Geschwindigkeitsmessung starten",
      ["BtnCancel"] = "Messung abbrechen",
      ["Language"] = "Sprache",
      ["MsgSetCv"] = "Bitte CV3 und CV4 des Lok-Decoders auf 0 setzen!",
      ["MsgInputRequired"] = "Eingabe erforderlich",
      ["MsgError"] = "Fehler",
      ["MsgPositionLoco"] = "Lokomotive in Vorwärtsrichtung vor den Sensoren positionieren.\nBitte CV3 und CV4 des Lok-Decoders auf 0 setzen!",
      ["PlotTitle"] = "Geschwindigkeiten",
      ["PlotForward"] = "Geschwindigkeit vorwärts",
      ["PlotBackward"] = "Geschwindigkeit rückwärts",
      ["BtnExport"] = "Nach Excel exportieren",
      ["AxisSpeedStep"] = "Fahrstufe",
      ["AxisSpeed"] = "Geschwindigkeit (km/h)",
      ["MsgNoData"] = "Keine Messdaten zum Exportieren.",
      ["TtMinimize"] = "Minimieren",
      ["TtMaximize"] = "Maximieren",
      ["TtClose"] = "Schließen"
    };

    public Language Current => _current;

    public string this[string key]
    {
      get
      {
        var table = _current == Language.Deutsch ? German : English;
        return table.TryGetValue(key, out var value) ? value : key;
      }
    }

    public void SetLanguage(Language language)
    {
      if (language == _current)
        return;

      _current = language;
      PropertyChanged?.Invoke(this, new("Item[]"));
      LanguageChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? LanguageChanged;
  }
}
