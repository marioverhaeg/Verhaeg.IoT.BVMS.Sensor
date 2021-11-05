# Introduction
BVMS is a Enterprise-grade video management system from Bosch: https://www.boschsecurity.com/bvms.

Eclipse Ditto™ is a technology in the IoT implementing a software pattern called “digital twins”: https://www.eclipse.org/ditto/

Verhaeg.IoT.BVMS.Sensor connects these two systems: BVMS events and alarms are pushed into Ditto. Ditto exposes itself using a REST interface, websocket, and other ways. 
This service allows other services to subscribe to BVMS events using websocket or get the status of BVMS using REST calls.

# Deployment
The BVMS SDK needs to be installed in the system where you run this service. The BVMS SDK is part of the BVMS set-up package. To deploy the service, for now, just copy the files includes in this Github to a directory of your choice. You can easily deploy the service using the commandline:

```
cd C:\Windows\Microsoft.NET\Framework[64]\[version]
installutil.exe C:\...\Verhaeg.IoT.BVMS.Sensor.exe
```

If there is interest I can attempt to build a decent installer for this project, but as long as I'm the only user it's not worth it :-).

# Configuration
You will find two sample configuration files in the Configuration folder:
- BVMS.json.example (replace the IP address, port, username, and password, and point to your BVMS system).
- Ditto_HTTP.json.example (replace the IP address, port, username, and password, and point to your Ditto system).
- Ditto_WS.json.example (replace the IP address, port, username, and password, and point to your Ditto system).

# Example
Once up and running, you will find BVMS statusses in Ditto, for example:
```json
[
  {
    "thingId": "Verhaeg.IoT.BVMS.IvmdMotionDetect03:Terras",
    "policyId": "Verhaeg.IoT.BVMS.IvmdMotionDetect03:Terras",
    "definition": "Verhaeg.IoT.BVMS:Event:1.0.0",
    "attributes": {
      "name": "Verhaeg.IoT.BVMS.IvmdMotionDetect03:Terras",
      "type": "IvmdMotionDetect03"
    },
    "features": {
      "Status": {
        "definition": [
          "Verhaeg.IoT.BVMS:Event:1.0.0"
        ],
        "properties": {
          "state": 0,
          "description": "MotionStopped"
        }
      }
    }
  }
]
```

# Command
You can also control the VirtualInput states from Ditto. Start the service and confirm the VirtualInput is available as a Thing in Ditto. Update its state and you should see the VirtualInput state changing in BVMS as well. You need to configure the Ditto websocking for this (using the Ditto_WS.json file).
```json
{
  "thingId": "Verhaeg.IoT.BVMS.Alarm.InputState:Alarm",
  "policyId": "Verhaeg.IoT.BVMS.Alarm.InputState:Alarm",
  "definition": "Verhaeg.IoT.BVMS:Event:1.0.0",
  "attributes": {
    "name": "Verhaeg.IoT.BVMS.Alarm.InputState:Alarm",
    "type": "InputState"
  },
  "features": {
    "Status": {
      "definition": [
        "Verhaeg.IoT.BVMS:Event:1.0.0"
      ],
      "properties": {
        "state": 0,
        "description": "Off"
      }
    }
  }
}
```
