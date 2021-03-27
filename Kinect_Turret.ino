#include <Servo.h>

#define SERVOX 9
#define SERVOY 10
#define RELAY 13

char incomingByte = 0;
Servo servoX;  // create servo object to control a servo
Servo servoY;  // create servo object to control a servo

int posX = 0;    // variable to store the servoX position
int posY = 0;    // variable to store the servoY position

int mode = -1; // -1 = none, 0 = x input, 1 = y input
bool firing = false;

const byte numChars = 32;
char receivedChars[numChars];

boolean newData = false;

void setup()
{
  servoX.attach(SERVOX);  // attaches the servo on pin 9 to the servo object
  servoY.attach(SERVOY);  // attaches the servo on pin 10 to the servo object
  pinMode(RELAY, OUTPUT);
  Serial.begin(9600); // opens serial port, sets data rate to 9600 bps
}

void loop()
{
  ReceiveSerial();
  if (!newData)
    return;
  //showNewData(); //Uncomment to print all received data.
  newData = false;
  int oldPosX = posX;
  int oldPosY = posY;
  ProcessNewData();
  //Print Servo Positions if they have changed:
  if (posX != oldPosX)
  {
    Serial.print("Setting ServoX To ");
    Serial.println(posX, DEC);
  }
  if (posY != oldPosY)
  {
    Serial.print("Setting ServoY To ");
    Serial.println(posY, DEC);
  }
  //Clamp Servo Positions:
  if (posX > 180)
    posX = 180;
  if (posX < 0)
    posX = 0;

  if (posY > 180)
    posY = 180;
  if (posY < 0)
    posY = 0;

  WriteData();
}

void WriteData()
{
  //Write Servo and Relay Data.
  servoX.write(posX);  // tell servo to go to position in variable
  servoY.write(posY);  // tell servo to go to position in variable
  if (firing)
    digitalWrite(RELAY, HIGH);
  else
    digitalWrite(RELAY, LOW);
}

void ProcessNewData()
{
  int i = 0;
  while (receivedChars[i] != '\0')
  {
    // read the incoming byte:
    switch (mode)
    {
      case -1: //Default mode
        if (receivedChars[i] == 'H') //About to receive Horizontal Angle, switch to horizontal mode
        {
          mode = 0;
        }
        else if (receivedChars[i] == 'V') //About to receive Vertical Angle, switch to vertical mode
        {
          mode = 1;
        }
        else if (receivedChars[i] == 'F') //Begin Firing
        {
          firing = true;
        }
        else if (receivedChars[i] == 'S') //Stop Firing
        {
          firing = false;
        }
        break;
      case 0: //Horizontal Angle Mode, set angle based on received byte.
        posX = (int)receivedChars[i];
        mode = -1;
        break;
      case 1: //Vertical Angle Mode, set angle based on received byte.
        posY = (int)receivedChars[i];
        mode = -1;
        break;
    }
    i++;
  }
}

void ReceiveSerial() {
  static boolean recvInProgress = false;
  static byte ndx = 0;
  char rc;

  while (Serial.available() > 0 && newData == false)
  {
    rc = Serial.read();

    receivedChars[ndx] = rc;
    ndx++;
    if (ndx >= numChars)
    {
      ndx = numChars - 1;
    }
  }

  if (ndx > 0)
  {
    receivedChars[ndx] = '\0'; // terminate the string
    ndx = 0;
    newData = true;
  }
}

void showNewData() {
  if (newData == true) {
    Serial.print("Arduino Received Data: ");
    Serial.println(receivedChars);
    newData = false;
  }
}
