#include <Arduino.h>
#include <WiFi.h>
#include <AsyncTCP.h>
#include <ESPAsyncWebServer.h>

// ---------------- EDIT FIELD BELOW ----------------
const char* WIFI_SSID = "SSID";
const char* WIFI_PASS = "PASSWORD";

const int SENSOR1_PIN = 6;
const int SENSOR2_PIN = 7;

const float SENSOR_DISTANCE_M = 0.49f;  // Distance in meters

AsyncWebServer server(80);

// ---- DO NOT EDIT ANYTHING BELOW ----
enum SystemState {
  WAITING_FOR_MEASUREMENT,
  MEASURING
};

SystemState systemState = WAITING_FOR_MEASUREMENT;

unsigned long startTime = 0;
unsigned long endTimeFront = 0;
unsigned long endTimeBack = 0;
unsigned long endSensorReleaseCandidate = 0;
int sensorLowCheck = 0;

uint32_t id = 0;
float speed_kmh = 0;
float train_length_cm = 0;

int startSensor = 0;  // either 1 or 2 for sensor 1 or 2.
int endSensor = 0;    // either 1 or 2 for sensor 1 or 2.

bool endSensorWasActive = false;

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

  server.on("/status", HTTP_GET, [](AsyncWebServerRequest *request) {
    String json = "{ \"status\": " + String(static_cast<int>(systemState)) + " }";
    request->send(200, "application/json", json);
  });

  server.on("/result", HTTP_GET, [](AsyncWebServerRequest *request) {
    String json = "{";
    json += "\"id\":" + String(id) + ",";
    json += "\"train_length_cm\":" + String(train_length_cm) + ",";
    json += "\"speed_kmh\":" + String(speed_kmh, 3);
    json += "}";

    request->send(200, "application/json", json);
  });

  server.on("/reset", HTTP_GET, [](AsyncWebServerRequest *request) {
    systemState = WAITING_FOR_MEASUREMENT;
    startTime = 0;
    endTimeFront = 0;
    endTimeBack = 0;
    endSensorReleaseCandidate = 0;
    endSensorWasActive = false;
    id = 0;
    speed_kmh = 0;
    train_length_cm = 0;
    startSensor = 0;
    endSensor = 0;
    sensorLowCheck = 0;
    request->send(200, "application/json", "{\"status\":\"reset\"}");
    Serial.println("System reset requested");
  });

  server.begin();
}

void loop() {

  if (systemState == WAITING_FOR_MEASUREMENT) {
    int s1 = digitalRead(SENSOR1_PIN);
    int s2 = digitalRead(SENSOR2_PIN);
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
      endTimeFront = 0;
      endTimeBack = 0;
      endSensorReleaseCandidate = 0;
      endSensorWasActive = false;

      Serial.println("Measurement started by sensor " + String(startSensor));
    }
  }

  if (systemState == MEASURING) {
    int endSensorPin = (endSensor == 1 ? SENSOR1_PIN : SENSOR2_PIN);

    if (!endSensorWasActive && digitalRead(endSensorPin) == LOW) {
      endSensorWasActive = true;
      endTimeFront = millis();
      sensorLowCheck = 0;
      Serial.println("Sensor " + String(endSensor) + " activated after " + String(endTimeFront / 1000) + " seconds");
    } else if (endSensorWasActive) {
      if (digitalRead(endSensorPin) == LOW) {
        endSensorReleaseCandidate = 0;
        endTimeBack = 0;
        return;
      } else {
        if (endTimeBack == 0)
          endTimeBack = millis();
        if (endSensorReleaseCandidate == 0)
          endSensorReleaseCandidate = micros();
        else if (micros() - endSensorReleaseCandidate >= 1000000) {
          unsigned long duration = endTimeFront - startTime;
          float seconds = duration / 1000.0f;

          id = esp_random();
          speed_kmh = (SENSOR_DISTANCE_M / seconds) * 3.6f;
          systemState = WAITING_FOR_MEASUREMENT;

          unsigned long totalDuration = endTimeBack - endTimeFront;
          float totalSeconds = totalDuration / 1000.0f;
          float speed_m_s = (SENSOR_DISTANCE_M / seconds);
          float train_length_m = speed_m_s * totalSeconds;
          train_length_cm = train_length_m * 100.0f;

          endSensorReleaseCandidate = 0;
          Serial.println("Measurement finished with id " + String(id));
          Serial.println("Speed: " + String(speed_kmh, 3) + " km/h");
          Serial.println("Train length: " + String(train_length_cm, 3) + " cm");
        }
      }
    }
  }
}
