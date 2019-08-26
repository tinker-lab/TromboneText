
@rem Start one server for distributing events / syncing the displays
START C:\V\bin\minvr_unity_server.exe -c ivlabcave_unity_server
SLEEP 2

@rem Start one graphics program per wall
START "Front Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveFrontWall_Top
SLEEP 5
START "Front Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveFrontWall_Bottom
SLEEP 5
START "Left Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveLeftWall_Top
SLEEP 5
START "Left Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveLeftWall_Bottom
SLEEP 5
START "Right Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveRightWall_Top
SLEEP 5
START "Right Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveRightWall_Bottom
SLEEP 5
START "Floor" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveFloor_Top
SLEEP 5
START "Floor" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrdevice CaveFloor_Bottom

