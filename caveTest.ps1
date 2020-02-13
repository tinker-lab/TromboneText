# Windows Powershell Launch Script
# Script Generated On Tuesday, August 20, 2019, 10:57:45 AM
# Setup contains 4 displays and 0 display managers

# Display: FrontWall
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveFrontWall_Top -vrmode stereo -logFile '.\logs\FrontWall.txt'
sleep 5
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveFrontWall_Bottom -vrmode stereo

# Display: LeftWall
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveLeftWall_Top -vrmode stereo
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveLeftWall_Bottom -vrmode stereo

# Display: RightWall
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveRightWall_Top -vrmode stereo
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveRightWall_Bottom -vrmode stereo

# Display: Floor
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveFloor_Top -vrmode stereo
& '.\MinVRUnity.exe' -screen-fullscreen 0 -popupwindow -vrdevice CaveFloor_Bottom -vrmode stereo
