Leap Motion Unity Examples
=====

The following repository contains Leap Motion SDK examples built with Unity. Each project is built using V2 Core Services Skeletal Tracking code from [Leap Motion's Developer Portal](https://developer.leapmotion.com). For additional support with these examples and more, feel free to reach out to us via: [Leap Motion's Community Forums](https://community.leapmotion.com/)  

##Project Descriptions

###BlockDestruction1
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/19b236ce-30d1-4278-ace5-1c4eae3e3b1c.jpg">

Prevent the blocks from hitting the camera by swatting the blocks away with your hand.

---

###BlockDestruction2
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/44374ce9-e61e-4cac-a027-bcc841e1c3cf.jpg">

Push, flick, wave around the green blocks to hit the white blocks, close your fist to bring back the green blocks to its initial stage.

---

###BlockPit

<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/01029c8b-812e-4c0b-8370-08f1a3a9c1bc.jpg">

Description coming soon...

---

###Leap3DObjectBrowser
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/4dcdf483-c754-480b-83f3-c94e7d9e056f.jpg">

Description coming soon...

---

###LeapRiggedHand
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/a2e7d4bf-b95a-4036-b4b6-d70e208f2b83.png">

Rigged hand package. Get a rigged mesh driven by the skeletal API into your unity project.

You can import the asset by going to Assets -> Import Package -> Custom Package.

To get a rigged hand into your scene:
- Use /Assets/Resources/LeapMotion/Prefabs/LeapHandController
- Assign a parent transform and an offset

Example:
Attach Main Camera as the parent and set the offset to (0, -6, 10).

Default.unity is a scene that has this basic setup.

To add functionality to hands, either

(1) Query for all hands through LeapHandController.GetHandGameObjects()
or
(2) Attach scripts to the UnityHand prefab. See examples of stuff you can attach to hands in Assets/Resources/LeapMotion/Scripts/ExtensionExamples

---

###Pendulum
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/35c24d05-c7cc-4b0f-ad54-accab3d2e710.jpg">

Wave you hand to start this simple pendulum physics demo. 
You can also change the camera angle of the pendulum by moving around your mouse.  

---

###PinchSelector
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/unity-pinch.png">

Palm position to hover over an item, pinch from thumb to index finger to select.

---

###RagdollThrower
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/51496b87-b9ba-4184-a581-510debcb6ca6.jpg">

Description coming soon...

---

###Robits
Image & description coming soon...

---

###RopeTest
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/bbe81d1a-4b55-4882-b7f1-a6745495b891.jpg">

This project is an experiment in 3D interaction with a rigged hand in Unity. Poke at the handing sign to move it.

---

###mirrorTest
<img src="https://leapmotion-leapdev-production.s3.amazonaws.com/uploads/library/thumbnail_image/79685b84-de98-4ff2-9143-206e72ff9026.jpg">

Mirror test is an experiement developed to test the useability of using a reflective surface along a touch plane to give users a sense of z-depth. 

---
