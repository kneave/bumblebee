#include <Wire.h>
#include <Adafruit_MotorShield.h>
#include <TimerOne.h>

Adafruit_MotorShield AFMS = Adafruit_MotorShield();

Adafruit_DCMotor *motorLeft = AFMS.getMotor(1);
Adafruit_DCMotor *motorRight = AFMS.getMotor(2);

int blowerPin = 5;
int sweeperPin = 6;

int leftBumperPin = A0;
int rightBumperPin = A1;

volatile bool leftBumperHit = false;
volatile bool rightBumperHit = false;

void setup()
{	
	//  Need to setup serial
	Serial.begin(115200);

	pinMode(sweeperPin, OUTPUT);
	pinMode(blowerPin, OUTPUT);

	while (!Serial) {
		; //  Wait for serial to initialise

	//  initialise things for the motor shield
	//  do whatever else needs doing

	}

	Serial.println("Serial connected!");
	
	Timer1.initialize(50000);
	Timer1.attachInterrupt(CheckSensors);

	AFMS.begin();
}

void loop()
{
	int leftMotorSpeed, rightMotorSpeed, sweeperSpeed, blowerSpeed;
	leftMotorSpeed = rightMotorSpeed = sweeperSpeed = blowerSpeed = 0;
	
	int delayTime = 50;
	
	// listen for commands on the serial port
	bool gotData = false;

	while (Serial.available() > 0) {
		// if command received, action it
		// commands:
		//	To set motors; L or R and +- 0..255.
		//	W   s[W]eeper, 0..255
		//	B	[B]lower, 0..255
		//  S	get [S]ensor data, to be defined once sensors hooked up

		char inc = Serial.read();
		gotData = true;
		
		switch (inc)
		{
		case 'L':
			leftMotorSpeed = GetMotorValue();

			if (leftBumperHit && leftMotorSpeed > 0) {
				leftMotorSpeed = 0;
			}

			break;
		case 'R':
			rightMotorSpeed = GetMotorValue();
			
			if (rightBumperHit && rightMotorSpeed > 0) {
				rightMotorSpeed = 0;
			}

			break;
		case 'W':
			sweeperSpeed = GetMotorValue();
			break;
		case 'B':
			blowerSpeed = GetMotorValue();
			break;
		case 'D':
			delayTime = GetMotorValue();
			break;
		}
	}

	if (gotData)
	{
		String returnVal = "Left: " + String(leftMotorSpeed) +
			", Right: " + String(rightMotorSpeed) +
			", Sweeper: " + String(sweeperSpeed) +
			", Blower: " + String(blowerSpeed);

		Serial.println(returnVal);

		SetMotorValue(motorLeft, leftMotorSpeed);
		SetMotorValue(motorRight, rightMotorSpeed);
		
		analogWrite(blowerPin, blowerSpeed);
		analogWrite(sweeperPin, sweeperSpeed);

		delay(delayTime);
	}

	motorLeft->run(RELEASE);
	motorRight->run(RELEASE);

	analogWrite(blowerPin, 0);
	analogWrite(sweeperPin, 0);
}

int GetMotorValue() {
	char inc = Serial.read();
	bool isPositive = true;

	if (inc == '-')
	{
		isPositive = false;
	}

	int val =
		((Serial.read() - 48) * 100) +
		((Serial.read() - 48) * 10) +
		(Serial.read() - 48);

	int retval = (isPositive ? val : val * -1);

	//Serial.println("Retval: " + String(retval));

	return retval;
}

void SetMotorValue(Adafruit_DCMotor* motor, int inputSpeed) {
	uint8_t speed = inputSpeed >= 0 ? inputSpeed : inputSpeed * -1;
	motor->setSpeed(speed);

	//Serial.println(String(speed));

	if (inputSpeed > 0) {
		motor->run(FORWARD);
	}
	else
	{
		motor->run(BACKWARD);
	}
}

void CheckSensors() {
	bool checkLeftBumperHit = analogRead(leftBumperPin) > 12;
	bool checkRightBumperHit = analogRead(rightBumperPin) > 12;

	if (checkLeftBumperHit != leftBumperHit | checkRightBumperHit != rightBumperHit) {
		leftBumperHit = checkLeftBumperHit;
		rightBumperHit = checkRightBumperHit;
		
		Serial.println("State change: L: " + String(leftBumperHit) + ", R: " + String(rightBumperHit));
	}
}