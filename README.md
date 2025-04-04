### Note from the Authors

This application was developed by Marcello Morena and Travis Strawbridge as a final year project for
our Software Engineering degrees. The Project was delegated to us by Dr. James Walsh through the Australian Research Centre for Interactive and Virtual Enviroments. The project was to be a proof of concept for a defence client and was developed throughout the course of a year.

# StellAR

StellAR is a proof of concept for a collaborative XR 3D model inspection/annotation tool. Users can import their own models into the system, allowing them to decompose models into individual parts and post text, voice, and colour annotations. Users can also import metadata for the model and each specific component to display information such as manufacturer, purpose, etc.

## Features

### Model inspection
<img src="./Documentation/Images/Object Manipulation.gif" />

### Model decomposition
<img src="./Documentation/Images/Explosion.gif" />

### Text annotations
<img src="./Documentation/Images/Text Annotation.gif" />

### Voice annotations
<img src="./Documentation/Images/Voice Annotation.gif" />

### Colour highlight
<img src="./Documentation/Images/Highlight.gif" />

# Video Demonstration
[![StellAR Demo](https://img.youtube.com/vi/-Q1NN2UmKO4/0.jpg)](https://www.youtube.com/watch?v=-Q1NN2UmKO4)



# Requirements

If you are using the already compiled PC application, or are deploying the already compiled .appx to HoloLens you do not need any additional software besides a web browser. If you plan to add new models to StellAR and then build/deploy a new version including the new models, you will need the following:

### Unity version 2022.3.21f1 LTS

This project was built on the 2022.3.21f1 LTS, if you want to ensure 100% compatability use this version. If building to HoloLens, install Unity with the 'Universal Windows Platform Build Support' module.

### Blender

Blender is required if importing .blend files into StellAR.

### Visual Studio

To build StellAR into HoloLens deployable applications you need Visual Studio with the 'Windows application development' workload installed. Under 'optional', tick the Windows SDK for your version of Windows, 'Universal Windows Platform tools', and 'C++ (v143) Universal Windows Platform tools'. If you also want to develop StellAR you will need additional workloads and features as detailed in the documentation.

# Building and deploying

Open the StellAR project in Unity and follow the instructions below for building and deployment to PC and HoloLens.

## PC

When downloading the build from the GitHub release, Windows Defender may report a trojan in the download as a false positive. Open Windows Defender to allow the download or build from source.

If you want to build StellAR for PC, go to 'File' > 'Build Settings...' and make sure the 'Windows, Mac, Linux' platform is selected. Modify 'Target Platform', 'Architecture', etc. as required and select 'Build'. Choose your destination directory and Unity will create the application in that location.

## HoloLens
### Build

Go to 'File' > 'Build Settings...' and make sure that the 'Universal Windows Platform' platform is selected. The settings here should already be correct, Build Type should be "D3D Project", Target SDK Version should be 'Latest installed', Minimum Platform Version should be '10.0.10240.0', and Visual Studio Version should be 'Latest installed'. Other settings can be left to default as they may only apply when selecting 'Build And Run'. Now click the Build button and select a location to build the Visual Studio solution to.

Open the Visual Studio solution (ensure you installed the required workload and features listed above). Right click on StellAR in the Solution Explorer and go to 'Publish...' > 'Create App Packages...'. On the first page ensure 'Sideloading' is selected then on the following page select 'Yes, use the current certificate:'. On the final page tick only the ARM64 architecture with the Master solution configuration. All other settings on this final page are irrelevant, you can now click Create and Visual Studio will build the solution including the .appx file that we will deploy to the HoloLens.

### Deploy
To deploy the .appx file to HoloLens devices we will use the Windows Device Portal. To enable this on the HoloLens go to Settings > Update & Security > For developers and enable Developer Mode ('Use developer features') and Device Portal. Access the device portal on your PC by opening a web browser (ensure you are on the same network) and entering the HoloLens IP in the address bar. You may encounter an error about an invalid certificate, bypass this error and continue to the web page.

On the device portal, go to Views > Apps and in the 'Deploy apps' area select the .appx file you just created and install it (you may need to scroll to find the Install button). On a fresh install of StellAR you will need to agree to providing StellAR access to the HoloLens camera for coordinate centering (using Vuforia image targets) in multiplayer sessions to work. StellAR is now successfully deployed and available on the HoloLens device.

## Modifying models in StellAR
### Adding new models

Models should be .fbx or .blend files with unique names. Drag and drop your model into the Assets/Resources/Models folder within Unity. If you place the model into the folder in Windows instead of dragging into Unity, make sure to right click the model file in Unity and reimport it so the necessary files are created. Select the model in Unity and ensure that the tickbox for Read/Write under the Meshes heading is ticked. Your model has now been successfully added to StellAR.

### Updating models

If you update a model that is already in the Assets/Resources/Models folder, the object in StellAR won't update until you delete the corresponding prefab in Assets/Resources/Prefabs. Replace the model in the Models folder with your newly updated model, delete prefab in the Prefabs folder with the same name as your model, then right click on your newly replaced model file in Models and reimport it to generate a prefab that contains your new changes. The model is now updated in StellAR.

### Deleting models
When removing a model from StellAR, just delete the prefab and model files from the Models and Prefabs directories.

# Using StellAR
## Hand controls for PC

Hold shift - Move left hand

Hold space - Move right hand

Left click - Pinch hands (for interacting)

t - Toggle left hand lock

y - Toggle right hand lock

Alt - Rotate left hand

Ctrl - Rotate right hand

WASD - Move camera

Q - Move down

E - Move up

## Tips

To bring up the hand menu toggle the lock on for one of the hands and rotate it so the palm faces the screen. Use the other hand to interact with buttons on the hand menu.

## Multiplayer sessions

Multiplayer sessions use Vuforia to centralise the space for all users, make sure you print the Vuforia Image Target and look at the print out before spawning any models.

Make sure all users have joined before spawning any models.

If the host leaves the session will end for all users.
