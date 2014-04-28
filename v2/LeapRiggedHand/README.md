LeapRiggedHand
=====

<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/a2e7d4bf-b95a-4036-b4b6-d70e208f2b83.png">

##Description

Rigged hand package. Get a rigged mesh driven by the skeletal API into your unity project.

You can import the asset by going to Assets -> Import Package -> Custom Package.

**To get a rigged hand into your scene:**
- Use /Assets/Resources/LeapMotion/Prefabs/LeapHandController
- Assign a parent transform and an offset

**Example:**
Attach Main Camera as the parent and set the offset to (0, -6, 10). Default.unity is a scene that has this basic setup.

**To add functionality to hands, either:**

(1) Query for all hands through LeapHandController.GetHandGameObjects()

or

(2) Attach scripts to the UnityHand prefab. See examples of stuff you can attach to hands in Assets/Resources/LeapMotion/Scripts/ExtensionExamples

##API Methods
* [Hands](https://developer.leapmotion.com/documentation/skeletal/csharp/api/Leap.Hand.html)
* [Fingers](https://developer.leapmotion.com/documentation/skeletal/csharp/api/Leap.Finger.html)
