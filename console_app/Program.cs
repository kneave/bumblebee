using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace console_app
{
    class Program
    {
        private static string _leftAxis;
        private static string _rightAxis;
        private static string _buttons;
        private static Controller _controller;

        private static Timer _gameloop;
        private static SerialPort _serialPort;
        private static State _controllerState;

        private static bool cleaning = false;

        static void Main(string[] args)
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = Properties.Settings.Default.COMPort;
            _serialPort.BaudRate = 115200;

            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
	        {
                Console.WriteLine(ex);    
            }

            if (_serialPort.IsOpen)
            {
                Console.WriteLine("Serial connected");
            }
            else
            {
                Console.WriteLine("No serial connection");
            }

            _controller = new Controller(UserIndex.One);
            
            if (_controller.IsConnected)
            {
                _gameloop = new Timer();
                _gameloop.Elapsed += Gameloop_Elapsed;
                _gameloop.Interval = 1;
                _gameloop.AutoReset = false;
                _gameloop.Start();
            }
            else
            {
                Console.WriteLine("No controller.");
            }

            Console.WriteLine("Press enter to quit...");
            Console.ReadLine();
            _serialPort.Close();
        }

        private static void Gameloop_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetControllerInfoStrings();
            //Console.WriteLine("{0}, {1}, {2}", _leftAxis, _rightAxis, _buttons);

            float leftMotorSpeed, rightMotorSpeed;
            int sweeperSpeed, blowerSpeed;
            blowerSpeed = sweeperSpeed = (cleaning) ? 255 : 000;

            string controlString;

            bool dPadUsed = _controllerState.Gamepad.Buttons == GamepadButtonFlags.DPadUp ||
                _controllerState.Gamepad.Buttons == GamepadButtonFlags.DPadDown ||
                _controllerState.Gamepad.Buttons == GamepadButtonFlags.DPadLeft ||
                _controllerState.Gamepad.Buttons == GamepadButtonFlags.DPadRight ||
                _controllerState.Gamepad.Buttons == GamepadButtonFlags.A;

            if (dPadUsed)
            {
                DPadControl(out leftMotorSpeed, out rightMotorSpeed);
            }
            else
            {
                LeftAnalogControl(
                    out leftMotorSpeed, 
                    out rightMotorSpeed,
                    out sweeperSpeed,
                    out blowerSpeed);
            }

            controlString = string.Format("L{0}{1:000}R{2}{3:000}W+{4:000}B+{5:000}",
                leftMotorSpeed >= 0 ? "+" : string.Empty,
                leftMotorSpeed,
                rightMotorSpeed >= 0 ? "+" : string.Empty,
                rightMotorSpeed,
                sweeperSpeed,
                blowerSpeed);

            Console.WriteLine(controlString);
            //Console.WriteLine(_serialPort.ReadLine());

            if (leftMotorSpeed != 0 | rightMotorSpeed != 0)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.WriteLine(controlString);
                }
            }
            _gameloop.Start();
        }

        private static void DPadControl(out float leftMotorSpeed, out float rightMotorSpeed)
        {
            leftMotorSpeed = rightMotorSpeed = 0;

            switch (_controllerState.Gamepad.Buttons)
            {
                case GamepadButtonFlags.DPadUp:
                    leftMotorSpeed = rightMotorSpeed = 255;
                    break;
                case GamepadButtonFlags.DPadDown:
                    leftMotorSpeed = rightMotorSpeed = -255;
                    break;
                case GamepadButtonFlags.DPadLeft:
                    leftMotorSpeed = -255;
                    rightMotorSpeed = 255;
                    break;
                case GamepadButtonFlags.DPadRight:
                    leftMotorSpeed = 255;
                    rightMotorSpeed = -255;
                    break;
                case GamepadButtonFlags.A:
                    cleaning = !cleaning;
                    break;
            }
        }

        private static void LeftAnalogControl(
            out float leftMotorSpeed, 
            out float rightMotorSpeed,
            out int sweeperSpeed,
            out int blowerSpeed)
        {
            leftMotorSpeed = rightMotorSpeed = 0;

            int maxValue = 32768;

            //Get X and Y from the Joystick, do whatever scaling and calibrating you need to do based on your hardware.            
            int xInput = (_controllerState.Gamepad.LeftThumbX);
            int yInput = (_controllerState.Gamepad.LeftThumbY);

            //Invert X
            int xValue = (Math.Abs(xInput) < 5000) ? 0 : -xInput;
            int yValue = (Math.Abs(yInput) < 5000) ? 0 : yInput;
                        
            //Calculate R+L(Call it V): V = (100 - ABS(X)) * (Y / 100) + Y
            int v = (maxValue - Math.Abs(xValue)) * (yValue / (maxValue*maxValue)) + yValue;

            //Calculate R-L(Call it W): W = (100 - ABS(Y)) * (X / 100) + X
            int w = (maxValue - Math.Abs(yValue)) * (xValue / (maxValue*maxValue)) + xValue;

            //Calculate R: R = (V + W) / 2
            rightMotorSpeed = ((v + w) / 2) / (float)(maxValue / 2);

            //Calculate L: L = (V - W) / 2
            leftMotorSpeed = ((v - w) / 2) / (float)(maxValue / 2);


            ////Do any scaling on R and L your hardware may require.
            rightMotorSpeed = Math.Abs(rightMotorSpeed) > 1
                ? (rightMotorSpeed > 0 ? 1 : -1)
                : rightMotorSpeed;

            leftMotorSpeed = Math.Abs(leftMotorSpeed) > 1
                ? (leftMotorSpeed > 0 ? 1 : -1)
                : leftMotorSpeed;

            rightMotorSpeed = 255 * rightMotorSpeed;
            leftMotorSpeed = 255 * leftMotorSpeed;

            sweeperSpeed = _controllerState.Gamepad.LeftTrigger;
            blowerSpeed = _controllerState.Gamepad.RightTrigger;

            //Console.WriteLine("I: {0},{1}; M: {2},{3}",
            //    _controllerState.Gamepad.LeftThumbX,
            //    _controllerState.Gamepad.LeftThumbY,
            //    leftMotorSpeed,
            //    rightMotorSpeed);
        }

        private static void GetControllerInfoStrings()
        {
            _controllerState = _controller.GetState();
            _leftAxis = string.Format("Left: X: {0} Y: {1}", _controllerState.Gamepad.LeftThumbX, _controllerState.Gamepad.LeftThumbY);
            _rightAxis = string.Format("Right: X: {0} Y: {1}", _controllerState.Gamepad.RightThumbX, _controllerState.Gamepad.RightThumbY);
            //_buttons = string.Format("A: {0} B: {1} X: {2} Y: {3}", state.Gamepad.Buttons.ToString(), state.Gamepad.LeftThumbY);
            _buttons = string.Format("Buttons: {0}", _controllerState.Gamepad.Buttons);
        }

        //  Thanks to Wil Sellwood for this code
        public static Tuple<int, int> MotorValues(double x, double y)
        {
            double motorPowerLeft = 0.0;
            double motorPowerRight = 0.0;

            int deadzoneX = 2500;
            int deadzoneY = 2500;

            double clampedX = Math.Round((((x + 32768) / (32768 * 2)) * 440) - 220);
            double clampedY = Math.Round((((y + 32768) / (32768 * 2)) * 440) - 220);

            bool xValid = x >= -deadzoneX & x <= deadzoneX ? false : true;
            bool yValid = y >= -deadzoneY & y <= deadzoneY ? false : true;

            if (xValid || yValid) // or dead zone values
            {
                // what will be how fast we move, based on how far we are from the centre point.
                double power = Math.Sqrt(clampedX * clampedX + clampedY * clampedY);
                // figure out angle from y axis of x,y cord
                double angle = AngleFromYAxis(clampedX, clampedY);
                
                double offset = Math.PI - ((3 * Math.PI) / 4);  // the point the cos and sin curves cross over.
                motorPowerLeft = Math.Sin(angle + offset) * power;
                motorPowerRight = Math.Cos(angle + offset) * power;

                //Console.WriteLine("Controller: {0},{1}; clamped: {2},{3} angle {4} p{5} Power: {6},{7}",
                //    x,
                //    y,
                //    clampedX,
                //    clampedY,
                //    (angle / Math.PI) * 180,
                //    power,
                //    motorPowerLeft,
                //    motorPowerRight);
            }

            Tuple<int, int> motorPower = new Tuple<int, int>((int)motorPowerLeft, (int)motorPowerRight);
                        
            return motorPower;
        }
        
        /**
         * Work out the angle of a point from the positive y axis. 
         */
        private static double AngleFromYAxis(double x, double y)
        {
            double angle = 0;

            int xS = Math.Sign(x);
            int yS = Math.Sign(y);
            if (x == 0 && yS > 0)
            {
                angle = 0;
            }
            else if (y == 0 && xS > 0)
            {
                angle = (Math.PI / 2);
            }
            else if (x == 0 && yS < 0)
            {
                angle = Math.PI;
            }
            else if (y == 0 && xS < 0)
            {
                angle = (Math.PI + (Math.PI / 2));
            }
            else if (xS > 0 && yS > 0)
            {
                angle = Math.Tan(x / y);
            }
            else if (xS > 0 && yS < 0)
            {
                angle = (Math.PI / 2) + Math.Tan(x / y);
            }
            else if (xS < 0 && yS < 0)
            {
                angle = Math.PI + Math.Tan(x / y);
            }
            else
            {
                angle = -Math.Tan(x / y);
            }
            return angle;
        }
    }
}
