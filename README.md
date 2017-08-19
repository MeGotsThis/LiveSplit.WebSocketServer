# LiveSplit WebSocket Server

LiveSplit WebSocket Server is a LiveSplit component that allows for other programs and other computers to read and control LiveSplit. 

### Why use this instead of LiveSplit.Server?

One of the main differences is that this component will send the majority of the LiveSplit current state as a JSON message. Using WebSockets allows you not to worry about buffering data unlike some socket library. Also this component will send the LiveSplit state when an event has occured like starting the timer or hitting a split. In addition, the current state will be sent every 15 seconds to help ensure consistency.

## Install

Download from https://github.com/MeGotsThis/LiveSplit.WebSocketServer/releases. Put LiveSplit.WebSocketServer.dll and websocket-sharp.dll in the Components folder of LiveSplit. Currently LiveSplit 1.7.4 is not supported. Only use the LiveSplit Development Build.

## Setup 

Add the component to the Layout (Control -> LiveSplit WebSocket Server). In Layout Settings, you can change the Server Port and view your local IP Address and allowing to auto start.

### Control 

You can start the Server before programs can talk to it (Right click on LiveSplit -> Control -> Start Server). Unlike LiveSplit Server, this component has the ability to auto start.
