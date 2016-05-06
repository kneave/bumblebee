#include <Adafruit_MotorShield.h>

int max_command_length = 30;
char* inc_command = new char[max_command_length + 1];

void setup()
{
	//  Need to setup serial
	Serial.begin(9600);

	while (!Serial) {
		; //  Wait for serial to initialise

	//  initialise things for the motor shield
	//  do whatever else needs doing

		Serial.println("Serial connected!");
	}
}

void loop()
{
	int leftMotor, rightMotor, sweeper, blower;
	leftMotor = rightMotor = sweeper = blower = 0;

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
			leftMotor = GetMotorValue();
			break;
		case 'R':
			rightMotor = GetMotorValue();
			break;
		case 'W':
			sweeper = GetMotorValue();
			break;
		case 'B':
			blower = GetMotorValue();
			break;
		}

		// if no command for a while, milliseconds, then stop all actions
		// some are bool, brush on/off for example, others momentary like motor control
	}

	if (gotData)
	{
		String returnVal = "Left: " + String(leftMotor) +
			", Right: " + String(rightMotor) +
			", Sweeper: " + String(sweeper) +
			", Blower: " + String(blower);

		Serial.println(returnVal);
	}
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