Leap Motion Unity Examples
=====

The following repository contains Leap Motion SDK examples built with Unity. Each project is built using V2 Core Services Skeletal Tracking code from [Leap Motion's Developer Portal](https://developer.leapmotion.com). 

##Project Descriptions

###BlockDestruction1
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/detail_image/6420b157-5c33-469e-9697-360196775b33.gif">
Prevent the blocks from hitting the camera by swatting the blocks away with your hand.

###BlockDestruction2
Push, flick, wave around the green blocks to hit the white blocks, close your fist to bring back the green blocks to its initial stage.

###BlockPit
Description coming soon...

###Leap3DObjectBrowser
Description coming soon...

###LeapRiggedHand
Rigged hand package. Get a rigged mesh driven by the skeletal API into your unity project.

You can import the asset by going to Assets -> Import Package -> Custom Package.

To get a rigged hand into your scene:
- Use /Assets/Resources/LeapMotion/Prefabs/LeapHandController
- Assign a parent transform and an offset

Example:
Attach Main Camera as the parent and set the offset to (0, -6, 10).

Default.unity is a scene that has this basic setup.

To add functionality to hands, either

(1) query for all hands through LeapHandController.GetHandGameObjects()
or
(2) attach scripts to the UnityHand prefab. See examples of stuff you can attach to hands in Assets/Resources/LeapMotion/Scripts/ExtensionExamples


###Pendulum
Wave you hand to start this simple pendulum physics demo. 
You can also change the camera angle of the pendulum by moving around your mouse.  

###PinchSelector
Palm position to hover over an item, pinch from thumb to index finger to select.

###RagdollThrower
Description coming soon...

###Robits
Description coming soon...

###RopeTest
This project is an experiment in 3D interaction with a rigged hand in Unity. Poke at the handing sign to move it.

###mirrorTest
Mirror test is an experiement developed to test the useability of using a reflective surface along a touch plane to give users a sense of z-depth. 
