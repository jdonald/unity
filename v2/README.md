<img src="https://lm-assets.s3.amazonaws.com/screenshots/leap_unity_logo.png">
=====

<img src="https://di4564baj7skl.cloudfront.net/assets/mac-a3b33298ed46dbf8a36151ac0357dbef.png">

##V2: Skeletal Beta
The following repository contains Leap Motion SDK examples built with Unity. Each project is built using V2 Core Services Skeletal Tracking code from [Leap Motion's Developer Portal](https://developer.leapmotion.com/downloads/skeletal-beta). For additional support with these examples and more, feel free to reach out to us via: [Leap Motion's Community Forums](https://community.leapmotion.com/category/beta)  

**NOTE**: The V2 Skeletal Beta code is NOT currently released to production for consumers. 

##Getting Started

####1. Clone the GitHub repository and choose a project.

```bash
git clone git@github.com:leapmotion-examples/unity
cd unity/v2/LeapRiggedHand/
```

####2. Prepare the project for Unity Standard (free license). Skip this step if using Unity Pro.
Examples for the command line. You can also use your file explorer (move two files, delete Plugins folder).

**Mac:**
```bash
mv Assets/Plugins/LeapCSharp.bundle/Contents/MacOS/libLeap*.dylib .
mv Assets/Plugins/LeapCSharp.NET3.5.dll Assets
rm -rf Assets/Plugins
```

**Windows:**
```bash
move Assets/Plugins/Leap.dll .
move Assets/Plugins/LeapCSharp.dll .
move Assets/Plugins/LeapCSharp.NET3.5.dll Assets
del Assets/Plugins
```

####3. Open the project
* Unity, select File -> Open Project -> Open other...
* Choose the cloned LeapRiggedHand directory and click Open.

####4. Play a Scene in the project
* In the project explorer, choose Assets -> Resources -> LeapMotion -> Scenes
* Double-click DefaultExample
* Click Play.

####5. You're done!
* Move your hands above the Leap Motion Controller and you should be seeing the new rigged hand.
* If not, see our [FAQ](https://developer.leapmotion.com/downloads/skeletal-beta/faq) section

##Resources
* V2 Skeletal Tracking Beta Access, Email: beta@leapmotion.com
* Each Unity project example folder has a short README
* [Leap Motion Getting Started (Unity)](https://developer.leapmotion.com/downloads/skeletal-beta/set_up_new_project#unity)
* [Leap Motion API Docs (C#)](https://developer.leapmotion.com/documentation/skeletal/csharp/index.html)
* [Unity Tutorial Videos](https://unity3d.com/learn/tutorials/modules)

##Contributing
* Make a fork, name your branch, add your addition or fix.
* Add your name, email, and github account to the CONTRIBUTORS.txt list, thereby agreeing to the terms and conditions of the Contributor License Agreement.
* Open a Pull Request. If your information is not in the CONTRIBUTORS file, your pull request will not be reviewed.
