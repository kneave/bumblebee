# Project Bumblebee
I was gifted an original iRobot Roomba, one of the 2002 models, which has developed a few faults.  I've always wanted to buid a robot too but the chassis cost and sensors just seemed to get out of control.  

If only there was a way to kill two birds with one stone...

This is an Original Roomba which means it doesn't have a diagnostic port, as a result I've had to hook directly in to the sensors and motors directly. 

# Current Status
I've hooked in to the electronics and replaced the existing board with an Arduino and Adafruit motor shield, a Raspberry Pi runs a Python script that listens for events from a gamepad and sends commands over serial.

The battery on the Roomba is toast, it needs replacing, and I haven't yet connected to the on board sensors and motor encoders.

# Future
I'm not sure how far I'm going to take this, it's mostly just a learning excercise at this point and a stepping stone to building something from scratch.  
