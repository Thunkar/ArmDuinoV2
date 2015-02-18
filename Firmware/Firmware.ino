#include <Servo.h>

//Servo definitions
Servo base;
Servo horizontal1;
Servo vertical1;
Servo horizontal2;
Servo vertical2;
Servo horizontal3;
Servo gripper;
//Servo array
Servo servos[] = {base, horizontal1, vertical1, horizontal2, vertical2, horizontal3, gripper  };
//Instant positions of the servos
int positions[] = {90,90,90,90,90,90,170};
//Target position that servos have to reach
int targets[] = {90,90,90,90,90,90,170};
//Indicates wether the servos have reached their targets
boolean movementStatus[] = {true, true, true, true, true, true, true};
//Store the time each servo has to move a step
long stepTimer[7];
//Stores last time a servo performed a step
long lastSteps[7];
//Global time the arm has to complete the next target
long movementPeriod = 500000;


const int FIELD_COUNT = 7;
const int FIELD_SIZE = 3;

int incomingByte;
int readCounter;
char buffer[FIELD_COUNT*FIELD_SIZE+3];
int data[] = {90,90,90,90,90,90,170};
int multiplier[] = {1, 10, 100, 1000, 10000};
boolean reading;

boolean robotConnected;

void sendStatus()
{
  if(!robotConnected)
    Serial.println("STATUS: IDLE");
  else
  {
    Serial.print("STATUS: CONNECTED. FIELD_COUNT: ");
    Serial.print(FIELD_COUNT); 
    Serial.print(" FIELD_SIZE: ");
    Serial.print(FIELD_SIZE);
    Serial.print(" POSITIONS: ");
    for(int i = 0; i < FIELD_COUNT; i++)
    {
      Serial.print(data[i]);
      Serial.print(" ");
    }
    
    Serial.print(" DATA: ");
    for(int i = 0; i < FIELD_COUNT; i++)
    {
      Serial.print(data[i]);
      Serial.print(" ");
    }
    Serial.println("AWAITING COMMANDS");
  }
}

/*
/ This method takes the data array and sets the target position for each servo. Also, it assigns each servo the time it has to move.
*/
void setTargets(int data[]){
  targets[0] = data[0];
  targets[1] = data[1];
  targets[2] = data[2];
  targets[3] = 180- data[3]; //Note that this servo is reversed by design
  targets[4] = data[4];
  targets[5] = data[5];
  targets[6] = data[6];
  for(int i = 0; i < FIELD_COUNT; i++)
  {
    int difference = abs(positions[i]-targets[i]);
    if(difference == 0){
      movementStatus[i] = true;
    }
    else{
      movementStatus[i] = false;
      stepTimer[i] = movementPeriod/difference;
    }
  }
}

/*
/ Checks if each servo has to move and moves it. Also, it stores the new position and sets the last time it moved to the present.
*/


void moveStep(int servo, int target){
  if(movementStatus[servo] == true) return; //We are already there? Done!
  if((micros()-lastSteps[servo])>=stepTimer[servo]) //Has enough time passed already?
  {
    if(positions[servo]<target){
      positions[servo] += 1;
      servos[servo].write(positions[servo]);
      lastSteps[servo] = micros();
    }
    else{
      positions[servo] -= 1;
      servos[servo].write(positions[servo]);
      lastSteps[servo] = micros();
    }
  }
  if(positions[servo] == target) movementStatus[servo] = true;
}

/*
/ Loop to move all the servos
*/

void moveSegments(){
    for(int i = 0; i < 6; i++)
    {
      moveStep(i, targets[i]);
    }
    servos[6].write(targets[6]); // Instantly move gripper
} 

/*
/ Checks if the arm is receiving its actual target as data.
*/

boolean sameTargets(int data[]){
  return
  targets[0] == data[0] &
  targets[1] == data[1] &
  targets[2] == data[2] &
  targets[3] == 180- data[3] & 
  targets[4] == data[4] &
  targets[5] == data[5] &
  targets[6] == data[6];
}

/*
/ Checks if the data we are going to process is the same that we have and in that case ignores it. If it's different, time to move
*/

void moveStuff()
{
  if(!sameTargets(data))
  {
      setTargets(data);
  }
  moveSegments();
}


void clearData()
{
  for(int i = 0; i < FIELD_COUNT; i++)
  {
    data[i] = 0;
  }
}



void processMovementData()
{
  if(!robotConnected)
  {
    Serial.println("Robot not connected!");
    return;
  }
  int counter = 0;
  clearData();
  for(int i = 3; i < FIELD_COUNT*FIELD_SIZE+3; i = i + FIELD_SIZE)
  {
      for(int j = 0; j < FIELD_SIZE; j ++)
      {
        data[counter] += ((int)buffer[i+j]-0x30)*multiplier[FIELD_SIZE - j -1];
      }
      counter++;
  }
}


void dumpInputBuffer()
{
  switch(buffer[0])
  {
    case 'M':
      processMovementData();
      return;
    case 'C':
      robotConnected = true;
      digitalWrite(13, LOW);
      gripper.write(170);
      delay(200);
      gripper.write(90);
      delay(200);
      gripper.write(170);
      Serial.println("Connected");
      return;
    case 'R':
      robotConnected = false;
      reset();
      Serial.println("Disconnected");
      return;
    case 'S':
      sendStatus();
      return;
    default:
     return; 
  }
}

/*
/ Resets the arm to the waiting for connection state
*/

void reset()
{
    digitalWrite(13, HIGH);
    for(int i = 0; i < 6; i++) 
    {
      positions[i] = 90;
      targets[i] = 90;
      data[i] = 90;
    }
    data[6] = 170;
    positions[6] = 170;
    targets[6] = 170;
    gripper.write(170);
    delay(500);
    base.write(90);
    delay(500);
    vertical1.write(90);
    delay(500);
    vertical2.write(90);
    delay(500);
    horizontal1.write(90);
    delay(500);
    horizontal2.write(90);
    delay(500);
    horizontal3.write(90);
    // gripper signal to indicate we are ready to receive stuff
    delay(300);
    gripper.write(90);
    delay(200);
    gripper.write(170);
}

void readData()
{
    incomingByte = Serial.read();
  if(incomingByte != -1) 
  {
    if(!reading)
    {
      if((char)incomingByte =='&')
      {
        reading = true;
      }
    }
    else if((char)incomingByte == '%')
    {
      reading = false;
      dumpInputBuffer();
      readCounter = 0;
    }
    else
    {
      buffer[readCounter] = (char)incomingByte;
      readCounter++;
    }
  }
}


void setup()
{
    pinMode(13, OUTPUT);
    digitalWrite(13, HIGH); //LED on means the arm is not connected
    gripper.attach(6);
    gripper.write(170);
    delay(500);
    base.attach(7);
    delay(500);
    vertical1.attach(5);
    delay(500);
    vertical2.attach(9);
    delay(500);
    horizontal1.attach(12);
    delay(500);
    horizontal2.attach(10);
    delay(500);
    horizontal3.attach(8);
    // gripper signal to indicate we are ready to receive stuff
    delay(300);
    gripper.write(90);
    delay(200);
    gripper.write(170);
    Serial.begin(115200);
}


void loop()
{
  readData();
  moveStuff();
}
