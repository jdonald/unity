==========================================
Project: LeapMotion Unity Freeform Style Menus - C#
File: README.txt
Author: Daniel Plemmons - LeapMotion Developer Experience Engineer
Twitter: @RandomOutput
==========================================

Hello! If you have trouble with this UnityPackage or if 
you have feedback on how it's built, please feel free to tweet at me. 

==========================================
Contents
==========================================
- Overview
- Configuration Options
- Integrating Your Existing Leap Controls
- Integrating Button Actions
- Expected Names and Tags
- Adding a Sub Label

==========================================
Overview
==========================================

The menu code and example scene included in this unity package aim to reproduce the user experience from Leap Motion’s Freeform application, originally developed in C++ with the Cinder framework. The menus were recreated from the ground-up with Unity developers in mind. The assset is designed to plug into the existing LeapMotion Unity Hacker Helper available on the asset store and packaged with this UnityAsset. If you already have your own boilerplate code for accessing LeapMotion data, it's not too hard to swap it over. 

The core MenuBehavior monobehavior has a wide variety of configuration options available from the Unity editor letting you customize the look and feel of the menus. Those options are detailed in "Configuration Options" below.

The example scene file {root}/Assets/Scenes/MenuDemo.unity shows a basic way to hook the menus into wider game commands. Exactly how the menus are built to integrate into your game can be found under “Integrating Button Actions”. 


==========================================
Configuration Options
==========================================

===============
Menu Behavior
===============

Menu type # This enumeration defines what kind of content your menu is using. 
 > TEXT will create menus with text lables on each button. 
 > ICON will allow you to specify Sprite icons that appear on each button. text provided will show up in a sub-label if available. 
 > TEXTURE will allow you to specify a square Texture2D for each button that will be stretched across the button area.
 > Each of the menu types can be seen in the example scene. 

Event Handler # The Game Object containing a MenuEventHandler script. This script will accept the button actions sent from menus on selection. The logic after this is up to you.

Text # An array of strings. The text label associated with each button option.

Icons_active # Used in ICON mode only. An array of Sprites. The icon used when the menu option is  selected. If an inactive icon is not provided, a reduced alpha version of this icon will be displayed when the menu option is inactive. 

Icons_inactive # Used in ICON mode only. An array of Sprites. The icon used when the menu option is inactive.

Textures # Used in Texture mode only. The Texture2D applied to the button.

Button Actions # Array of ButtonAction’s. The length of this array determines how many slices the menu circle is split into. If a slice is given an action, that slice is interactable and will have content. If the slice is given NONE, it will be an inactive zone. This allows you to create menus that only use certain wedges of the total circle.

Angle Offset # Often when a menu only uses a certain wedge of the total circle, you will want to orient the menu so the wedge is facing the proper direction. The angle offset rotates the menu’s orientation. Angles are 0-360 counter clockwise. 

Radius # The radius of the fully activated menu. The radius is the center of the strip.

Thickness # The thickness of the menu strip.

Capture Offset # By the menu wedge is captured when the user reaches the center of the wedge. The offset allows you to offset this control point from the radius. A negative number will move the capture point towards the menu’s center.

Button_prefab # The prefab to use to create each menu wedge. It is not recommended that you modify this value. 

Activation Radius # The radius from the menu center (in pixels) where the menu will activate.

Selection Radius # The radius from the menu center where a menu button will be selected.

Deactivate Z # The world space Z of the Leap’s finger at which the menu will deactivate. 

Deactivation Speed # The speed at which the menu will scale down on deactivation.

Activation Curve # The easing curve for the menu activation action.

Activation Time # The total time of the activation animation.

Start Highlight # The distance from the wedge center when the highlight color transition will begin.

Full Highlight # The distance from the wedge center when the highlight color transition will complete.

Base Color # The unactivated color of the menu wedges. 

Selected Color # The color of a previously selected menu wedge.

Highlight Color # The color of a highlighted menu wedge.

Highlight Percent Growth # The scale of ICON and TEXT content when the wedge becomes highlighted.

Scale Down Speed # Similar to Deactivation Speed. The Scale down speed is the speed the other wedges scale down when another menu wedge is selected.

Selection Snap Time # When a wedge is selected, it snaps back towards the menu center a small amount. This is how long that snap takes.

Sprite Scaling Factor # When a wedge is selected, it snaps back towards the menu center a small amount. This is how far it snaps back.

Selection Cooldown # This is how long the menu will wait to select another wedge after a wedge has been selected. 

===============
MenuMoverBehavior
===============

Hand Sweep Enabled: Enable the hand sweep gesture to remove the menus from the screen and bring them back.

Full Throw Distance: The max distance the sweep gesture can throw the menus.

Throw Filter: A filter for taking the throw location and converting it to a throw distance. 

Throw Speed: The speed at which the throw curve is traversed.

Fade In Time: When a menu is selected, all other menus fade out and move away. This is the time they take to come back.

Fade Out Time: When a menu is selected, all other menus fade out and move away. This is the time they take to fade out.

Fade Curve: An easing curve for the fade in/out animation.
 
Fade Throw Distance: The total move-away distance during a fade.

Layout Original Aspect Ratio: For aspect ratio agnostic layout. The aspect ratio at which the original layout was made.

==========================================
Integrating Your Existing Leap Controls
==========================================

The menus expect all their Leap inputs in terms of world coordinates. The example scene uses a the included LeapMotion Hacker Helper package to handle these conversions. You can either use this as a base and extend it to your needs, or implement and expose your own versions of the data provided. The README.txt in the Leap_Hacker_Helper will explain more.

==========================================
Integrating Button Actions
==========================================

In the sample scene, the menus send their Button Actions to a MenuEventHandler script. The easiest place to tie the menus into your own scene is by implementing your own logic in this script. If you’d like to change the selection logic at a deeper level, the selection occurs in MenuBehavior->update()

//Make Selection
if(worldDistance - _captureOffset > _selection_radius)
 {
_selectionEndTime = Time.time + _selectionDelayTime;
_currentSelection = _closest;
_scalingFactor = 1.0f;

if(_eventHandler != null && _closest < _buttonActions.Length)
{
_eventHandler.recieveMenuEvent(_buttonActions[_closest]);
}

_currentState = MenuState.SELECTION;
}

==========================================
Expected Names and Tags
==========================================

Every Menu is expected to have the Menu tag.
Every Menu root node is expected to have the MenuRoot tag.
The orthographic camera that renders the UI elements is expected to have the name UI Cam.
The perspective camera that renders the main scene is expected to have the name MainCam.

==========================================
Adding a Sub Label
==========================================

If a menu root has a TextMesh gameObject named menuSub as a child, that will be treated as a sub label.
