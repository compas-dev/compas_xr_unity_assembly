# compas_xr_unity
COMPAS XR: Visualizer app for collaborative robotic assembly

Firebase Installations Quickstart

### Requirements
1. Download and install Unity Hub and [Unity 2023.3.10f1](https://unity.com/download)   
2. Android Build Support (OpenJDK & Android SDK & NDK Tools)(when developing for Android) and iOS Build Support (if developing for iOS) - have to be ticked in the installation modules when installing Unity. 
   
![add_modules](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/8f14d61a-eb75-4082-9077-473d24f9ba4f)

Alternatively, they can also be added post-installation from "Add Modules".

![image](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/262ec2be-64ed-423d-b12a-a55ea8129d16)

![image](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/347d98fb-2103-49ab-baa0-659cd0316295)


### Firebase


Register your Android/iOS app with [Firebase](https://firebase.google.com/docs/unity/setup).

1. Create a Firebase account (https://console.firebase.google.com)

2. Create a new project in the Firebase console
   
![Screenshot 2024-03-04 at 12 12 26](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/f958aafe-d239-4182-a9d2-6928e2ef7317)
![Screenshot 2024-03-04 at 12 11 34](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/0bc3df26-59cd-4389-ba31-c49c27d8f450)
![Screenshot 2024-03-04 at 12 11 44](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/f3c0cd81-dac5-4c71-97ad-b2a28f5622bd)
![Screenshot 2024-03-04 at 12 12 09](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/b0135279-cd4b-4fa5-be66-8cd6f4c10656)


3. Associate your project to a Unity app by clicking the Unity icon.
   
![Screenshot 2024-03-04 at 12 45 05](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/67ec8475-cc0c-4256-b8ab-51454d35612e)

- Here you can add both an Android and an iOS app to your project.
- You should use a format such as ```com.ETHZ.cdf``` for your bundle identifier. Make sure that your bundle identifier is unique (a strict requirement for iOS app development)
  
![Screenshot 2024-03-04 at 12 54 53](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/331d1ad5-4ccb-4bfa-9573-22e624ba21c7)

4. Download the ```google-services.json``` file associated with your Firebase project  for Android and ```GoogleService-Info.plist``` for iOS. These files contain the information that you need to connect your Android app to the Firebase backend, and will need to be included either in the FirebaseInitialize script in the Unity project or at the start of the app, before initializing Firebase. You will need to look for the following parameters:
App id, api key, database url, storage bucket, and project id
  
![Screenshot 2024-03-04 at 12 55 26](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/354c66a2-6c8a-4334-b8f8-ea04d5257303)

 - An example of these configurations is shown below:

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

5. Accessing Firebase config information afterwards
    - In your Firebase console, navigate to Project Overview and click the gear icon.
    - In the drop-down window select Project Settings
    - In the Project Settings window under Your apps
    - The required config information is listed under the section SDK setup and configuration -  ```google-services.json``` for Android, respectively ```GoogleService-Info.plist``` for iOS
  
![Screenshot 2024-03-04 at 12 55 55](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/5cf1d388-0efc-4ff8-9394-4fc2d390620c)

    

6. Android apps must be signed by a SHA1 key, and the key's signature must be registered to your project in the Firebase Console.
   To generate a SHA1, first you will need to set the keystore in the Unity project.
    - Go to ```Publishing Settings``` under ```Player Settings``` in the Unity editor.
    - Select an existing keystore, or create a new keystore using the toggle.
    - Select an existing key, or create a new key using ```Create a new key```.
    - Build an apk to be able to generate the SHA1 key (see below under ```Unity``` and ```Android``` build how to build)
    
 
![Screenshot 2024-03-04 at 13 34 22](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/84c69d22-b6a1-491e-9c4c-5e284f44eb8f)


 7. Generate the SHA1 key and copy it into your Firebase project.
    - After setting the keystore and key, as well as building the app once, you can generate a SHA1 by running this command in CMD (admin):
      
    ```
    keytool -list -v -keystore <path_to_keystore> -alias <key_name>
    ```

    - Copy the SHA1 digest string into your clipboard.
    - Navigate to your Android App in your Firebase console.
    - From the main console view, click on your Android App at the top, and open the settings page.
    - Scroll down to your apps at the bottom of the page and click on Add Fingerprint.
    - Paste the SHA1 digest of your key into the form. The SHA1 box will illuminate if the string is valid. If it's not valid, check that you have copied the entire SHA1 digest string.
    
![Screenshot 2024-03-04 at 13 36 07](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/0c722295-5642-45b2-a2bc-f6454369b56e)
    
   
    

8. Change the rules in ```Realtime Database``` to :

```
{
  "rules": {
    ".read": true,
    ".write": true
  }
}
```




### Unity

1. Open Unity Hub. In Projects, click on Open(MacOS) or ADD(Windows). Locate the folder you downloaded from GitHub `compas_xr_unity` on the drive and add it.

   <img width="781" alt="Screenshot 2023-10-30 at 11 34 07" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/cac60d2a-8f64-466f-a8da-d92565934d21">

   
2. Open the Unity Project.

3. Update the Project Settings
    - Select the File > Build Settings menu option.
    - Select Android or iOS in the Platform list and click Switch Platform to set the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner of the Unity status bar.
    - Select the active platform and click Player Settings.
    - In the Player Settings panel adjust the following settings accordingly for Android, respectively iOS
  
### Android

   - In Player Settings, scroll down to Identification / Override Default Package Name/ and update Package Name to the value of the Bundle Identifier you provided when you registered your app with Firebase.
   - In Build Settings, click Build and Run to build the project on an Android device. * In case the device is not a developer device, scroll at the bottom to the Developer Device section.
     Alternatively, one can just Build to obtain the ```apk``` and distribute it to Android devices.
   - If an error occurs, check player settings against default player settings depicted below:

   ![1](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/9f6b28c8-be15-472c-bd90-d800e595abba)
   ![2](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/b607ceba-24c9-4ea8-ad95-0c4107da7db8)
   ![3](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/bba71c84-8296-41ff-8739-b9034298ec35)
   ![4](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/08849be6-852b-4831-95a0-cc55e39520d4)
   ![5](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/6876b6f9-3412-4c52-824a-111b0ed6abb3)
   ![6](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/2387f5f6-864c-4278-8f97-bf61440ebda2)
   ![7](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/9b6efd26-9adf-4a89-ad15-285423f39dcb)
   ![8](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/fbe00a7a-2182-4031-ae9b-d5c689baf924)
   ![9](https://github.com/gramaziokohler/compas_xr_unity/assets/94670422/466a5be1-bfde-4336-b6d9-b83d8ec21fbe)

  
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

     





