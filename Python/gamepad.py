#import evdev
from evdev import InputDevice, categorize, ecodes
import serial 

# event3 is right for my joypad, yours may vary
gamepad = InputDevice('/dev/input/event3')

#  Entry point
print ("Starting up")

# speed variables
left = 0
right = 0
blower = 0
sweeper = 0

print ("Starting up")
connected = False

ser = serial.Serial('/dev/ttyACM1', 115200, timeout = 1) # ttyACM0 for Arduino board
readOut = 0   #chars waiting from Arduino

print(gamepad)

commandToSend = "L+000R+000"

def readGamepad():
	global left
	global right
	global blower
	global sweeper
	
	global gamepad
	global commandToSend
		
	try:
		for event in gamepad.read():	
			# Buttons 
			if event.type == ecodes.EV_KEY:
				print (event)
				
				absevent = categorize(event)
				code = ecodes.bytype[absevent.event.type][absevent.event.code]
				print(code, absevent.event.value)
				
				if code == "BTN_TR2":
					if absevent.event.value == 1:
						blower = 200
						sweeper = 200
					else:
						blower = 0
						sweeper = 0
			
			# Analogue gamepad
			if event.type == ecodes.EV_ABS:
				absevent = categorize(event)
				code = ecodes.bytype[absevent.event.type][absevent.event.code]
				
				# print(code, absevent.event.value)
				
				if code == "ABS_Y":
					left = (absevent.event.value - 128) * -1
					left *= 1.8
					
				if code == "ABS_RZ":
					right = (absevent.event.value - 128) * -1
					right *= 1.8
				
		
		leftSymbol = ""
		rightSymbol = ""
		
		if left >= 0:
			leftSymbol = "+"
						
		if right >= 0:
			rightSymbol = "+"
		
		commandToSend  = "L" + leftSymbol + str(int(left)).zfill(3) 
		commandToSend += "R" + rightSymbol + str(int(right)).zfill(3)
		
		if blower > 0:
			commandToSend += "W+200"
			commandToSend += "B+200"
		
		print(commandToSend)
		
	except:
		pass

while True:
	readGamepad()
	ser.write(str(commandToSend).encode())
	
	try:
		#print ("Attempt to Read")
		readOut = ser.readline().decode('ascii')
		#time.sleep(0.01)
		#print ("Reading: ", readOut) 
		continue
	except:
		pass
		
	ser.flush()
