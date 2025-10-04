#include <math.h>

#define CH1 3
#define CH2 5
#define CH3 6
#define CH4 9
#define CH5 10

// Read the number of a given channel and convert to the range provided.
// If the channel is off, return the default value
int readChannel(int channelInput, int minLimit, int maxLimit, int defaultValue){
  int ch = pulseIn(channelInput, HIGH, 30000);
  if (ch < 100) return defaultValue;
  return map(ch, 1000, 2000, minLimit, maxLimit);
}

// Read the channel and return a boolean value
bool redSwitch(byte channelInput, bool defaultValue){
  int intDefaultValue = (defaultValue)? 100: 0;
  int ch = readChannel(channelInput, 0, 100, intDefaultValue);
  return (ch > 50);
}

void setup(){
  Serial.begin(115200);
  pinMode(CH1, INPUT);
  pinMode(CH2, INPUT);
  pinMode(CH3, INPUT);
  pinMode(CH4, INPUT);
  pinMode(CH5, INPUT);
}

int yawValue, pitchValue, rollValue, ch4Value;
int cameraAngle;  // Ángulo de la cámara controlado por CH5

void loop() {
  yawValue = readChannel(CH1, -100, 100, 0);    // CH1 como Yaw
  pitchValue = readChannel(CH2, -100, 100, 0);  // CH2 como Pitch
  rollValue = readChannel(CH3, -100, 100, -100); // CH3 como Roll
  ch4Value = readChannel(CH4, -100, 100, 0);

  // CH5 como potenciómetro para ángulo de cámara (0 a 180 grados)
  cameraAngle = readChannel(CH5, 0, 180, 90);  // Valor por defecto 90 grados

  // Cálculo del ángulo de inclinación (roll) en grados usando yaw y pitch
  // Si quieres usar yaw y pitch para calcular roll, o simplemente mostrar los valores:
  // Aquí solo mostramos los valores leídos.

  Serial.print("Yaw (CH1): ");
  Serial.print(yawValue);
  Serial.print(" Pitch (CH2): ");
  Serial.print(pitchValue);
  Serial.print(" Roll (CH3): ");
  Serial.print(rollValue);
  Serial.print(" CH4: ");
  Serial.print(ch4Value);

  Serial.print(" | Ángulo cámara (CH5): ");
  Serial.print(cameraAngle);
  Serial.println(" grados");

  delay(500);
}