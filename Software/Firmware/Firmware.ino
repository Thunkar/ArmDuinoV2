#include <SoftPWM.h> //Soft pwm is used to control motors in analog outputs
#include <SoftPWM_timer.h>
#include <Servo.h>

const int FIELD_COUNT = 15;
const int FIELD_SIZE = 4;
const int SERVO_COUNT=11;

typedef struct motor{ //Describes motor values. In H-Bridges, two analog outputs are used, one for forward power and other for backward power.
  int forward;
  int backward;
} Motor;

//Servo definitions
Servo base;
Servo horizontal1;
Servo vertical1;
Servo horizontal2;
Servo vertical2;
Servo horizontal3;
Servo gripper;
Servo flAngle; //Front left rotation
Servo frAngle; //Front right rotation
Servo blAngle; //Back left rotation
Servo brAngle; //Back right rotation
Motor flSpeed; //Front left speed
Motor frSpeed; //Front right speed
Motor blSpeed; //Back left speed
Motor brSpeed; //Back right speed
//Servo array
Servo servos[] = {base, horizontal1, vertical1, horizontal2, vertical2, horizontal3, gripper, flAngle,frAngle,blAngle,brAngle  };
Motor motors[] = {flSpeed,frSpeed,blSpeed,brSpeed};
//Instant positions of the servos
int positions[] = {90,90,90,90,90,90,170,90,90,90,90,0,0,0,0};
//Target position that servos have to reach
int targets[] = {90,90,90,90,90,90,170,90,90,90,90,0,0,0,0};
//Indicates wether the servos have reached their targets 
boolean movementStatus[] = {true, true, true, true, true, true, true,true,true,true,true,true,true,true,true};
//Store the time each servo has to move a step
long stepTimer[FIELD_COUNT];
//Stores last time a servo performed a step
long lastSteps[FIELD_COUNT];
//Global time the arm has to complete the next target
long movementPeriod = 100000;


int incomingByte;
int readCounter;
char buffer[FIELD_COUNT*FIELD_SIZE+3]; //Buffer for incoming data
int data[] = {90,90,90,90,90,90,170,90,90,90,90,0,0,0,0}; //Processed servo data
int multiplier[] = {1, 10, 100, 1000, 10000}; // Array of multiples of ten to be able to process data efficiently
boolean reading; 

boolean robotConnected;

void sendStatus()
{
  if(!robotConnected) 
  {
    Serial.println("STATUS: IDLE");
    Serial1.println("STATUS: IDLE");
  }
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
    Serial1.print("STATUS: CONNECTED. FIELD_COUNT: ");
    Serial1.print(FIELD_COUNT); 
    Serial1.print(" FIELD_SIZE: ");
    Serial1.print(FIELD_SIZE);
    Serial1.print(" POSITIONS: ");
    for(int i = 0; i < FIELD_COUNT; i++)
    {
      Serial1.print(data[i]);
      Serial1.print(" ");
    }
    
    Serial1.print(" DATA: ");
    for(int i = 0; i < FIELD_COUNT; i++)
    {
      Serial1.print(data[i]);
      Serial1.print(" ");
    }
    Serial1.println("AWAITING COMMANDS");
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
  targets[7] = data[7];
  targets[8] = data[8];
  targets[9] = data[9];
  targets[10] = data[10];
  targets[11] = data[11];
  targets[12] = data[12];
  targets[13] = data[13];
  targets[14] = data[14];
  for(int i = 0; i < FIELD_COUNT; i++)
  {
    int difference = abs(positions[i]-targets[i]); //Gets the difference between desired positions and current positions
    if(difference == 0){
      movementStatus[i] = true;
    }
    else{
      movementStatus[i] = false;
      stepTimer[i] = movementPeriod/difference; //Defines the timestep
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
      if(servo<SERVO_COUNT){ //If we have to move a servo
        servos[servo].write(positions[servo]); //Use the servo library
      }else{ //Else, use softpwm
        if(positions[servo]>255){ //Since motors can go backward and forward, numbers greater than than 255 mean forward power
          SoftPWMSet(motors[servo-SERVO_COUNT].forward,positions[servo]-256);
        }else{
          SoftPWMSet(motors[servo-SERVO_COUNT].backward,positions[servo]); //And viceversa.
        }
      }
      lastSteps[servo] = micros(); //Update last step
    }
    else{
      positions[servo] -= 1;
      if(servo<SERVO_COUNT){
        servos[servo].write(positions[servo]);
      }else{
        if(positions[servo]>255){
          SoftPWMSet(motors[servo-SERVO_COUNT].forward,positions[servo]-256);
        }else{
          SoftPWMSet(motors[servo-SERVO_COUNT].backward,positions[servo]);
        }
      }
      lastSteps[servo] = micros();
    }
  }
  if(positions[servo] == target) movementStatus[servo] = true;
}

/*
/ Loop to move all the servos
*/

void moveSegments(){
    for(int i = 0; i < FIELD_COUNT; i++)
    {
      if(i!=6){
        moveStep(i, targets[i]);
      }
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
  targets[6] == data[6] &
  targets[7] == data[7] &
  targets[8] == data[8] &
  targets[9] == data[9] &
  targets[10] == data[10] &
  targets[11] == data[11] &
  targets[12] == data[12] &
  targets[13] == data[13] &
  targets[14] == data[14];
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
  for(int i = 0; i < FIELD_COUNT; i++) //Clears all data
  {
    data[i] = 0; 
  }
}



void processMovementData()
{
  if(!robotConnected)
  {
    Serial.println("Robot not connected!");
    Serial1.println("Robot not connected!");
    return;
  }
  int counter = 0;
  for(int i = 3; i < FIELD_COUNT*FIELD_SIZE+3; i = i + FIELD_SIZE) //Iterates over all data caring about initial chars and steps
  {
    if((int)buffer[i]-0x30==1){ //IF the first number of the  field is zero, don't modify current data (disabled servo/motor)
      data[counter]=0;    //Clear last data
      for(int j = 1; j < FIELD_SIZE; j ++) //convert a secuence of numbers to an integer
      {
        data[counter] += ((int)buffer[i+j]-0x30)*multiplier[FIELD_SIZE - j-1];
      }
    }
    counter++; //move to next data
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
      Serial1.println("Connected");
      Serial.println("Connected");
      return;
    case 'R':
      robotConnected = false;
      reset();
      Serial1.println("Disconnected");
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
    for(int i = 0; i < FIELD_COUNT; i++) 
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
    delay(500);
    flAngle.write(90);
    delay(500);
    frAngle.write(90);
    delay(500);
    blAngle.write(90);
    delay(500);
    brAngle.write(90);
    
    // gripper signal to indicate we are ready to receive stuff
    delay(300);
    gripper.write(90);
    delay(200);
    gripper.write(170);
}

void readSerialData()
{
  incomingByte = Serial.read(); //Read byte
  if(incomingByte != -1) 
  {
    Serial1.write((char)incomingByte);
    Serial.write((char)incomingByte);
    if(!reading)
    {
      if((char)incomingByte =='&')//Open code
      {
        reading = true;
      }
    }
    else if((char)incomingByte == '%')//Close code
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

void readSerial1Data()
{
  incomingByte = Serial1.read(); //Read byte
  if(incomingByte != -1) 
  {
    Serial1.write((char)incomingByte);
    Serial.write((char)incomingByte);
    if(!reading)
    {
      if((char)incomingByte =='&')//Open code
      {
        reading = true;
      }
    }
    else if((char)incomingByte == '%')//Close code
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
    delay(5000);
    pinMode(13, OUTPUT);
    digitalWrite(13, HIGH); //LED on means the arm is not connected
    gripper.attach(6);
    gripper.write(170);
    delay(500);
    base.attach(7);
    delay(500);
    vertical1.attach(5);
    delay(500);
    //vertical2.attach(9);
    delay(500);
    horizontal1.attach(12);
    delay(500);
    horizontal2.attach(10);
    delay(500);
    horizontal3.attach(8);
    delay(500);
    flAngle.attach(11);
    delay(500);
    frAngle.attach(2);
    delay(500);
    blAngle.attach(3);
    delay(500);
    brAngle.attach(4);
    flSpeed.forward=A0;
    flSpeed.backward=A1;
    frSpeed.forward=A2;
    frSpeed.backward=A3;
    blSpeed.forward=A4;
    blSpeed.backward=A5;
    brSpeed.forward=9;
    brSpeed.backward=13;
    
    // gripper signal to indicate we are ready to receive stuff
    delay(300);
    gripper.write(90);
    delay(200);
    gripper.write(170);
    Serial.begin(115200);
    Serial1.begin(38400);
    SoftPWMBegin();
}


void loop()
{
  readSerialData();
  readSerial1Data();
  moveStuff();
}
