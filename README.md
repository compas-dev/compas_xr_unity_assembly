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


6. In the Project Overview, under ```All products``` click to add the following: Authentication, Realtime Database and Storage.

![Screenshot 2024-03-04 at 16 45 08](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/0f7d0961-e7cf-4c5b-b9f7-943d25e5edb8)


7. Create a Database

   - In Project Overview, under Project shortcuts, click on ```Realtime Database```
   - Click on Create Database
   - Set the Databse location according to your needs and ```Start in locked mode```
   

![Screenshot 2024-03-04 at 17 04 10](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/2b26269e-78c5-4ddb-aace-5ad31d072369)

![Screenshot 2024-03-04 at 17 04 27](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/890bcbbc-1b47-4abc-8724-975256c49a03)

![Screenshot 2024-03-04 at 17 04 59](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/9564497f-fab5-4cf6-ad23-6f77ef19e5c3)


8. Change the rules in ```Realtime Database``` to  allow writing.

   - In Project Shortcuts, under Realtime Database, click on ```Rules``` and change ```false``` to ```true``` and ```Publish```

```
{
  "rules": {
    ".read": true,
    ".write": true
  }
}
```

![Screenshot 2024-03-04 at 17 18 14](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/1f2acb6e-fc26-4814-aff3-7755553e126f)
![Screenshot 2024-03-04 at 17 18 33](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/5bee71ab-430c-4ede-9128-78343ff565d1)


### *The following steps are specific to Android development if building from Unity:   

8. Android apps must be signed by a SHA1 key, and the key's signature must be registered to your project in the Firebase Console.
   To generate a SHA1, first you will need to set the keystore in the Unity project.
    - Go to ```Publishing Settings``` under ```Player Settings``` in the Unity editor.
    - Select an existing keystore, or create a new keystore using the toggle.
    - Select an existing key, or create a new key using ```Create a new key```.
    - Build an apk to be able to generate the SHA1 key (see below under ```Unity``` and ```Android``` build how to build)
    
 
![Screenshot 2024-03-04 at 13 34 22](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/84c69d22-b6a1-491e-9c4c-5e284f44eb8f)


 9. Generate the SHA1 key and copy it into your Firebase project.
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
    



### Unity

1. Open Unity Hub. In Projects, click on Open(MacOS) or ADD(Windows). Locate the folder you downloaded from GitHub `compas_xr_unity` on the drive and add it.

<img width="781" alt="Screenshot 2023-10-30 at 11 34 07" src="https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/cac60d2a-8f64-466f-a8da-d92565934d21">

   
2. Open the Unity Project.

3. Update the Project Settings
    - Select the File > Build Settings menu option.
    - Select ```Android``` or ```iOS``` in the Platform list and click Switch Platform to set the target platform.
    - Wait for the spinner (compiling) icon to stop in the bottom right corner of the Unity status bar.
    - Select the active platform and click Player Settings.
    - In the Player Settings panel adjust the following settings accordingly for Android, respectively iOS
  
   ### Unity - Android

   - In Player Settings, under the Android panel: scroll down to Identification / Override Default Package Name/ and update ```Package Name``` to the value of the Bundle Identifier you provided when you registered your app with Firebase.
   - In File > Build Settings, click Build and Run to build the project on an Android device. * In case the device is not a developer device, scroll at the bottom to the Developer Device section.
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

  
   ### Unity - iOS 

Once the target platform gas been switched to iOS, Unity will try to install CocoaPods, an iOS resolver. 
![Screenshot 2024-02-20 at 10 44 26 AM](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/84193ecc-35bf-44fd-8e56-f6c53edbce43)
   
   
   Most likely it will fail and you will need to do the following fixes:

   ### 1. In Xcode: 

   - Make sure you have ```Xcode``` and the ```Developer Tools``` installed on your MacBook. If you don't, go to your MacBook's AppStore and install Xcode.
   - Sign in with your Apple ID.

![Screenshot 2024-02-20 at 11 08 41 AM](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/2b711a51-1817-4589-9a30-2123fb270cb0)

   - Connect your iOS device to the laptop via a USB cable and unless your device is already in Developer Mode, scroll down and see the instructions for Developer Devices.
   - Install necessary modules in Xcode (e.g. iOS 17.2)

![Screenshot 2024-02-20 at 11 19 44 AM (1)](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/a9310653-0ee0-43d5-9b72-7901c3379327)


   ### 2. In Terminal:

   - Set the export path for gems as follows: ```export PATH="/Users/username/.gem/ruby/2.6.0/bin:$PATH"```
   - Install gem active support: ```gem install activesupport -v 6.1.7.6 --user-install```

![Screenshot 2024-02-20 at 11 01 53 AM](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/7ccb26ac-dac4-4c23-b73f-ba210327f2a1)


   ### 3. Back in Unity: 

    - Under Assets > External Dependency Manager > iOS resolver > Install CocoaPods
    - Under Assets > External Dependency Manager > iOS resolver > Settings check that you have matching settings with the ones below:

![Screenshot 2024-03-04 at 18 29 06](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/1541047c-a754-45a9-9805-faf11a3ef0d4)

![Screenshot 2024-03-04 at 18 31 04](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/707281bb-0748-4dea-aeba-434219d81c0e)



   In the Player Settings, under the iOS panel:
   
   - Scroll down to Identification / Override Default Package Name/ and update ```Package Name``` to the value of the Bundle Identifier you provided when you registered your app with Firebase.
   - Scroll down to ```Camera Usage Description``` and write a message describing the need to use the camera, such as ```"please allow camera use for AR"```.
   - Scroll down to ```iOS version``` and pick the adequate version (min. 14 to support current project packages).
   - If an error occurs, during a build, check player settings against default player settings depicted below:
     
![Screenshot 2024-03-04 at 17 41 14](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/e9eff300-2a88-4700-9a56-c9c2fd2c96d6)
![Screenshot 2024-03-04 at 17 41 28](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/b3242215-34d1-44bd-918d-5e4a2d41262d)
![Screenshot 2024-03-04 at 17 41 41](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/2be88dcf-7e32-41ad-8965-3815a1b0ed7b)

  
   ### Note - it is possible to build for iOS from a Windows computer, but the resulting folder needs to go through a MacBook with Xcode in order to be installed or distributed on an iOS device.
   
   - Ideally one would Build or Build and Run the project from a MacBook that has Xcode installed.
   - In File > Build Settings click on Build and select a folder location on your drive for the build. Ideally you should create a folder called Builds and within it, individual files for each build.
    
![Screenshot 2024-03-04 at 18 35 23](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/b6eab44a-526d-4875-9f99-1410c9d613ec)


   ### 4. In Finder:

   - Open the Builds folder and find the file with the last build on your computer, eg. ```01```
   - Select the ```BuildName".xcworkspace``` file and open it with Xcode. Make sure you open the ```.xcworkspace``` and not the.xcodeproj

   ### 5. In Xcode: 
   
   - In Xcode click the file's name on the left column to open the Settings
   - Under Signing and Capabilities > All, tick ```Automatically manage signing``` and confirm ```Enable Automatic```

![Screenshot 2024-02-20 at 11 08 07 AM](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/9f7da855-0d4b-4e3b-8abd-4626b66a9651)

   - Under Team, open the drop-down down and select the development team/individual
   - Optional: this is the last chance to adjust the Bundle Identifier and App Name before installing
   - At the top, make sure the iOS device is connected and click the play triangle to start building and installing on device
  
![Screenshot 2024-02-16 at 14 48 31](https://github.com/gramaziokohler/compas_xr_unity/assets/146987499/c74fa3ac-bf67-45bb-ac26-9328f617de50)

     





