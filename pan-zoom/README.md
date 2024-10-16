## Introduction

This project contains functions that enable pan and pinch to zoom from within the Rive file. This way you can zoom in without losing quality. 

A modified version of RiveScreen.cs is provided which also prevents button click during panning/zooming and will prevent panning/zooming during button click.

Currently only a screen overly mode is supported which draws one artboard over the entire screen. If you need to draw the artboard in a frame, additional coordinate mapping is required.

This project uses the new Unity Input system and works both with a mouse, track pad, and touch screen. In order to integrate this into your own project, you have to follow a specific workflow, described below.

The logic will not allow panning or zooming outside of the artboard boundaries, so you need to zoom in before pan is possible.

## New project workflow

The workflow is somewhat complex. The rev file is provided, which will help figuring out how to set it up.

-In the Unity project settings, set the Active Input Handling to the New input system.

-Add the **Touch** folder to the project. This is used for touch input mapping.
	
-Put all the graphic items you want to be use for pan/zoom into a group called "origin". Set the origin position to Rive coordinates 0,0

![Alt text](readme-png/origin%20coordinate.png?raw=true)

-Create timelines with the following names:

![Alt text](readme-png/timeline%20names.png?raw=true)

-Select the state machine and create boolean and number inputs with the following names:

![Alt text](readme-png/inputs.png?raw=true)

-Set the "pan x" and "pan y" number inputs to a value of 50. set the "scale" input to 0. These numbers are timeline movement percentages. So 50 means the timeline is in the middle and 0 means the timeline is all the way to the left.

-Create joysticks with the following names:

![Alt text](readme-png/joysticks.png?raw=true)

-Assign the joystick handles to the inputs "scale", "pan x", and "pan y". Do not use "pan x joy 1" etc. because those are used for something else.

![Alt text](readme-png/joystick%20assignment.png?raw=true)

-Set the "scale" joystick handle to -100%. This is needed to prevent a scaled view while using the Rive editor for other tasks.

![Alt text](readme-png/joystick%202.png?raw=true)

-Select the state machine and delete any the auto generated layer containing one of the created timelines.

-Create a timeline, call it "scale" and add a "Blend State (Additive)" node.

-Drag the EMPTY node onto the state machine. 

-Connect the state machines like so:

![Alt text](readme-png/blend%20state%20connect.png?raw=true)

-Select the transition arrow from the blend state to the empty state and add a condition named "scale changed". Set it to "false":

![Alt text](readme-png/scale%20changed%20condition%201.png?raw=true)

-Now select the transition arrow from the empty state to the blend state and add the same condition: "scale changed" Set this one to "true":

![Alt text](readme-png/scale%20changed%20condition%202.png?raw=true)

The empty timeline in the state machine is used for performance reasons. The blend state should only run if panning or zooming is in progress, otherwise it will use resources all the time.

-Select the blend state, click the + button next to "Timelines" and select "Mix by Value". On the drop down box, select "scale joy 1". Leave the corresponding number to 100.

-Select the + button again and select "Mix by Input". For the timeline on the left drop down box select "sale joy 2". From the right drop down box (next to scale joy 2), select "scale" (not "scale changed").

![Alt text](readme-png/blend%20state%20setup.png?raw=true)

-Duplicate the scale state machine layer and call it "pan x". Select the blend state and change the timelines to "pan x joy 1" and "pan x joy 2" (2 on the top, 1 on the bottom). Change "scale" next to "pan x joy 2" to "pan x". Select the transition arrow and change both directions to "pan x changed".

![Alt text](readme-png/pan%20blend%20state%20setup.png?raw=true)

-Create another state machine layer for "pan y" using the same method.
	
-On the Animations panel, select the "scale" timeline. In the Hierarchy select the origin group. 

-With the timeline cursor all the way to the left, key the x and y scale, set at 100%:

![Alt text](readme-png/scale%20timeline.png?raw=true)

-Move the timeline cursor all the way to the right and key the scale as 250% for both x and y.

-Open the file PanZoom.cs and set the variable maxScaleRive to the same value (250 in this case).
	
-On the Animations panel, select the "pan x" timeline. In the Hierarchy select the origin group. 

-With the timeline cursor all the way to the left, key the x position of the origin and set it to -1000.

-Move the timeline cursor all the way to the right and key the right, key the x position of the origin and set it to +1000.

-Now set the timeline cursor in the middle so the view is centred.

-Open the file PanZoom.cs and set the variable maxPanRive to the same positive value (1000 in this case).
	
-On the Animations panel, select the "pan y" timeline. In the Hierarchy select the origin group. 

-With the timeline cursor all the way to the left, key the y position of the origin and set it to -1000.

-Move the timeline cursor all the way to the right and key the right, key the y position of the origin and set it to +1000.

-Now set the timeline cursor in the middle so the view is centred.
	
-The maximum pan value should be bigger than the maximum zoom value and depends also on the artboard size. The formula how to calculate the required pan value is given in PanZoom.cs in using the function GetPanRangeRequired().
	
-In the Animations panel, select the timeline "scale joy 1". In the Hierarchy, select the joystick "scale.

-With the timeline cursor selected all the way to the left, key the joystick handle to -100%:
	
![Alt text](readme-png/scale%20timeline%20joystick%20handle%20key.png?raw=true)
	
-With the same method, key the "scale joy 2" timeline to a joystick handle of +100%.

-Do the same for the "pan x" and "pan y" timelines. Make sure to select the correct joystick if you key the handle.
	
-Test the setup. Select the state machine, and click Play. Tick the box of all boolean inputs. Change the pan and scale values and confirm that the graphic is moving as expected. For "pan y", a higher input value should make the graphic move downward. For "pan x", a higher input value should make the graphic move to the right. For the scale, a higher input value should make the graphic zoom in.