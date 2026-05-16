#include <Arduino.h>
#include <WiFi.h>
#include <AsyncTCP.h>
#include <ESPAsyncWebServer.h>

// ---------------- EDIT FIELD BELOW ----------------
const char* WIFI_SSID = "SSID";
const char* WIFI_PASS = "PASSWORD";

const int SENSOR1_PIN = 6;
const int SENSOR2_PIN = 7;

const float SENSOR_DISTANCE_M = 0.49f; // Distance in meters

AsyncWebServer server(80);

// ---- DO NOT EDIT ANYTHING BELOW ----
enum SystemState {
  WAITING_FOR_MEASUREMENT,
  MEASURING
};

SystemState systemState = WAITING_FOR_MEASUREMENT;

unsigned long startTime = 0;
unsigned long endTime = 0;

unsigned long lastActiveEndSensor = 0;
bool endSensorWasActive = false;

uint32_t lastRandomId = 0;

int startSensor = 0; // either 1 or 2 for sensor 1 or 2.
int endSensor = 0;   // either 1 or 2 for sensor 1 or 2.

String stateToString() {
  return (systemState == WAITING_FOR_MEASUREMENT)
         ? "waitingForMeasurement"
         : "measuring";
}

void setup() {
  Serial.begin(115200);

  pinMode(SENSOR1_PIN, INPUT_PULLUP);
  pinMode(SENSOR2_PIN, INPUT_PULLUP);

  WiFi.begin(WIFI_SSID, WIFI_PASS);
  Serial.print("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(300);
    Serial.print(".");
  }
  Serial.println("\nConnected!");
  Serial.println(WiFi.localIP());

  randomSeed(esp_random());

  server.on("/status", HTTP_GET, [](AsyncWebServerRequest *request){
    String json = "{ \"status\": \"" + stateToString() + "\" }";
    request->send(200, "application/json", json);
  });

  server.on("/result", HTTP_GET, [](AsyncWebServerRequest *request){
    
    unsigned long duration = 0;
    float seconds = 0;
    float speed_kmh = 0;

    if (lastRandomId != 0) {
      duration = endTime - startTime;
      seconds = duration / 1000.0f;
      speed_kmh = (SENSOR_DISTANCE_M / seconds) * 3.6f;
      }

    String json = "{";
    json += "\"id\":" + String(lastRandomId) + ",";
    json += "\"duration_ms\":" + String(duration) + ",";
    json += "\"speed_kmh\":" + String(speed_kmh, 3) + ",";
    json += "\"timestamp\":" + String(millis());
    json += "}";

    request->send(200, "application/json", json);
  });

  server.on("/reset", HTTP_GET, [](AsyncWebServerRequest *request){
    systemState = WAITING_FOR_MEASUREMENT;
    startTime = 0;
    endTime = 0;
    lastActiveEndSensor = 0;
    endSensorWasActive = false;
    lastRandomId = 0;
    startSensor = 0;
    endSensor = 0;

    request->send(200, "application/json", "{\"status\":\"reset\"}");
    Serial.println("System reset to waitingForMeasurement");
  });

  server.begin();
}

void loop() {
  int s1 = digitalRead(SENSOR1_PIN);
  int s2 = digitalRead(SENSOR2_PIN);

  if (systemState == WAITING_FOR_MEASUREMENT) {
    if (s1 == LOW || s2 == LOW) {
      if (s1 == LOW) {
        startSensor = 1;
        endSensor = 2;
      } else {
        startSensor = 2;
        endSensor = 1;
      }

      systemState = MEASURING;
      startTime = millis();
      endSensorWasActive = false;
      lastActiveEndSensor = millis();

      Serial.print("Measurement started by sensor ");
      Serial.println(startSensor);
    }
  }

  if (systemState == MEASURING) {
    int endSensorPin = (endSensor == 1 ? SENSOR1_PIN : SENSOR2_PIN);

    if (digitalRead(endSensorPin) == LOW) {
      endSensorWasActive = true;
      lastActiveEndSensor = millis();
    }

    if (endSensorWasActive && (millis() - lastActiveEndSensor > 100)) {
      endTime = lastActiveEndSensor;
      lastRandomId = esp_random();

      Serial.print("Measurement finished. New ID: ");
      Serial.println(lastRandomId);

      systemState = WAITING_FOR_MEASUREMENT;
    }
  }
}
