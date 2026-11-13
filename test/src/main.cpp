/*
 * WaterSensor Prototype (Máy thô, cảm biến rời)
 * ESP32-WROOM-32 + Cảm biến rời: pH (GPIO34) + TDS (GPIO35, kích Q1 GPIO26) + DS18B20 (GPIO4)
 * Giao thức truyền tin: MQTT (TCP port 1883) qua WiFi
 */

#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <Preferences.h>

// ============================================================
// CẤU HÌNH PHẦN CỨNG & CHÂN PIN
// ============================================================
#define PH_PIN          34
#define TDS_PIN         35
#define ONE_WIRE_BUS     4
#define TDS_POWER_PIN   26

const float V_REF          = 3.3f;
const float ADC_RESOLUTION = 4095.0f;
const float DIVIDER_RATIO  = 20.0f / (10.0f + 20.0f); // 0.6667
const float V_PH7          = 3.6566f;
const float V_PH4          = 4.3770f;
const float PH_SLOPE       = (6.85f - 3.5f) / (V_PH7 - V_PH4);

// ============================================================
// CẤU HÌNH WI-FI & MQTT BROKER
// ============================================================
const char* WIFI_SSID       = "YOUR_WIFI_SSID";
const char* WIFI_PASSWORD   = "YOUR_WIFI_PASSWORD";

// Địa chỉ IP của Broker (NAS hoặc máy nội bộ LAN, vd: 192.168.1.150)
const char* MQTT_BROKER     = "192.168.1.150";
const uint16_t MQTT_PORT    = 1883;
const char* MQTT_USER       = "";  // Để trống nếu Mosquitto cho phép anonymous
const char* MQTT_PASSWORD   = "";
const char* DEVICE_ID       = "ESP32_01";

// Topic quy chuẩn khớp với WinForms Desktop UI & Backend
// water/device/{DEVICE_ID}/telemetry -> Node phát số liệu đo
// water/device/{DEVICE_ID}/calibrate -> Node lắng nghe lệnh hiệu chuẩn từ Desktop
// water/device/{DEVICE_ID}/status    -> Trạng thái kết nối (LWT: online/offline)
char TOPIC_TELEMETRY[64];
char TOPIC_CALIBRATE[64];
char TOPIC_STATUS[64];

// ============================================================
// BIẾN ĐO ĐẠC & THÔNG SỐ HIỆU CHUẨN (LƯU NVS FLASH)
// ============================================================
float phValue     = 7.0f;
float tdsValue    = 0.0f;
float temperature = 25.0f;

// Thông số hiệu chuẩn nhận từ Desktop UI qua MQTT
float phOffset    = 0.00f;  // Cộng vào kết quả pH
float tdsFactor   = 1.00f;  // Nhân với kết quả TDS
float tempOffset  = 0.00f;  // Cộng vào kết quả nhiệt độ

Preferences prefs;
WiFiClient espClient;
PubSubClient mqttClient(espClient);

OneWire oneWire(ONE_WIRE_BUS);
DallasTemperature sensors(&oneWire);

unsigned long lastReadTime = 0;
const unsigned long READ_INTERVAL = 4000UL; // Đọc và phát dữ liệu mỗi 4 giây
unsigned long lastReconnectAttempt = 0;

// ============================================================
// NVS PREFERENCES (LƯU VÀ TẢI THÔNG SỐ HIỆU CHUẨN)
// ============================================================
void loadCalibration() {
  prefs.begin("water_cal", true);
  phOffset   = prefs.getFloat("ph_off", 0.00f);
  tdsFactor  = prefs.getFloat("tds_fac", 1.00f);
  tempOffset = prefs.getFloat("temp_off", 0.00f);
  prefs.end();
  Serial.printf("[NVS] Da tai hieu chuan: phOffset=%.2f, tdsFactor=%.2f, tempOffset=%.2f\n",
                phOffset, tdsFactor, tempOffset);
}

void saveCalibration(float phOff, float tdsFac, float tempOff) {
  prefs.begin("water_cal", false);
  prefs.putFloat("ph_off", phOff);
  prefs.putFloat("tds_fac", tdsFac);
  prefs.putFloat("temp_off", tempOff);
  prefs.end();
  phOffset   = phOff;
  tdsFactor  = tdsFac;
  tempOffset = tempOff;
  Serial.printf("[NVS] Da luu hieu chuan moi: phOffset=%.2f, tdsFactor=%.2f, tempOffset=%.2f\n",
                phOffset, tdsFactor, tempOffset);
}

// ============================================================
// HÀM ĐO ĐẠC CẢM BIẾN
// ============================================================
float readPH() {
  long sum = 0;
  for (int i = 0; i < 40; i++) {
    sum += analogRead(PH_PIN);
    delay(5);
  }
  float adcAvg    = sum / 40.0f;
  float vMeasured = adcAvg * (V_REF / ADC_RESOLUTION);
  float vActual   = vMeasured / DIVIDER_RATIO;
  float rawPh     = 6.85f + PH_SLOPE * (vActual - V_PH7);
  return rawPh + phOffset; // Áp dụng độ lệch hiệu chuẩn
}

float readTDS(float temp) {
  digitalWrite(TDS_POWER_PIN, HIGH);
  delay(500); // Ổn định que đo
  long sum = 0;
  for (int i = 0; i < 20; i++) {
    sum += analogRead(TDS_PIN);
    delay(5);
  }
  digitalWrite(TDS_POWER_PIN, LOW); // Tắt que đo tránh ăn mòn điện hóa

  float adcAvg  = sum / 20.0f;
  float voltage = adcAvg * (V_REF / ADC_RESOLUTION);
  float coeff   = 1.0f + 0.02f * (temp - 25.0f);
  float vComp   = voltage / coeff;

  float rawTds = (133.42f * pow(vComp, 3)
                - 255.86f * pow(vComp, 2)
                + 857.39f * vComp) * 0.5f;
  if (rawTds < 0.0f) rawTds = 0.0f;
  return rawTds * tdsFactor; // Áp dụng hệ số hiệu chuẩn
}

// ============================================================
// MQTT CALLBACK (NHẬN LỆNH HIỆU CHUẨN TỪ DESKTOP UI)
// ============================================================
void mqttCallback(char* topic, byte* message, unsigned int length) {
  char payload[160];
  if (length >= sizeof(payload)) length = sizeof(payload) - 1;
  memcpy(payload, message, length);
  payload[length] = '\0';

  Serial.printf("[MQTT] Nhan topic %s: %s\n", topic, payload);

  // Parse JSON hiệu chuẩn từ Desktop UI (YAGNI, zero-dep)
  // Format mong đợi: {"phOffset":0.15,"tdsFactor":1.05,"tempOffset":-0.2}
  if (strcmp(topic, TOPIC_CALIBRATE) == 0) {
    float newPh = phOffset;
    float newTds = tdsFactor;
    float newTemp = tempOffset;

    char* pPh = strstr(payload, "\"phOffset\":");
    if (pPh) newPh = atof(pPh + 11);

    char* pTds = strstr(payload, "\"tdsFactor\":");
    if (pTds) {
      float parsedTds = atof(pTds + 12);
      if (parsedTds > 0.0f) newTds = parsedTds;
    }

    char* pTemp = strstr(payload, "\"tempOffset\":");
    if (pTemp) newTemp = atof(pTemp + 13);

    saveCalibration(newPh, newTds, newTemp);

    // Phản hồi xác nhận về broker
    char ack[128];
    snprintf(ack, sizeof(ack), "{\"deviceId\":\"%s\",\"status\":\"calibrated\",\"ph\":%.2f,\"tds\":%.2f}",
             DEVICE_ID, phOffset, tdsFactor);
    mqttClient.publish(TOPIC_STATUS, ack);
  }
}

// ============================================================
// KẾT NỐI WI-FI & MQTT
// ============================================================
void connectWiFi() {
  if (WiFi.status() == WL_CONNECTED) return;
  Serial.printf("[WiFi] Ket noi %s", WIFI_SSID);
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  unsigned long start = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - start < 8000) {
    delay(300);
    Serial.print(".");
  }
  if (WiFi.status() == WL_CONNECTED) {
    Serial.printf("\n[WiFi] Da ket noi! IP: %s\n", WiFi.localIP().toString().c_str());
  } else {
    Serial.println("\n[WiFi] Khong the ket noi WiFi!");
  }
}

bool connectMQTT() {
  if (mqttClient.connected()) return true;
  if (WiFi.status() != WL_CONNECTED) return false;

  Serial.print("[MQTT] Dang ket noi toi Broker... ");
  // Last Will and Testament: Báo offline nếu thiết bị mất nguồn đột ngột
  bool connected = false;
  if (strlen(MQTT_USER) > 0) {
    connected = mqttClient.connect(DEVICE_ID, MQTT_USER, MQTT_PASSWORD, TOPIC_STATUS, 1, true, "offline");
  } else {
    connected = mqttClient.connect(DEVICE_ID, TOPIC_STATUS, 1, true, "offline");
  }

  if (connected) {
    Serial.println("Thanh cong!");
    mqttClient.publish(TOPIC_STATUS, "online", true); // Retain status
    mqttClient.subscribe(TOPIC_CALIBRATE);
    Serial.printf("[MQTT] Da subscribe: %s\n", TOPIC_CALIBRATE);
    return true;
  } else {
    Serial.printf("That bai, rc=%d\n", mqttClient.state());
    return false;
  }
}

// ============================================================
// SETUP & LOOP
// ============================================================
void setup() {
  Serial.begin(115200);
  delay(300);

  // Tạo topic chuẩn theo DEVICE_ID
  snprintf(TOPIC_TELEMETRY, sizeof(TOPIC_TELEMETRY), "water/device/%s/telemetry", DEVICE_ID);
  snprintf(TOPIC_CALIBRATE, sizeof(TOPIC_CALIBRATE), "water/device/%s/calibrate", DEVICE_ID);
  snprintf(TOPIC_STATUS,    sizeof(TOPIC_STATUS),    "water/device/%s/status",    DEVICE_ID);

  loadCalibration();

  pinMode(TDS_POWER_PIN, OUTPUT);
  digitalWrite(TDS_POWER_PIN, LOW);

  analogSetPinAttenuation(PH_PIN,  ADC_11db);
  analogSetPinAttenuation(TDS_PIN, ADC_11db);

  sensors.begin();

  mqttClient.setServer(MQTT_BROKER, MQTT_PORT);
  mqttClient.setCallback(mqttCallback);

  connectWiFi();
  connectMQTT();

  Serial.println("=== WaterSensor Prototype MQTT San Sang ===");
}

void loop() {
  // Duy trì kết nối MQTT và lắng nghe gói tin đến
  if (WiFi.status() != WL_CONNECTED) {
    connectWiFi();
  } else if (!mqttClient.connected()) {
    unsigned long now = millis();
    if (now - lastReconnectAttempt > 5000) {
      lastReconnectAttempt = now;
      connectMQTT();
    }
  } else {
    mqttClient.loop();
  }

  // Chu kỳ đọc cảm biến và phát MQTT
  unsigned long now = millis();
  if (now - lastReadTime >= READ_INTERVAL) {
    lastReadTime = now;

    // 1. Đọc nhiệt độ
    sensors.requestTemperatures();
    float t = sensors.getTempCByIndex(0);
    if (t != DEVICE_DISCONNECTED_C) {
      temperature = t + tempOffset;
    }

    // 2. Đọc TDS
    tdsValue = readTDS(temperature);

    // 3. Nghỉ 800ms khử điện tích rồi đọc pH
    delay(800);
    phValue = readPH();

    // 4. In Serial Monitor
    Serial.printf("[Sensor] T: %.1f C | pH: %.2f | TDS: %.0f ppm\n",
                  temperature, phValue, tdsValue);

    // 5. Đóng gói JSON & Publish MQTT
    if (mqttClient.connected()) {
      char payload[160];
      snprintf(payload, sizeof(payload),
               "{\"deviceId\":\"%s\",\"temperature\":%.2f,\"ph\":%.2f,\"tds\":%.1f,\"rssi\":%d}",
               DEVICE_ID, temperature, phValue, tdsValue, WiFi.RSSI());
      mqttClient.publish(TOPIC_TELEMETRY, payload);
      Serial.printf("[MQTT] Publish -> %s: %s\n", TOPIC_TELEMETRY, payload);
    }
  }
}
