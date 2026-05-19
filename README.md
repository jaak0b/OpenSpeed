# OpenSpeed
OpenSpeed is an open source model train speed measuring system using cheap of the shelf hardware.

<img width="909" height="520" alt="image" src="https://github.com/user-attachments/assets/8b870938-3319-46e3-8379-e18b4ce6f565" />

# BOM
* 2 x  [IR Infrared Module](https://www.amazon.de/AZDelivery-Infrared-Detection-Compatible-Raspberry/dp/B07D924JHT/)
* 1 x [Seeed Studio XIAO ESP32C3](https://www.amazon.de/-/en/Seeed-Studio-XIAO-ESP32C3-Microcontroller/dp/B0B94JZ2YF/) (or any other wlan capable esp32)

# Arduino Sketch
Download the Arduino sketch for the ESP32C3 from [here](https://github.com/jaak0b/OpenSpeed/blob/main/Arduino%20Sketch/OpenSpeed/OpenSpeed.ino).  
Install the following libraries:

* https://github.com/ESP32Async/AsyncTCP
* https://github.com/ESP32Async/ESPAsyncWebServer

In the sketch you need to fill in the following values.
* WIFI_SSID: Wifi SSID
* WIFI_PASS: Wifi password
* SENSOR1_PIN / SENSOR2_PIN: Ir sensors pins. (by default they are D4 and D5 on the ESP32C3)
* Sensor_Distance_M: Distance between the two ir sensor in meters. 
