# compas_xr_unity
COMPAS XR: Visualizer app for collaborative robotic assembly

Firebase Installations Quickstart

### Requirements
1. Download and install Unity Hub and [Unity 2022.3.3f1] (unityhub://2022.3.3f1/7cdc2969a641)   
2. Android SDK and Java JDK (when developing for Android) and iOS (if developing for iOS) - have to be ticked in the installation modules when installing Unity. 
   
   <img width="718" alt="Screenshot 2023-10-30 at 10 55 29" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/e6c67897-379b-4180-9481-79d43805842c">

Alternatively, they can also be added post-installation from "Add Modules".

   <img width="1022" alt="Screenshot 2024-02-16 at 13 33 41" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/d882c2ec-0d25-4ad9-a207-b442b1b3414b">

   <img width="713" alt="Screenshot 2024-02-16 at 13 33 57" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/4c45aef8-2baa-4a77-9401-084207556221">




### Unity

1. Open Unity Hub. In Projects, click on Open(MacOS) or ADD(Windows). Locate the folder you downloaded from GitHub `compas_xr_unity` on the drive and add it.

   <img width="781" alt="Screenshot 2023-10-30 at 11 34 07" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/cac60d2a-8f64-466f-a8da-d92565934d21">

   
2. Open the Unity Project.
3. Accept the Developer Agreement

    - In Unity, go through the following steps in order to accept Vuforia’s Developer Agreement:
      Help - Vuforia Engine - Show Developer Agreement -> Accept
      
       <img width="499" alt="Screenshot 2023-10-30 at 11 27 16" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/8f08fced-6fd9-4662-a77e-a390f20be665">

4. Import Ros# into the project

    - In case you don’t have a Unity ID yet, go to the Unity website and register an account.
      https://id.unity.com/en/conversations/5c9a9838-2b4d-4c7e-bc53-d31475d0ba8001af 
    - Following that, go to the Asset Store and add Ros# to your asset list:
      https://assetstore.unity.com/packages/tools/physics/ros-107085
    
      <img width="488" alt="Screenshot 2023-10-30 at 11 28 14" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/3d207ce7-09eb-4e0b-b37f-09afa575621a">

    - In Unity’s Package Manager Window open the Packages drop-down menu and choose My Assets.  Make sure you are loading all your assets in the list.
    - Download and import Ros# to the project 

### Android
(Unless you wish to test the project with the given credentials, please follow all steps below. Otherwise, skip to 7)
Register your Android app with [Firebase](https://firebase.google.com/docs/unity/setup).
1. Create a Unity project in the Firebase console.

2. Associate your project to an app by clicking the Add app button, and selecting the Unity icon.
    - You should use ```com.ETHZ.cdf``` as the package name while you're testing.
    - If you do not use the prescribed package name you will need to update the bundle identifier as described in the
      - *Optional: Update the Project Bundle Identifier below.*
    - change the rules in ```Realtime Database``` to :

```
{
  "rules": {
    ".read": true,
    ".write": true
  }
}
```

3. Accessing Firebase config information
    - In your Firebase console, navigate to Project Overview and click the gear icon.
    - In the drop-down window select Project Settings
    - In the project settings window under Your apps / Androis Apps select Compas_XR
    - The Required Config Information is listed under the section SDK setup and configuration / ```google-services.json```
and an example is shown below:

```
// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyAO_YVROUIc866BqgWgcBpPxUe6SVG5O9g",
  authDomain: "cdf-project-f570f.firebaseapp.com",
  databaseURL: "https://cdf-project-f570f-default-rtdb.europe-west1.firebasedatabase.app",
  projectId: "cdf-project-f570f",
  storageBucket: "cdf-project-f570f.appspot.com",
  messagingSenderId: "641027065982",
  appId: "1:641027065982:web:20ca92f0a2326bc3dab02f",
  measurementId: "G-RZ5BVHNGK8"
};
```

4. Android apps must be signed by a SHA1 key, and the key's signature must be registered to your project in the Firebase Console. To generate a SHA1, first you will need to set the keystore in the Unity project.
    - Go to ```Publishing Settings``` under ```Player Settings``` in the Unity editor.
    - Select an existing keystore, or create a new keystore using the toggle.
    - Select an existing key, or create a new key using ```Create a new key```.
    - Build an apk to be able to generate the SHA1 key
    - After setting the keystore and key, as well as building the app once, you can generate a SHA1 by running this command in CMD (admin):
      
    ```
    keytool -list -v -keystore <path_to_keystore> -alias <key_name>
    ```

    - Copy the SHA1 digest string into your clipboard.
    - Navigate to your Android App in your Firebase console.
    - From the main console view, click on your Android App at the top, and open the settings page.
    - Scroll down to your apps at the bottom of the page and click on Add Fingerprint.
    - Paste the SHA1 digest of your key into the form. The SHA1 box will illuminate if the string is valid. If it's not valid, check that you have copied the entire SHA1 digest string.
      
5. Download the ```google-services.json``` file associated with your Firebase project from the console. This file contains the information mentioned above that, you need to connect your Android app to the Firebase backend, and will need to be included either in the FirebaseInitialize script in the Unity project or at the start of the app, before initializing Firebase. You will need to look for the following parameters:
App id, api key, database url, storage bucket, and project id


6. Optional: Update the Project Bundle Identifier.
    - If you did not use ```com.ETHZ.cdf``` as the project package name you will need to update the sample's Bundle Identifier.
    - Select the File > Build Settings menu option.
    - Select Android in the Platform list
    - Click Player Settings.
    - In the Player Settings panel scroll down to Bundle Identifier and update the value to the package name you provided when you registered your app with Firebase.
      
7. Build for Android.
    - Select the File > Build Settings menu option.
    - Select Android in the Platform list.
    - Click Switch Platform to select Android as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner of the Unity status bar.
    - Click Build and Run.
  
### iOS - can only be installed on ios from a MacBook running Xcode
1. Create a Unity project in the Firebase console.

2. Associate your project to an app by clicking the Add app button, and selecting the iOS icon.
    - You should use ```com.ETHZ.compas.xr.ios``` as the package name while you're testing.
    - If you do not use the prescribed package name you will need to update the bundle identifier as described in the
      - *Optional: Update the Project Bundle Identifier below.*
    - change the rules in ```Realtime Database``` to :

```
{
  "rules": {
    ".read": true,
    ".write": true
  }
}
```

3. Accessing Firebase config information
    - In your Firebase console, navigate to Project Overview and click the gear icon.
    - In the drop-down window select Project Settings
    - In the project settings window under Your apps / Apple Apps select Compas_XR
    - The Required Config Information is listed under the section SDK setup and configuration / ```GoogleService-Info.plist```, which is the equivalent of the ```google-services.json``` for ios

4. Download the ```GoogleService-Info.plist``` file associated with your Firebase project from the console. This file contains the information mentioned above that, you need to connect your iOS app to the Firebase backend, and will need to be included either in the FirebaseInitialize script in the Unity project or at the start of the app, before initializing Firebase. You will need to look for the following parameters:
App id, api key, database url, storage bucket, and project id.

5. Optional: Update the Project Bundle Identifier in Unity.
    - If you did not use ```com.ETHZ.compas.xr.ios``` as the project package name you will need to update the sample's Bundle Identifier.
    - Select Android in the Platform list
    - Click Player Settings.
    - In the Player Settings, under the iOS panel:
         - scroll down to Bundle Identifier and update the value to the package name you provided when you registered your app with Firebase.
         - scroll down to Camera Usage Description and write a message describing the need to use the camera, such as "please allow camera use for AR"
         - scroll down to iOS version and pick the adequate version (min. 14 to support current project packages)

      ![Screenshot 2024-02-16 at 11 56 27](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/625d7f8f-12d1-424f-9e54-c420b6986748)

6. Build for iOS.
    - Select the File > Build Settings menu option.
    - Select iOS in the Platform list.
    - Click Switch Platform to select iOS as the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner of the Unity status bar.
    - Click Build and select a folder location on your drive for the build. Ideally you should create a folder called Builds and within it, individual files for each build.
  
      <img width="604" alt="Screenshot 2024-02-16 at 14 28 06" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/16aec886-085d-4db1-b9e2-88f0d5613b1b">

8. Optional: Unless you have Xcode installed,
   - Go to your MacBook's AppStore and install Xcode
   - Install necessary iOS modules


8. Install on iOS Device with Xcode
   - Open the file with the last build on your computer
   - Select the "Name".xcworkspace file and open it with Xcode
   - In Xcode click the file's name on the left column to open the Settings
   - Under Signing and Capabilities \ All, tick ```Automatically manage signing``` and confirm ```Enable Automatic```
   - 
     ![Screenshot 2024-02-16 at 14 51 31](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/2d47b798-3f99-4662-b858-2a358d44b79a)

   - Under Team, open the drop-down down and select the development team/individual
   - Optional: this is the last chance to adjust the Bundle Identifier and App Name before installing
   - Connect iOS device by cable
   - Unless the device is already enabled as Developer, the possibility to enable the mode opens after connecting to Xcode
   - At the top, make sure the device is connected and click the play triangle to start building and installing on device
  
     ![Screenshot 2024-02-16 at 14 48 31](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/c74fa3ac-bf67-45bb-ac26-9328f617de50)

     





