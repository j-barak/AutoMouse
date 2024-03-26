using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoMouse {
    class AutoMouse {
        #region "Button"
        public enum ButtonType {
            Left,
            Middle,
            Right
        }

        private ButtonType buttonType;
        private bool doubleClick;
        #endregion

        #region "Location"
        public enum LocationType {
            Cursor,
            Fixed,
            Random,
            RandomRange
        }

        private LocationType locationType;
        private int x;
        private int y;
        private int width;
        private int height;
        #endregion

        #region "Delay"
        public enum DelayType {
            Fixed,
            Range
        }

        private DelayType delayType;
        private int delay;
        private int delayRange;
        #endregion

        #region "Count"
        public enum CountType {
            Fixed,
            UntilStopped
        }

        private CountType countType;
        private int count;
        #endregion

        #region "Update storage"
        private bool buttonUpdated;
        private ButtonType tmpButtonType;
        private bool tmpDoubleClick;
        private bool locationUpdated;
        private LocationType tmpLocationType;
        private int tmpX;
        private int tmpY;
        private int tmpWidth;
        private int tmpHeight;
        private bool delayUpdated;
        private DelayType tmpDelayType;
        private int tmpDelay;
        private int tmpDelayRange;
        private bool countUpdated;
        private CountType tmpCountType;
        private int tmpCount;
        #endregion

        Thread Clicker;
        Random rnd;

        public AutoMouse() {
            rnd = new Random();
        }

        public class NextClickEventArgs : EventArgs {
            public int NextClick;
        }

        public event EventHandler<NextClickEventArgs> NextClick;
        public EventHandler<EventArgs> Finished;
        
		private void Click() {
            SyncSettings();
            int remaining = count;
            while (countType == CountType.UntilStopped || remaining > 0) {
                if (!IsAlive)
                    return;
                SyncSettings();
                List<Windows10.INPUT> inputs = new List<Windows10.INPUT>();
                if (locationType == LocationType.Fixed) {
                    Windows10.INPUT input = new Windows10.INPUT {
                        type = Windows10.InputEventType.InputMouse,
                        mi = new Windows10.MOUSEINPUT {
                            dx = Windows10.CalculateAbsoluteCoordinateX(x),
                            dy = Windows10.CalculateAbsoluteCoordinateX(y),
                            dwFlags = Windows10.MouseFlags.Move | Windows10.MouseFlags.Absolute
                        }
                    };
                    inputs.Add(input);
                } else if (locationType == LocationType.Random) {
                    Windows10.INPUT input = new Windows10.INPUT {
                        type = Windows10.InputEventType.InputMouse,
                        mi = new Windows10.MOUSEINPUT {
                            dx = rnd.Next(65536),
                            dy = rnd.Next(65536),
                            dwFlags = Windows10.MouseFlags.Move | Windows10.MouseFlags.Absolute
                        }
                    };
                    inputs.Add(input);
                } else if (locationType == LocationType.RandomRange) {
                    Windows10.INPUT input = new Windows10.INPUT {
                        type = Windows10.InputEventType.InputMouse,
                        mi = new Windows10.MOUSEINPUT {
                            dx = Windows10.CalculateAbsoluteCoordinateX(rnd.Next(x, x + width)),
                            dy = Windows10.CalculateAbsoluteCoordinateY(rnd.Next(y, y + height)),
                            dwFlags = Windows10.MouseFlags.Move | Windows10.MouseFlags.Absolute
                        }
                    };
                    inputs.Add(input);
                } // end if-else()

                for (int i = 0; i < (doubleClick ? 2 : 1); i++) {
                    if (i == 1) {
                        Thread.Sleep(50);
                    }	// end if()
                    
					if (buttonType == ButtonType.Left) {
                        Windows10.INPUT inputDown = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT {
                                dwFlags = Windows10.MouseFlags.LeftDown
                            }
                        };
                        inputs.Add(inputDown);
                        Windows10.INPUT inputUp = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT
                            {
                                dwFlags = Windows10.MouseFlags.LeftUp
                            }
                        };
                        inputs.Add(inputUp);
                    }	// end if()

                    if (buttonType == ButtonType.Middle) {
                        Windows10.INPUT inputDown = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT {
                                dwFlags = Windows10.MouseFlags.MiddleDown
                            }
                        };
                        inputs.Add(inputDown);
                        Windows10.INPUT inputUp = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT
                            {
                                dwFlags = Windows10.MouseFlags.MiddleUp
                            }
                        };
                        inputs.Add(inputUp);
                    }

                    if (buttonType == ButtonType.Right) {
                        Windows10.INPUT inputDown = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT {
                                dwFlags = Windows10.MouseFlags.RightDown
                            }
                        };
                        inputs.Add(inputDown);
                        Windows10.INPUT inputUp = new Windows10.INPUT {
                            type = Windows10.InputEventType.InputMouse,
                            mi = new Windows10.MOUSEINPUT {
                                dwFlags = Windows10.MouseFlags.RightUp
                            }
                        };
                        inputs.Add(inputUp);
                    }	// end if()
                }

                Windows10.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(new Windows10.INPUT()));
				int nextDelay = 0;
                
				if (delayType == DelayType.Fixed) {
                    nextDelay = delay;
                    
                } else {
                    nextDelay = rnd.Next(delay, delayRange);
                }	// end if-else()
				
                NextClick?.Invoke(this, new NextClickEventArgs { NextClick = nextDelay });
                Thread.Sleep(nextDelay);
                remaining--;
            }	// end while()
            Finished?.Invoke(this, null);
        }	// end void Click()

        public bool IsAlive {
            get {
                if (Clicker == null) {
                    return false;
                }
                return Clicker.IsAlive;
            }
        }	// end IsAlive()

        public void Start() {
            Clicker = new Thread(Click);
            Clicker.IsBackground = true;
            Clicker.Start();
        }	// end Start()

        public void Stop() {
            if (Clicker != null) {
                Clicker.Abort();
            }
        }	// end Stop()

        private void SyncSettings() {
            if (buttonUpdated) {
                buttonType = tmpButtonType;
                doubleClick = tmpDoubleClick;
                buttonUpdated = false;
            }	// end if()

            if (locationUpdated) {
                locationType = tmpLocationType;
                x = tmpX;
                y = tmpY;
                width = tmpWidth;
                height = tmpHeight;
                locationUpdated = false;
            }	// end if()

            if (delayUpdated) {
                delayType = tmpDelayType;
                delay = tmpDelay;
                delayRange = tmpDelayRange;

                delayUpdated = false;
            }	// end if()

            if (countUpdated) {
                countType = tmpCountType;
                count = tmpCount;

                countUpdated = false;
            }	// end if()
        }	// end SyncSettings()

        public void UpdateButton(ButtonType ButtonType, bool DoubleClick) {
            tmpButtonType = ButtonType;
            tmpDoubleClick = DoubleClick;

            buttonUpdated = true;
        }	// end UpdateButton()
	
        public void UpdateLocation(LocationType LocationType, int X, int Y, int Width, int Height) {
            tmpLocationType = LocationType;
            tmpX = X;
            tmpY = Y;
            tmpWidth = Width;
            tmpHeight = Height;
            locationUpdated = true;
        }	// end UpdateLocation()

        public void UpdateDelay(DelayType DelayType, int Delay, int DelayRange) {
            tmpDelayType = DelayType;
            tmpDelay = Delay;
            tmpDelayRange = DelayRange;
            delayUpdated = true;
        }	// end UpdateDelay()

        public void UpdateCount(CountType CountType, int Count) {
            tmpCountType = CountType;
            tmpCount = Count;
            countUpdated = true;
        }	// end UpdateCount()
    }
}
