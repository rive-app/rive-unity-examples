using UnityEngine;
using UnityEngine.InputSystem;
using Rive;

/*
A Samsung pen registers both as a finger touch and a pen device. All of these are engaged if a Samsung pen is on screen:
controls.Movement.Touch0.started
controls.Movement.Pen.started 
Pen.current.tip.isPressed

To prevent double events, this has to be filtered in code.

An Apple pen only registers only as a pen device and no touch related code is triggered.
*/


public class PanZoom: MonoBehaviour {

    //To prevent a button press during panning and zooming, and to prevent panning during a button press,
    //a threshold value is used.
    private float panScaleThresholdRive = 20f;

    //This must be the maximum zoom set in the timeline when the timeline is at 100%.
    //The smallest zoom at timeline 0% is assumed to be 100% zoom.
    // This must be less than maxPanRive to ensure full range panning at maximum zoom is possible.
    private float maxScaleRive = 250;

    //This is the maximum amount of pan (x and y transform) set by the panning timeline.
    private float maxPanRive = 1000f;

    //Timeline percentage (not actual zoom percentage) how much timeline percent zoom change for each click of a scroll wheel.
    //If the actual zoom percentage has a range of 100% to 1000%, then 5.5556 gives 50% zoom steps (100%, 150%, 200%, etc).
    private float scaleTimelineScrollChange = 5.55555556f;

    private bool isZoomIn = false;

    private Vector2 startPrimaryFingerPosRive;
    private Vector2 panTimelineStart;
    private float startTouchDistanceRive;
    private float scaleTimelineStart = 0;
    private Vector2 startMidPosRive;

    private RiveScreenMod m_riveScreen;

    private int activeTouchCountPrev;

    private float scaleTimeline = 0f;
    private float scalePrev = 0f;
    private Vector2 panTimeline;
    private float panXPercentPrev = 0f;
    private float panYPercentPrev = 0f;

    private RiveScreenMod riveScreenMod;

    private SMIBool scaleChangedRef;
    private SMIBool panXPercentChangedRef;
    private SMIBool panYPercentChangedRef;
    private  SMINumber scaleRef;
    private SMINumber panXPercentRef;
    private SMINumber panYPercentRef;

    private bool forceUpdate = false;

    void Start() {

        riveScreenMod = gameObject.GetComponent<RiveScreenMod>();
        m_riveScreen = gameObject.GetComponent<RiveScreenMod>();

        //The timeline zoom is not a zoom percentage but a percentage of the timeline range.
        //The range is from 0 to 100, which equates to 100% zoom to 1000% zoom, or whatever the zoom is set to in the timeline.
        scaleTimeline = 0;

        panTimeline = new Vector2(50f, 50f);

        //The panning range set in Rive (via a timeline) must be large enough to fit the zoom range. 
        //This function will give the pan range required.
        float panRangeRequired = GetPanRangeRequired(maxScaleRive);
        Debug.Log("Max pan range required: " + panRangeRequired);
    }

    void Update(){

        Init();

        if ( scaleRef != null ) {

            forceUpdate = true;

            //Pan and scale
            scalePrev = SetScale(scaleTimeline, scalePrev);
            panXPercentPrev = SetPanXPercent(panTimeline.x, panXPercentPrev);
            panYPercentPrev = SetPanYPercent(panTimeline.y, panYPercentPrev);
        }
    }

    public void PanZoomLogic(int activeTouchCount, Vector2 primaryFingerPosUnity, Vector2 secondaryFingerPosUnity, Vector2 primaryFingerPosRive, Vector2 secondaryFingerPosRive) {

        //Pan start
        if ( (activeTouchCount == 1) && ((activeTouchCountPrev == 0) || (activeTouchCountPrev > 1)) ) {

            panTimelineStart = panTimeline;
            scaleTimelineStart = scaleTimeline;
            startPrimaryFingerPosRive = primaryFingerPosRive;
        }

        //Panning
        if ( activeTouchCount == 1 ) {

            Pan(ref  panTimeline, primaryFingerPosRive, startPrimaryFingerPosRive, panTimelineStart,  scaleTimeline);
        }

        //Zoom start
        if ( (activeTouchCount == 2) && ((activeTouchCountPrev < 2) || (activeTouchCountPrev > 2)) ) {

            panTimelineStart =  panTimeline;
            scaleTimelineStart =  scaleTimeline;
            startTouchDistanceRive = Vector2.Distance(primaryFingerPosRive, secondaryFingerPosRive);

            // Compute the midpoint between the two fingers
            Vector2 unityFingerMid = (primaryFingerPosUnity + secondaryFingerPosUnity) / 2f;
            startMidPosRive = m_riveScreen.UnityToRivePos(unityFingerMid);

            m_riveScreen.isPanOrZoom = true;
        }

        //Zooming
        if ( activeTouchCount == 2 ) {

            Zoom(primaryFingerPosRive, secondaryFingerPosRive, primaryFingerPosUnity, secondaryFingerPosUnity);
        }

        //Mouse zooming
        MouseZoom(primaryFingerPosRive);

        activeTouchCountPrev = activeTouchCount;
    }



    void MouseZoom(Vector2 primaryFingerPosRive) {

        float scrollWheel = 0f;

        if ( Mouse.current != null ) {

            scrollWheel = Mouse.current.scroll.ReadValue().y;
        }

        if ( scrollWheel != 0 ) {

            panTimelineStart =  panTimeline;
            scaleTimelineStart =  scaleTimeline;

            isZoomIn = scrollWheel > 0;
            float scaleAddition = isZoomIn ? scaleTimelineScrollChange : -scaleTimelineScrollChange;

            float newScaleTimeline =  scaleTimeline + scaleAddition;
            newScaleTimeline = Mathf.Clamp(newScaleTimeline, 0f, 100f);

            scaleTimeline = newScaleTimeline;

            // Adjust pan to keep zoom centered at mouse position
            KeepZoomCenter(ref panTimeline, primaryFingerPosRive, primaryFingerPosRive, scaleTimelineStart, newScaleTimeline, panTimelineStart,  scaleTimeline);
        }
    }

    private void Zoom(Vector2 primaryFingerPosRive, Vector2 secondaryFingerPosRive, Vector2 primaryFingerPosUnity, Vector2 secondaryFingerPosUnity) {

        float newTouchDistanceRive = Vector2.Distance(primaryFingerPosRive, secondaryFingerPosRive);

         scaleTimeline = GetScale(newTouchDistanceRive, startTouchDistanceRive, scaleTimelineStart);

        // Compute the midpoint between the two fingers
        Vector2 unityFingerMid = (primaryFingerPosUnity + secondaryFingerPosUnity) / 2f;
        Vector2 currentMidPosRive = m_riveScreen.UnityToRivePos(unityFingerMid);

        // Adjust pan to keep zoom centered at midpoint
        KeepZoomCenter(ref  panTimeline, currentMidPosRive, startMidPosRive, scaleTimelineStart,  scaleTimeline, panTimelineStart,  scaleTimeline);
    }

    private void Pan(ref Vector2 panTimeline, Vector2 primaryFingerPosRive, Vector2 startPrimaryFingerPosRive, Vector2 panTimelineStart, float scaleTimeline) {

        //Get a vector between the initial touch position and the current touch position. 
        Vector2 rivePixelDifference = startPrimaryFingerPosRive - primaryFingerPosRive;

        if ( (Vector2.Distance(primaryFingerPosRive, startPrimaryFingerPosRive) > panScaleThresholdRive) || m_riveScreen.isPanOrZoom ) {

            m_riveScreen.isPanOrZoom = true;

            //Flip y depending the platform.
            rivePixelDifference.y = m_riveScreen.IsDeviceTypeOpenGL() || m_riveScreen.FlipY() ? rivePixelDifference.y : -rivePixelDifference.y;

            //Convert from a timeline percentage to Rive pixels.
            Vector2 startRivePanPixels = LinearFull(panTimelineStart, 0f, -maxPanRive, 100f, maxPanRive);

            Vector2 newRivePanPixels = startRivePanPixels - rivePixelDifference;

            //Convert from Rive pixels to a timeline percentage.
            panTimeline = LinearFull(newRivePanPixels, -maxPanRive, 0f, maxPanRive, 100f);

            panTimeline = LimitPan(panTimeline, scaleTimeline);
        }
    }

    private void KeepZoomCenter(ref Vector2 panTimeline, Vector2 currentMidPosRive, Vector2 startMidPosRive, float scaleTimelineStart, float scaleTimelineEnd, Vector2 panTimelineStart, float scaleTimeline) {

        //Convert from a timeline percentage (range 0 to 100) to an actual pan value (range -1000 to 1000).
        Vector2 rivePixelStartPan = LinearFull(panTimelineStart, 0f, -maxPanRive, 100f, maxPanRive);

        //Convert from a timeline percentage (range 0 to 100) to an actual zoom value (range 100 to 1000).
        float riveScalePercentStart = LinearFull(scaleTimelineStart, 0f, 100f, 100f, maxScaleRive);
        float riveScalePercentEnd = LinearFull(scaleTimelineEnd, 0f, 100f, 100f, maxScaleRive);

        float scaleFactor = riveScalePercentEnd / riveScalePercentStart;

        //Get the new position caused by scaling.
        Vector2 scaleShiftPos = (currentMidPosRive - rivePixelStartPan) * scaleFactor;
        Vector2 rivePixelDifference = (scaleShiftPos + rivePixelStartPan) - currentMidPosRive;

        //Offset for two finger panning.
        Vector2 twoFingerPanOffset = currentMidPosRive - startMidPosRive;
        rivePixelDifference -= twoFingerPanOffset;

        //Flip y depending the platform.
        rivePixelDifference.y = m_riveScreen.IsDeviceTypeOpenGL() || m_riveScreen.FlipY() ? rivePixelDifference.y : -rivePixelDifference.y;

        //Convert from Rive pixel change to a timeline percentage change.
        Vector2 panTimelineDiff = (rivePixelDifference * 50f) / maxPanRive;

        panTimeline = panTimelineStart - panTimelineDiff;

        panTimeline = LimitPan(panTimeline, scaleTimeline);
    }

    //Limit the pan to prevent out of view.
    private Vector2 LimitPan(Vector2 panTimeline, float scaleTimeline) {

        //Convert from a timeline percentage (range 0 to 100) to an actual zoom value (range 100 to 1000).
        float riveScalePercent = LinearFull(scaleTimeline, 0f, 100f, 100f, maxScaleRive);

        Vector2 artboardPixelSize = new Vector2(m_riveScreen.m_artboard.Width, m_riveScreen.m_artboard.Height);

        //Adjust the artboard size for the scale.
        Vector2 maxScalePosShift = artboardPixelSize * (riveScalePercent / 100f);

        Vector2 maxScalePosDiff = maxScalePosShift - artboardPixelSize;

        //Convert from Rive pixel change to a timeline percentage change.
        Vector2 panTimelineDiff = (maxScalePosDiff * 50f) / maxPanRive;

        //Clamp a vector.
        Vector2 minClipPanTimeline = new Vector2(50f - panTimelineDiff.x, 50f - panTimelineDiff.y);
        panTimeline = ClampPan(panTimeline, minClipPanTimeline, 50f);

        return panTimeline;
    }

    private float GetScale(float newTouchDistanceRive, float startTouchDistanceRive, float scaleTimelineStart) {

        //Calculate the scale change percentage.
        float scaleChangePercent = (newTouchDistanceRive / startTouchDistanceRive) * 100f;

        //Convert from scale percent (range 100 to maxScaleRive) to a timeline (range 0 to 100)
        float scaleChangeTimeline = LinearFull(scaleChangePercent, 100f, 0f, maxScaleRive, 100f);

        float newScaleTimeline = scaleTimelineStart + scaleChangeTimeline;

        newScaleTimeline = Mathf.Clamp(newScaleTimeline, 0f, 100f);

        return newScaleTimeline;
    }

    private Vector2 ClampPan(Vector2 value, Vector2 min, float max) {

        float x = Mathf.Clamp(value.x, min.x, max);
        float y = Mathf.Clamp(value.y, min.y, max);

        return new Vector2(x, y);
    }

    private float GetPanRangeRequired(float maxScale) {

        //Find the highest number
        float maxArtboardSize = Mathf.Max(m_riveScreen.m_artboard.Width, m_riveScreen.m_artboard.Height);

        float translated = maxArtboardSize * (maxScale / 100f);

        float panRangeRequired = translated - maxArtboardSize;

        return panRangeRequired;
    }

            //Initializing Rive references.
    void Init() {

        if  ((riveScreenMod.m_stateMachine != null ) && ( scaleRef == null )) {

            scaleChangedRef =  riveScreenMod.m_stateMachine.GetBool("scale changed");
            panXPercentChangedRef =  riveScreenMod.m_stateMachine.GetBool("pan x changed");
            panYPercentChangedRef =  riveScreenMod.m_stateMachine.GetBool("pan y changed");

            scaleRef =  riveScreenMod.m_stateMachine.GetNumber("scale");
            panXPercentRef =  riveScreenMod.m_stateMachine.GetNumber("pan x");
            panYPercentRef =  riveScreenMod.m_stateMachine.GetNumber("pan y");
        }
    }

    private float SetScale(float timelinePercent, float previous) {

        if ( (timelinePercent != previous) || forceUpdate ) {

            scaleChangedRef.Value = true;
            scaleRef.Value = timelinePercent;

        } else {

            scaleChangedRef.Value = false;
        }

        return timelinePercent;
    }

    private float SetPanXPercent(float percent, float previous) {

        if ( (percent != previous) || forceUpdate ) {

            panXPercentChangedRef.Value = true;
            panXPercentRef.Value = percent;

        } else {

            panXPercentChangedRef.Value = false;
        }

        return percent;
    }

    private float SetPanYPercent(float percent, float previous) {

        if ( (percent != previous) || forceUpdate ) {

            panYPercentChangedRef.Value = true;
            panYPercentRef.Value = percent;

        } else {

            panYPercentChangedRef.Value = false;
        }

        return percent;
    }




    //Get y from a linear function, with x as an input. The linear function goes through points
    //Pxy on the left ,and Qxy on the right.
    public static float LinearFull(float x, float Px, float Py, float Qx, float Qy) {

        float y = 0f;

        float A = Qy - Py;
        float B = Qx - Px;

        float C = A / B;

        y = Py + (C * (x - Px));

        return y;
    }

    private Vector2 LinearFull(Vector2 x, float Px, float Py, float Qx, float Qy) {

        Vector2 y = Vector2.zero;

        float A = Qy - Py;
        float B = Qx - Px;

        float C = A / B;

        y.x = Py + (C * (x.x - Px));
        y.y = Py + (C * (x.y - Px));

        return y;
    }
}


