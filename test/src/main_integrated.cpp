/*
 * WaterSensor v3.0 / v4.0 — Firmware cho bo mạch tích hợp
 * Bo gộp: ESP32-WROOM-32 + MCP6002 (pH op-amp) + DS3231 (RTC)
 *         + ST7789 LCD SPI + DS18B20 + TDS cầu phân áp + BNC pH
 *
 * TÍNH NĂNG CHÍNH:
 *   - Đo chu kỳ: Temp (DS18B20) -> TDS (kích xung Q1) -> nghỉ 200ms -> pH (MCP6002) -> Pin LiPo
 *   - Bù nhiệt độ tự động cho pH (phương trình Nernst) và TDS (hệ số 2%/°C)
 *   - Kết nối Wi-Fi phát dữ liệu telemetry JSON qua HTTP POST
 *   - DS3231 RTC hẹn giờ đánh thức chính xác bằng ngắt phần cứng ~INT/SQW
 *   - ESP32 Deep Sleep (ext0 wake-up trên GPIO33, mức LOW) tiết kiệm tối đa pin
 *
 * Pinout theo schematic WaterSensor.kicad_sch:
 *   GPIO14        = GPS_RX          (Pin 13, ESP32 RX <- GPS TXD, NEO-M8N / ATGM336H)
 *   GPIO13        = GPS_TX          (Pin 16, ESP32 TX -> GPS RXD)
 *   GPIO34        = PH_ADC          (Pin 6, input-only ADC1_CH6, qua R_PH_FILT + C_PH_FILT)
 *   GPIO35        = TDS_ADC         (Pin 7, input-only ADC1_CH7, qua R_TDS_FILT + C_TDS_FILT)
 *   GPIO39 (SVN)  = BAT_MONITOR     (Pin 5, input-only ADC1_CH3, cầu chia áp pin 100k/100k)
 *   GPIO32        = LCD_BL_PWM      (Pin 8, LEDC PWM / GPIO kích gate Q2 đèn nền)
 *   GPIO33        = RTC_INT         (Pin 9, RTC_GPIO8, ext0 deep sleep wake-up từ DS3231 ~INT)
 *   GPIO26        = TDS_PWR_CTRL    (Pin 11, output gate Q1 2N7002 qua R_TDS_GATE 1k)
 *   GPIO4         = ONEWIRE_TEMP    (Pin 26, DS18B20 1-Wire)
 *   GPIO5         = LCD_CS          (Pin 29, SPI Chip Select LCD)
 *   GPIO16        = LCD_DC          (Pin 27, Data/Command LCD)
 *   GPIO17        = LCD_RST         (Pin 28, Reset LCD)
 *   GPIO18        = SPI_SCK         (Pin 30, SPI Clock)
 *   GPIO23        = SPI_MOSI        (Pin 37, SPI Data Out)
 *   GPIO21        = I2C_SDA         (Pin 33, DS3231 SDA)
 *   GPIO22        = I2C_SCL         (Pin 36, DS3231 SCL)
 */

#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <Preferences.h>
#include <Wire.h>
#include <SPI.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <esp_sleep.h>
#include <driver/gpio.h>

// ADC Attenuation macro đa phiên bản Arduino-ESP32 (v2 / v3)
#if defined(ADC_ATTEN_DB_12)
  #define BOARD_ADC_ATTEN ADC_ATTEN_DB_12
#elif defined(ADC_ATTEN_DB_11)
  #define BOARD_ADC_ATTEN ADC_ATTEN_DB_11
#elif defined(ADC_11db)
  #define BOARD_ADC_ATTEN ADC_11db
#else
  #define BOARD_ADC_ATTEN (adc_attenuation_t)3
#endif

// ============================================================
// CẤU HÌNH WI-FI & MQTT BROKER
// ============================================================
const char* WIFI_SSID       = "YOUR_WIFI_SSID";
const char* WIFI_PASSWORD   = "YOUR_WIFI_PASSWORD";

// Địa chỉ Broker MQTT (NAS hoặc máy local LAN, vd: 192.168.1.150)
const char* MQTT_BROKER     = "192.168.1.150";
const uint16_t MQTT_PORT    = 1883;
const char* MQTT_USER       = "";
const char* MQTT_PASSWORD   = "";
const char* DEVICE_ID       = "ESP32_01";

const uint32_t WIFI_TIMEOUT_MS = 10000; // 10s timeout nếu không có Wi-Fi

// Chu kỳ ngủ Deep Sleep (phút)
const uint8_t SLEEP_INTERVAL_MIN = 5;

// ============================================================
// GPIO PINOUT — Khớp 100% WaterSensor.kicad_sch
// ============================================================
#define GPS_RX_PIN        14   // GPIO14 (Pin 13) — ESP32 RX <- GPS TXD
#define GPS_TX_PIN        13   // GPIO13 (Pin 16) — ESP32 TX -> GPS RXD
#define GPS_BAUD_RATE     9600

#define PH_ADC_PIN        34   // GPIO34 (Pin 6)  — Input-only ADC1_CH6
#define TDS_ADC_PIN       35   // GPIO35 (Pin 7)  — Input-only ADC1_CH7
#define BAT_MON_PIN       39   // GPIO39 (Pin 5)  — Input-only ADC1_CH3 (SVN)
#define LCD_BL_PIN        32   // GPIO32 (Pin 8)  — Output/PWM gate Q2 đèn nền
#define RTC_INT_PIN       33   // GPIO33 (Pin 9)  — RTC_GPIO8 ext0 wakeup
#define TDS_PWR_PIN       26   // GPIO26 (Pin 11) — Gate Q1 cấp nguồn que TDS
#define ONE_WIRE_PIN       4   // GPIO4  (Pin 26) — 1-Wire DS18B20

#define LCD_CS_PIN         5   // GPIO5
#define LCD_DC_PIN        16   // GPIO16
#define LCD_RST_PIN       17   // GPIO17
#define SPI_SCK_PIN       18   // GPIO18
#define SPI_MOSI_PIN      23   // GPIO23

#define I2C_SDA_PIN       21   // GPIO21
#define I2C_SCL_PIN       22   // GPIO22

// DS3231 I2C
#define DS3231_I2C_ADDR   0x68

// ============================================================
// THÔNG SỐ HIỆU CHUẨN CẢM BIẾN
// ============================================================
const float PH_OFFSET_DEFAULT = 2.50f;   // Điện áp tại pH 7.0 (chỉnh qua biến trở RV1)
const float R_TDS_REF         = 1000.0f; // Trở kéo lên R_TDS_REF 1k 1%
const float V_SUPPLY          = 3.30f;
const float TDS_CELL_K        = 1.0f;    // Hệ số que đo (cm^-1)
const float BAT_DIVIDER_RATIO = 2.0f;    // Cầu chia áp 100k / 100k

const unsigned long TDS_SETTLE_MS = 50;  // Chờ ổn định dòng qua que TDS
const unsigned long PH_SETTLE_MS  = 200; // Nghỉ để khử điện tích dư trong nước

// ============================================================
// KHỞI TẠO ĐỐI TƯỢNG NGOẠI VI
// ============================================================
OneWire oneWire(ONE_WIRE_PIN);
DallasTemperature ds18b20(&oneWire);
HardwareSerial gpsSerial(2);

// Biến lưu kết quả đo
float temperature = 25.0f;
float phValue     = 7.0f;
float tdsValue    = 0.0f;
float batVoltage  = 0.0f;
float gpsLat      = 0.0f;
float gpsLon      = 0.0f;
bool  gpsFix      = false;

// Topic quy chuẩn MQTT
char TOPIC_TELEMETRY[64];
char TOPIC_CALIBRATE[64];
char TOPIC_STATUS[64];

WiFiClient espClient;
PubSubClient mqttClient(espClient);

// Thông số hiệu chuẩn nhận từ Desktop UI qua MQTT
float phOffset    = 0.00f;
float tdsFactor   = 1.00f;
float tempOffset  = 0.00f;

Preferences prefs;

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

void mqttCallback(char* topic, byte* message, unsigned int length) {
  char payload[160];
  if (length >= sizeof(payload)) length = sizeof(payload) - 1;
  memcpy(payload, message, length);
  payload[length] = '\0';
  Serial.printf("[MQTT] Nhan topic %s: %s\n", topic, payload);

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
}

// ============================================================
// DS3231 NATIVE I2C DRIVER (Không phụ thuộc thư viện ngoài)
// ============================================================
uint8_t decToBcd(uint8_t val) { return ((val / 10 * 16) + (val % 10)); }
uint8_t bcdToDec(uint8_t val) { return ((val / 16 * 10) + (val % 16)); }

void ds3231_clear_alarm_flags() {
  Wire.beginTransmission(DS3231_I2C_ADDR);
  Wire.write(0x0F); // Status register
  Wire.endTransmission();
  Wire.requestFrom(DS3231_I2C_ADDR, 1);
  if (Wire.available()) {
    uint8_t status = Wire.read();
    status &= ~0x03; // Xóa cờ A1F (bit 0) và A2F (bit 1) để chân ~INT kéo lên HIGH
    Wire.beginTransmission(DS3231_I2C_ADDR);
    Wire.write(0x0F);
    Wire.write(status);
    Wire.endTransmission();
  }
}

void ds3231_schedule_alarm(uint8_t minutes_ahead) {
  if (minutes_ahead == 0) minutes_ahead = 1;
  if (minutes_ahead > 59) minutes_ahead = 59; // Gioi han de khong tran gio khi chi so khop phut

  // Đọc phút hiện tại từ thanh ghi 0x01
  Wire.beginTransmission(DS3231_I2C_ADDR);
  Wire.write(0x01);
  Wire.endTransmission();
  Wire.requestFrom(DS3231_I2C_ADDR, 1);
  uint8_t current_min = 0;
  if (Wire.available()) {
    current_min = bcdToDec(Wire.read() & 0x7F);
  }

  uint8_t alarm_min = (current_min + minutes_ahead) % 60;

  // Cấu hình Alarm 1:
  // Reg 0x07: Giây = 00, A1M1 = 0 (khớp giây 00)
  // Reg 0x08: Phút = alarm_min, A1M2 = 0 (khớp phút alarm_min)
  // Reg 0x09: A1M3 = 1 (bỏ qua giờ, khớp mọi giờ)
  // Reg 0x0A: A1M4 = 1 (bỏ qua ngày, khớp mọi ngày)
  Wire.beginTransmission(DS3231_I2C_ADDR);
  Wire.write(0x07);
  Wire.write(decToBcd(0));               // 00 sec (A1M1 = 0)
  Wire.write(decToBcd(alarm_min));       // alarm_min (A1M2 = 0)
  Wire.write(0x80);                      // A1M3 = 1
  Wire.write(0x80);                      // A1M4 = 1
  Wire.endTransmission();

  // Bật ngắt Alarm 1 trên chân ~INT/SQW:
  // Control Register (0x0E): INTCN = 1 (bit 2), A1IE = 1 (bit 0), A2IE = 0 (bit 1)
  Wire.beginTransmission(DS3231_I2C_ADDR);
  Wire.write(0x0E);
  Wire.endTransmission();
  Wire.requestFrom(DS3231_I2C_ADDR, 1);
  uint8_t ctrl = 0x1C; // Giá trị mặc định
  if (Wire.available()) {
    ctrl = Wire.read();
  }
  ctrl |= 0x05;  // INTCN=1, A1IE=1
  ctrl &= ~0x02; // A2IE=0
  Wire.beginTransmission(DS3231_I2C_ADDR);
  Wire.write(0x0E);
  Wire.write(ctrl);
  Wire.endTransmission();

  // Xóa cờ ngắt tồn đọng
  ds3231_clear_alarm_flags();
  Serial.printf("[RTC] Hen gio thuc day sau %d phut (vao phut %02d:00)\n",
                minutes_ahead, alarm_min);
}

// ============================================================
// HÀM ĐO ĐẠC CẢM BIẾN
// ============================================================
float readTemperature() {
  ds18b20.requestTemperatures();
  float t = ds18b20.getTempCByIndex(0);
  if (t != DEVICE_DISCONNECTED_C && t > -50.0f && t < 125.0f) {
    return t + tempOffset;
  }
  return 25.0f + tempOffset; // Giá trị fallback nếu mất cảm biến
}

float readTDS(float temp) {
  // 1. Kích Q1 mở mass cho que đo
  digitalWrite(TDS_PWR_PIN, HIGH);
  delay(TDS_SETTLE_MS);

  // 2. Lấy mẫu ADC (20 mẫu trung bình)
  long sum = 0;
  for (int i = 0; i < 20; i++) {
    sum += analogReadMilliVolts(TDS_ADC_PIN);
    delay(2);
  }

  // 3. Tắt Q1 NGAY LẬP TỨC — chống phân cực & khử dòng rò sang đầu dò pH
  digitalWrite(TDS_PWR_PIN, LOW);

  float vSense = (sum / 20.0f) / 1000.0f;

  if (vSense >= (V_SUPPLY - 0.02f)) return 0.0f; // Que hở trong không khí
  if (vSense <= 0.01f) return 9999.0f;           // Ngắn mạch đầu dò

  float rWater = R_TDS_REF * vSense / (V_SUPPLY - vSense);
  float tempCoeff = 1.0f + 0.02f * (temp - 25.0f); // Bù nhiệt 2%/°C
  float ec_25 = ((TDS_CELL_K * 1.0e6f) / rWater) / tempCoeff;
  float tds = ec_25 * 0.5f;
  if (tds < 0.0f) tds = 0.0f;

  return tds * tdsFactor;
}

float readPH(float temp) {
  // Lấy 40 mẫu trung bình từ buffer MCP6002
  long sum = 0;
  for (int i = 0; i < 40; i++) {
    sum += analogReadMilliVolts(PH_ADC_PIN);
    delay(5);
  }
  float voltage = (sum / 40.0f) / 1000.0f;

  // Phương trình Nernst bù nhiệt: slope = 59.16mV * (273.15 + T) / 298.15
  float slope_V = (59.16f * (273.15f + temp) / 298.15f) / 1000.0f;
  float ph = 7.0f + (PH_OFFSET_DEFAULT - voltage) / slope_V;
  ph += phOffset;

  if (ph < 0.0f)  ph = 0.0f;
  if (ph > 14.0f) ph = 14.0f;

  return ph;
}

float readBattery() {
  long sum = 0;
  for (int i = 0; i < 10; i++) {
    sum += analogReadMilliVolts(BAT_MON_PIN);
    delay(2);
  }
  float vAdc = (sum / 10.0f) / 1000.0f;
  float vBat = vAdc * BAT_DIVIDER_RATIO;
  if (vBat < 0.0f) vBat = 0.0f;
  if (vBat > 5.0f) vBat = 5.0f;
  return vBat;
}

// ponytail: Zero-alloc NMEA parser for $GNGGA/$GPRMC. Timeout 2.5s. Upgrade: TinyGPS++ if HDOP/sat count required.
bool readGPS(float &lat, float &lon, uint32_t timeout_ms = 2500) {
  gpsSerial.begin(GPS_BAUD_RATE, SERIAL_8N1, GPS_RX_PIN, GPS_TX_PIN);
  unsigned long start = millis();
  char line[96];
  uint8_t idx = 0;

  while (millis() - start < timeout_ms) {
    while (gpsSerial.available()) {
      char c = (char)gpsSerial.read();
      if (c == '\r') continue;
      if (c == '\n') {
        line[idx] = '\0';
        idx = 0;
        if (strncmp(line, "$G", 2) == 0 && (strstr(line, "RMC") || strstr(line, "GGA"))) {
          char* p = line;
          int f = 0;
          char* tokens[10];
          while (p && f < 10) {
            tokens[f++] = p;
            p = strchr(p, ',');
            if (p) *p++ = '\0';
          }
          if (strstr(tokens[0], "GGA") && f >= 7 && tokens[6] && tokens[6][0] > '0') {
            float rawLat = atof(tokens[2]);
            float rawLon = atof(tokens[4]);
            int dLat = (int)(rawLat / 100);
            int dLon = (int)(rawLon / 100);
            lat = dLat + (rawLat - dLat * 100.0f) / 60.0f;
            lon = dLon + (rawLon - dLon * 100.0f) / 60.0f;
            if (tokens[3][0] == 'S') lat = -lat;
            if (tokens[5][0] == 'W') lon = -lon;
            gpsSerial.end();
            return true;
          } else if (strstr(tokens[0], "RMC") && f >= 7 && tokens[2] && tokens[2][0] == 'A') {
            float rawLat = atof(tokens[3]);
            float rawLon = atof(tokens[5]);
            int dLat = (int)(rawLat / 100);
            int dLon = (int)(rawLon / 100);
            lat = dLat + (rawLat - dLat * 100.0f) / 60.0f;
            lon = dLon + (rawLon - dLon * 100.0f) / 60.0f;
            if (tokens[4][0] == 'S') lat = -lat;
            if (tokens[6][0] == 'W') lon = -lon;
            gpsSerial.end();
            return true;
          }
        }
      } else if (idx < sizeof(line) - 1) {
        line[idx++] = c;
      }
    }
    delay(10);
  }
  gpsSerial.end();
  return false;
}

// ============================================================
// TRUYỀN DỮ LIỆU TELEMETRY QUA MQTT (KẾT NỐI -> PHÁT -> NGẮT)
// ============================================================
bool sendTelemetry(float temp, float ph, float tds, float bat, float lat, float lon, bool hasFix) {
  Serial.printf("[Wi-Fi] Ket noi SSID: %s ...\n", WIFI_SSID);
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  unsigned long start = millis();
  while (WiFi.status() != WL_CONNECTED && (millis() - start < WIFI_TIMEOUT_MS)) {
    delay(200);
    Serial.print(".");
  }
  Serial.println();

  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("[Wi-Fi] Ket noi that bai! Bo qua truyen du lieu.");
    WiFi.disconnect(true);
    WiFi.mode(WIFI_OFF);
    return false;
  }

  Serial.printf("[Wi-Fi] Da ket noi! IP: %s | RSSI: %d dBm\n",
                WiFi.localIP().toString().c_str(), WiFi.RSSI());

  // Cấu hình MQTT Client
  mqttClient.setServer(MQTT_BROKER, MQTT_PORT);
  mqttClient.setCallback(mqttCallback);

  Serial.print("[MQTT] Dang ket noi toi Broker... ");
  bool connected = false;
  if (strlen(MQTT_USER) > 0) {
    connected = mqttClient.connect(DEVICE_ID, MQTT_USER, MQTT_PASSWORD, TOPIC_STATUS, 1, true, "offline");
  } else {
    connected = mqttClient.connect(DEVICE_ID, TOPIC_STATUS, 1, true, "offline");
  }

  if (!connected) {
    Serial.printf("That bai, rc=%d. Ngat Wi-Fi.\n", mqttClient.state());
    WiFi.disconnect(true);
    WiFi.mode(WIFI_OFF);
    return false;
  }

  Serial.println("Thanh cong!");
  mqttClient.publish(TOPIC_STATUS, "online", true);

  // Subscribe kênh hiệu chuẩn để nhận các cấu hình được gửi từ Desktop UI (Retained message)
  mqttClient.subscribe(TOPIC_CALIBRATE);
  unsigned long subStart = millis();
  while (millis() - subStart < 400) {
    mqttClient.loop();
    delay(20);
  }

  // Đóng gói JSON telemetry
  char payload[220];
  if (hasFix) {
    snprintf(payload, sizeof(payload),
             "{\"deviceId\":\"%s\",\"temperature\":%.2f,\"ph\":%.2f,\"tds\":%.1f,\"battery\":%.2f,\"lat\":%.6f,\"lon\":%.6f,\"rssi\":%d}",
             DEVICE_ID, temp, ph, tds, bat, lat, lon, WiFi.RSSI());
  } else {
    snprintf(payload, sizeof(payload),
             "{\"deviceId\":\"%s\",\"temperature\":%.2f,\"ph\":%.2f,\"tds\":%.1f,\"battery\":%.2f,\"lat\":null,\"lon\":null,\"rssi\":%d}",
             DEVICE_ID, temp, ph, tds, bat, WiFi.RSSI());
  }

  Serial.printf("[MQTT] Publish -> %s: %s\n", TOPIC_TELEMETRY, payload);
  bool pubSuccess = mqttClient.publish(TOPIC_TELEMETRY, payload);
  mqttClient.loop();
  delay(100);

  mqttClient.disconnect();

  // Ngắt Wi-Fi và tắt RF hoàn toàn để chuẩn bị vào Deep Sleep
  WiFi.disconnect(true);
  WiFi.mode(WIFI_OFF);
  return pubSuccess;
}

// ============================================================
// SETUP — Chu trình thức dậy -> Đo -> Phát -> Ngủ
// ============================================================
void setup() {
  Serial.begin(115200);
  delay(100);

  esp_sleep_wakeup_cause_t wakeup_reason = esp_sleep_get_wakeup_cause();
  if (wakeup_reason == ESP_SLEEP_WAKEUP_EXT0) {
    Serial.println("\n>>> WAKE UP tu DS3231 RTC Alarm (ext0 GPIO33) <<<");
  } else {
    Serial.println("\n>>> BOOT UP lan dau (Power-on / Reset) <<<");
  }

  // Gỡ chốt GPIO hold sau khi thức dậy
  gpio_hold_dis((gpio_num_t)TDS_PWR_PIN);
  gpio_hold_dis((gpio_num_t)LCD_BL_PIN);

  // Cấu hình GPIO
  pinMode(TDS_PWR_PIN, OUTPUT);
  digitalWrite(TDS_PWR_PIN, LOW); // Đảm bảo TDS luôn TẮT khi khởi động

  pinMode(LCD_BL_PIN, OUTPUT);
  digitalWrite(LCD_BL_PIN, LOW);  // Tắt đèn nền tiết kiệm pin

  // Chân ngắt RTC có trở kéo lên ngoài 10k R_RTC_PU
  pinMode(RTC_INT_PIN, INPUT_PULLUP);

  // ADC Attenuation — Full range 0 - 3.3V
  analogSetPinAttenuation(PH_ADC_PIN,  BOARD_ADC_ATTEN);
  analogSetPinAttenuation(TDS_ADC_PIN, BOARD_ADC_ATTEN);
  analogSetPinAttenuation(BAT_MON_PIN, BOARD_ADC_ATTEN);

  // Khởi động I2C cho DS3231
  Wire.begin(I2C_SDA_PIN, I2C_SCL_PIN);

  // Khởi động 1-Wire DS18B20
  ds18b20.begin();

  // Khởi tạo topic MQTT & tải thông số hiệu chuẩn từ NVS
  snprintf(TOPIC_TELEMETRY, sizeof(TOPIC_TELEMETRY), "water/device/%s/telemetry", DEVICE_ID);
  snprintf(TOPIC_CALIBRATE, sizeof(TOPIC_CALIBRATE), "water/device/%s/calibrate", DEVICE_ID);
  snprintf(TOPIC_STATUS,    sizeof(TOPIC_STATUS),    "water/device/%s/status",    DEVICE_ID);
  loadCalibration();

  // ==========================================
  // BƯỚC 1: ĐỌC DỮ LIỆU CẢM BIẾN
  // ==========================================
  // 1.1 Nhiệt độ
  temperature = readTemperature();

  // 1.2 TDS (kích xung Q1 -> đọc -> ngắt Q1)
  tdsValue = readTDS(temperature);

  // 1.3 Nghỉ khử điện tích dư trong nước trước khi đọc pH
  delay(PH_SETTLE_MS);

  // 1.4 pH (đọc qua MCP6002 op-amp buffer)
  phValue = readPH(temperature);

  // 1.5 GPS GNSS (u-blox NEO-M8N / ATGM336H qua Serial2 GPIO14/GPIO13)
  gpsFix = readGPS(gpsLat, gpsLon, 2500);

  // 1.6 Điện áp pin LiPo
  batVoltage = readBattery();

  Serial.println("----------------------------------------");
  Serial.printf("Temp: %.1f °C  |  pH: %.2f  |  TDS: %.0f ppm  |  Bat: %.2f V  |  GPS: %s (%.5f, %.5f)\n",
                temperature, phValue, tdsValue, batVoltage, gpsFix ? "FIX" : "NO_FIX", gpsLat, gpsLon);
  Serial.println("----------------------------------------");

  // ==========================================
  // BƯỚC 2: PHÁT SÓNG WI-FI & GỬI DỮ LIỆU
  // ==========================================
  sendTelemetry(temperature, phValue, tdsValue, batVoltage, gpsLat, gpsLon, gpsFix);

  // ==========================================
  // BƯỚC 3: HẸN GIỜ RTC & VÀO DEEP SLEEP
  // ==========================================
  // Cài đặt Alarm DS3231 cho chu kỳ kế tiếp
  ds3231_schedule_alarm(SLEEP_INTERVAL_MIN);

  // Đảm bảo toàn bộ tải ngoại vi đã ngắt và chốt mức logic LOW khi ngủ
  digitalWrite(TDS_PWR_PIN, LOW);
  digitalWrite(LCD_BL_PIN, LOW);
  gpio_hold_en((gpio_num_t)TDS_PWR_PIN);
  gpio_hold_en((gpio_num_t)LCD_BL_PIN);
  gpio_deep_sleep_hold_en();

  // Bật ngắt ext0 trên GPIO33, kích hoạt khi mức LOW (DS3231 Open-Drain kéo LOW)
  esp_sleep_enable_ext0_wakeup((gpio_num_t)RTC_INT_PIN, 0);

  Serial.println("[Power] San sang vao Deep Sleep. Tam biet!");
  Serial.flush();

  // Vào Deep Sleep
  esp_deep_sleep_start();
}

void loop() {
  // Không bao giờ chạy đến loop() vì chip luôn sleep sau setup()
}
