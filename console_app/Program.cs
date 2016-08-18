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
            //_serialPort = new SerialPort();
            //_serialPort.PortName = Properties.Settings.Default.COMPort;
            //_serialPort.BaudRate = 115200;
            //_serialPort.Open();

            //if (_serialPort.IsOpen)
            //{
            //    Console.WriteLine("Serial connected");
            //}
            //else
            //{
            //    Console.WriteLine("No serial connection");
            //}

            _controller = new Controller(UserIndex.One);
            
            if (_controller.IsConnected)
            {
                _gameloop = new Timer();
                _gameloop.Elapsed += Gameloop_Elapsed;
                _gameloop.Interval = 100;
                _gameloop.AutoReset = true;
                _gameloop.Start();
            }
            else
            {
                Console.WriteLine("No controller.");
            }

            Console.WriteLine("Press enter to quit...");
            Console.ReadLine();
        }

        private static void Gameloop_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetControllerInfoStrings();
            //Console.WriteLine("{0}, {1}, {2}", _leftAxis, _rightAxis, _buttons);

            //int leftMotorSpeed, rightMotorSpeed;
            //string controlString;
            //DPadControl(out leftMotorSpeed, out rightMotorSpeed, out controlString);

            LeftAnalogControl();

            //if (leftMotorSpeed != 0 | rightMotorSpeed != 0)
            //{
            //    if (_serialPort.IsOpen)
            //    {
            //        _serialPort.WriteLine(controlString);
            //    }
            //}
        }

        private static void DPadControl(out int leftMotorSpeed, out int rightMotorSpeed, out string controlString)
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

            controlString = string.Format("L{0}{1:000}R{2}{3:000}W{4}B{4}",
                leftMotorSpeed >= 0 ? "+" : string.Empty,
                leftMotorSpeed,
                rightMotorSpeed >= 0 ? "+" : string.Empty,
                rightMotorSpeed,
                cleaning ? "+255" : "000");
            Console.WriteLine(controlString);
        }

        private static void LeftAnalogControl()
        {
            int xValue = _controllerState.Gamepad.LeftThumbX;
            int yValue = _controllerState.Gamepad.LeftThumbY;
            Tuple<double, double> motorPower = MotorValues(xValue, yValue);

            
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
        public static Tuple<double, double> MotorValues(double x, double y)
        {
            double motorPowerLeft = 0.0;
            double motorPowerRight = 0.0;

            int deadzoneX = 1500;
            int deadzoneY = 1500;

            bool xValid = x >= deadzoneX & x <= (deadzoneX * -1) ? true : false;
            bool yValid = y >= deadzoneY & y <= (deadzoneY * -1) ? true : false;

            double clampedX = (((x + 32768) / (32768 * 2)) * 20) - 10;
            double clampedY = (((y + 32768) / (32768 * 2)) * 20) - 10;
                

            //if (xValid | yValid) // or dead zone values
            if (true)
            {
                // what will be how fast we move, based on how far we are from the centre point.
                double power = Math.Sqrt(clampedX * clampedX + clampedY * clampedY);
                // figure out angle from y axis of x,y cord
                double angle = 0D;
                int xS = Math.Sign(clampedX);
                int yS = Math.Sign(clampedY);
                if (clampedX == 0 && yS > 0)
                {
                    angle = 0;
                }
                else if (clampedY == 0 && xS > 0)
                {
                    angle = (Math.PI / 2);
                }
                else if (clampedX == 0 && yS < 0)
                {
                    angle = Math.PI;
                }
                else if (clampedY == 0 && xS < 0)
                {
                    angle = (Math.PI + (Math.PI / 2));
                }
                else if (xS > 0 && yS > 0)
                {
                    angle = Math.Tan(clampedY / clampedX);
                }
                else if (xS < 0 && yS > 0)
                {
                    angle = (Math.PI / 2) + Math.Tan(clampedX / clampedY);
                }
                else if (xS < 0 && yS < 0)
                {
                    angle = Math.PI + Math.Tan(clampedX / clampedY);
                }
                else
                {
                    angle = (Math.PI + (Math.PI / 2)) + Math.Tan(clampedY / clampedX);
                }
                double offset = Math.PI - ((3 * Math.PI) / 4);  // the point the cos and sin curves cross over.
                motorPowerLeft = Math.Sin(angle + offset) * power;
                motorPowerRight = Math.Cos(angle + offset) * power;

                Console.WriteLine("Controller: {0},{1}; clamped: {2},{3} angle {4} p{5} Power: {6},{7}",
                x,
                y,
                clampedX,
                clampedY,
                (angle / Math.PI) * 180,
                power,
                motorPowerLeft,
                motorPowerRight
                );
            }

            Tuple<double, double> motorPower = new Tuple<double, double>(motorPowerLeft, motorPowerRight);

            
            return motorPower;
        }
    }
}