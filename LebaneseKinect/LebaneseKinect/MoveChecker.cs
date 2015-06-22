using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace LebaneseKinect
{
    class MoveChecker
    {
        //scoring window
        int scoring_window = 600;
        
        //bools
        bool rightFootCrossed = false;

        //Knees and ankles
        float lky, lkx, lkz, rky, rkx, rkz, lax, lay, laz, rax, ray, raz;

        //Hands
        float lhy, lhx, lhz, rhx, rhy, rhz;

        //Spine is the cnter of the torso
        float spineX, spineY;

        //Shoulders
        float rsx, rsy, rsz, lsx, lsy, lsz, csx, csy, csz;

        //Elbows
        float rex, rey, rez, lex, ley, lez;

        //Waist/Hips
        float wstrx, wstry, wstrz, wstlx, wstly, wstlz;

        //Head pos
        float hx, hy;

        public bool CheckMove(DanceMove move, TimeSpan currentTime)
        {
            //return -1 if move not triggered
            //otherwise, return the score for that move

            //TODO: call this somewhere else
            bool score = false;
            switch (move.GetName())
            {
                //TODO: There MUST be a better way of doing this than a huge switch statement
                #region "m/f left knee moves"
                case "FrontHop":
                    if (FrontHopTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "FrontLeftKneeLift" :
                    if (FrontLeftKneeLiftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeFrontAndFrontTorso" :
                    if (LeftKneeFrontAndFrontTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftFootLiftCross":
                    if (LeftFootLiftCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLift":
                    if (LeftKneeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndFrontTorsoAndLeftHand":
                    if (LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndLeftHand":
                    if (LeftKneeLiftAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndFrontTorso":
                    if (LeftKneeLiftAndFrontTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndCross":
                    if (LeftKneeLiftAndCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceLeft":
                    if (LeftKneeLiftFaceLeftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceRight":
                    if (LeftKneeLiftFaceRightTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceBack":
                    if (LeftKneeLiftFaceBackTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndUnderArm":
                    if (LeftKneeLiftAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeKick":
                    if (LeftKneeKickTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                #region "m/f right knee moves"
                case "RightFootLiftCross":
                    if (RightFootLiftCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeBack" :
                    if (RightKneeBackTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "rightKneeFrontAndFrontTorso":
                    if (RightKneeFrontAndFrontTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLift":
                    if (RightKneeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndCross":
                    if (RightKneeLiftAndCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndLeftHand":
                    if (RightKneeLiftAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndBackTorso": //this might not be real
                    if (RightKneeLiftAndBackTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndBackTorsoAndLeftHand": //this might not be real either
                    if (RightKneeLiftAndBackTorsoAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKick":
                    if (RightKneeKickTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKickUnderArm":
                    if (RightKneeKickAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftFaceLeft":
                    if (RightKneeLiftFaceLeftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "BackSpinRightKneeLift":
                case "RightKneeLiftFaceBack":
                    if (RightKneeLiftFaceBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                #region "male special moves for Dance1"
                case "KneelAndClap":
                    if (KneelingTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKneelAndUnderArm":
                    if (RightKneeKneelAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKneelAndUnderArmAndLeftHandBehind":
                    if (RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "WaiterHand":
                    if (MoveToRightAndWaiterHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ShrugShoulders":
                    if (ShrugShouldersTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightFootSwing":
                    if (RightFootSwingTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightFootCross":
                    if (RightFootCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeBendCrouch":
                case "LeftKneeBendCrouchLeft":
                case "LeftKneeBendCrouchRight":
                    if (LeftKneeBendCrouchTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftHandToFaceSpinForward":
                    if (LeftHandToFaceSpinForwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftHandToFaceSpinBackward":
                    if (LeftHandToFaceSpinBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                #region "f moves dance 1"
                case "CrazyHands":
                    if (CrazyHandsTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "CrazyHandsCrouch":
                    if (CrazyHandsCrouchTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HandSwingFront":
                    if (RightHandToFaceSpinForwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HandSwingBack":
                    if (RightHandToFaceSpinBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HandSwingRight":
                    if (RightHandToFaceSpinForwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HandSwingLeft":
                    if (RightHandToFaceSpinBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HipShakeBack":
                    if (HipShakeFaceBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "HipShakeFront":
                    if (HipShakeFaceForwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "CrouchAndHipShake":
                    if (CrouchAndHipShakeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ElbowSway":
                case "RightElbowSway":
                    if (ElbowSwayRightTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftElbowSway":
                    if (ElbowSwayLeftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ScrollingHandsRight":
                    if (ScrollingHandsRightTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ScrollingHandsLeft":
                    if (ScrollingHandsRightTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftHandRaise":
                case "LeftWristArcRaise":
                    if (LeftHandRaiseTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightHandRaise":
                case "RightWristArcRaise":
                    if (RightHandRaiseTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "Home":
                    if (HandsOnHipsTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ThrillerHandsLeft":
                    if (ThrillerHandsLeftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftBendHipShake":
                    if (LeftBendHipShakeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                default:
                    //GLOBALS.writer.WriteLine(move.GetName() + " unknown");
                    move.ScoreMove(currentTime);
                    break;
            }

            return score;
        }

        public bool ScoreMove(DanceMove move, TimeSpan currentTime)
        {
            return true;
            //double diff = Math.Abs((currentTime.Subtract(move.moveSpan).TotalMilliseconds));
            //return (GLOBALS.SCORING_WINDOW - diff) > 0;
        }

        #region Individual Move Check Functions
        //Left knee lift move, total of 24 for the male

        /** NEW **/
        public bool LeftFootLiftCrossTriggered()
        {
            //Knee crossed via z-distance, left ankle is crossed over right via x-axis
            return LeftKneeCrossTriggered() && (lax > rax);
        }
        public bool RightFootLiftCrossTriggered()
        {
            //Knee crossed via z-distance, right ankle is crossed over left via x-axis
            return RightKneeCrossTriggered() && (rax < lax);
        }
        public bool FrontHopTriggered()
        { 
            //Has the player moved forward relative to the Kinect
            return wstlz > -0.01 && wstrz > -0.01;
        }
        public bool RightKneeBackTriggered()
        {
            //Right ankle behind and above left ankle 
            return raz > laz && ray > lay;
        }
        public bool FrontLeftKneeLiftTriggered()
        {
            return LeftFootLiftCrossTriggered();
        }
        public bool LeftKneeFrontAndFrontTorsoTriggered()
        {
            //Left knee in front of right (assuming forward is negative)
            return lkz < rkz && FaceLeftTriggered();
        }
        public bool RightKneeFrontAndFrontTorsoTriggered()
        {
            //Right knee in front of left 
            return rkz < lkz && FaceRightTriggered();
        }
        public bool CrazyHandsTriggered()
        {
            //Simple check that hands are above elbows
            return (rhy - rey > 0.01) && (lhy > ley);
        }
        public bool CrazyHandsCrouchTriggered()
        {
            return CrazyHandsTriggered() && CrouchTriggered(); 
        }
        public bool LeftHandRaiseTriggered()
        {
            return lhy - hy > .15;
        }
        public bool RightHandRaiseTriggered()
        {
            return rhy - hy > .15;
        }
        private bool LeftKneeTriggered()
        {
            //same for male and female
            bool yes = (lky > rky && lay > ray && distance2d(lkx, rkx, lky, rky) > .2);
            if (yes)
                return true;
            else
                return false;
        }
        private bool FaceLeftTriggered()
        {
            return (rsz - lsz) < -.05; //?? TODO: Are these the same idea?
        }
        private bool FaceRightTriggered()
        {
            return (lsz - rsz) < -.05;
        }
        private bool FaceForwardTriggered()
        {
            return (rhx - lhx) > .25;
        }
        private bool FaceBackwardTriggered()
        {
            return (lhx - rhx) > .25;
        }
        private bool LeftKneeLiftAndFrontTorsoTriggered()
        {
            return LeftKneeTriggered() && FaceLeftTriggered();
        }
        private bool LeftHandToFaceTriggered()
        {
            return (Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25);
        }
        private bool LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()
        {
            return LeftHandToFaceTriggered() && LeftKneeLiftAndFrontTorsoTriggered();
        }
        private bool LeftKneeLiftAndLeftHandTriggered()
        {
            return LeftHandToFaceTriggered() && LeftKneeTriggered();
        }
        private bool LeftKneeCrossTriggered()
        {
            return (lkz - rkz) < -.05;
        }
        private bool RightKneeCrossTriggered()
        {
            return (rkz - lkz) < -.05;
        }
        private bool LeftKneeLiftAndCrossTriggered()
        {
            return LeftKneeTriggered() && LeftKneeCrossTriggered();
        }
        private bool LeftKneeLiftFaceLeftTriggered() //left knee lifted, facing left
        {
            return LeftKneeTriggered() && FaceLeftTriggered();
        }
        private bool LeftKneeLiftFaceRightTriggered() //left knee lifted, facing right
        {
            return LeftKneeTriggered() && FaceRightTriggered();
        }
        private bool LeftKneeLiftFaceBackTriggered() //left knee lifted, facing backwards
        {
            //Spinning back if left is on pos x
            return LeftKneeTriggered() && ((lsx - rsx) > .01);
        }
        private bool RightArmRaisedTriggered()//right arm and elbow above head
        {
            return (rhy > hy) && (rey > hy);
        }
        private bool LeftKneeLiftAndUnderArmTriggered()
        {
            return RightArmRaisedTriggered() && LeftKneeTriggered();
        }
        private bool RightKneeTriggered() //right knee lifted
        {
            return (rky > lky && ray > lay && distance2d(lkx, rkx, lky, rky) > .2);
        }
        private bool RightKneeLiftAndBackTorsoTriggered()
        {
            return !FaceLeftTriggered() && RightKneeTriggered();
        }
        private bool RightKneeLiftAndBackTorsoAndLeftHandTriggered()
        {
            return LeftHandToFaceTriggered() && RightKneeLiftAndBackTorsoTriggered();
        }
        private bool RightKneeLiftAndLeftHandTriggered()
        {
            return RightKneeTriggered() && LeftHandToFaceTriggered();
        }
        private bool RightKneeLiftAndCrossTriggered()
        {
            return RightKneeTriggered() && LeftKneeCrossTriggered();
        }
        private bool RightKneeLiftFaceLeftTriggered()
        {
            return RightKneeTriggered() && FaceLeftTriggered();
        }
        private bool RightKneeLiftFaceBackwardTriggered()
        {
            return RightKneeTriggered() && FaceBackwardTriggered();
        }
        private bool KneelingTriggered()
        {
            return distance2d(rkx, wstrx, rky, wstry) < .2 && (rky - lky) > .01;
        }
        private bool RightKneeKneelAndUnderArmTriggered()
        {
            return KneelingTriggered() && RightArmRaisedTriggered();
        }
        private bool LeftHandBehindWaistTriggered()
        {
            return (wstrz - lhz) < -.05;
        }
        private bool RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()
        {
            return RightKneeKneelAndUnderArmTriggered() && LeftHandBehindWaistTriggered();
        }
        private bool MoveToRightAndWaiterHandTriggered()
        {
            return !KneelingTriggered() && (rhy - rey) > 0.01;
        }
        private bool ShrugShouldersTriggered()
        {
            //TODO: Implement this step
            return true; //code to do this didn't work, cs is interpolated from ls and rs
        }
        private bool RightFootOverTriggered()
        {
            return distance2d(rax, lax, ray, lay) > .1;
        }
        private bool RightFootSwingTriggered() //RightFootSwing, if we've seen a cross, look for a swing back
        {
            if (rightFootCrossed && RightFootOverTriggered())
            {
                rightFootCrossed = false;
                return true;
            }
            return false;
        }
        private bool RightFootCrossTriggered() //RightFootSwing, if we've seen a cross, look for a swing back
        {
            if (!rightFootCrossed && !RightFootOverTriggered())
            {
                rightFootCrossed = true;
                return true;
            }
            return false;
        }
        private bool CrouchTriggered()
        {
            return (rhy - wstry) > .01 && (lhy - wstly) > 0.01;
        }
        private bool LeftKneeBendCrouchTriggered()
        {
            return (lkz - wstrz) < -.05; //TODO: Test this step.
        }
        private bool LeftHandToFaceSpinForwardTriggered()
        {
            return LeftHandToFaceTriggered() && FaceForwardTriggered();
        }
        private bool LeftHandToFaceSpinBackwardTriggered()
        {
            return LeftHandToFaceTriggered() && FaceBackwardTriggered();
        }

        private bool RightHandToFaceSpinForwardTriggered()
        {
            return RightHandToFaceTriggered() && FaceForwardTriggered();
        }
        private bool RightHandToFaceSpinBackwardTriggered()
        {
            return RightHandToFaceTriggered() && FaceBackwardTriggered();
        }
        private bool RightHandToFaceSpinLeftTriggered()
        {
            return RightHandToFaceTriggered() && FaceLeftTriggered();
        }
        private bool RightHandToFaceSpinRightTriggered()
        {
            return RightHandToFaceTriggered() && FaceRightTriggered();
        }

        private bool RightKneeKickTriggered()
        {
            return ((rkz - wstrz) < -.05) && ((raz - rkz) < -.05);
        }
        private bool LeftKneeKickTriggered()
        {
            return ((lkz - wstrz) < -.05) && ((laz - lkz) < -.05);
        }
        private bool RightKneeKickAndUnderArmTriggered()
        {
            return RightArmRaisedTriggered() && RightKneeKickTriggered();
        }

        private bool ForwardSpinFacingRightKneeLiftTriggered()
        {
            return FaceForwardTriggered() && RightKneeTriggered();
        }

        private bool RightHandToFaceTriggered()
        {
            return (Math.Abs(hx - rhx) < .25) && (Math.Abs(hy - rhy) < .25);
        }

        private bool HipShakeTriggered()
        {
            return (((wstrx - rkx) > .01 && (wstlx - lkx) > .01)
                                    || ((rkx - wstrx) > .01 && (lkx - wstlx) > .01));
        }

        private bool HipShakeFaceForwardTriggered()
        {
            return HipShakeTriggered() && FaceForwardTriggered();
        }

        private bool HipShakeFaceBackwardTriggered()
        {
            return HipShakeTriggered() && FaceBackwardTriggered();
        }

        private bool CrouchAndHipShakeTriggered()
        {
            return HipShakeTriggered() && CrouchTriggered();
        }

        private bool RightHandOutTriggered()
        {
            return Math.Abs(rhy - rey) < .05;
        }

        private bool LeftHandOutTriggered()
        {
            return Math.Abs(lhy - ley) < .05;
        }

        private bool LeftHandAboveRightHandTriggered()
        {
            return (lhy - rhy) > .01;
        }

        private bool RightHandAboveLeftHandTriggered()
        {
            return (rhy - lhy) > .01;
        }

        private bool ScrollingHandsRightTriggered()
        {
            return LeftHandToFaceTriggered() && RightHandOutTriggered() && LeftHandAboveRightHandTriggered();
        }

        private bool ScrollingHandsLeftTriggered()
        {
            return RightHandToFaceTriggered() && LeftHandOutTriggered() && RightHandAboveLeftHandTriggered();
        }

        private bool HandsOnHipsTriggered()
        {
            return rhy < spineY && lhy < spineY && ley > wstly && rey > wstry;
        }

        private bool ElbowSwayLeftTriggered()
        {
            return HandsOnHipsTriggered() && FaceLeftTriggered();
        }

        private bool ElbowSwayRightTriggered()
        {
            return HandsOnHipsTriggered() && FaceRightTriggered();
        }

        private bool ThrillerHandsLeftTriggered()
        {
            //(rhy > spineY && lhy > spineY)
            return FaceLeftTriggered() && LeftHandOutTriggered();
        }

        private bool BendLeftTriggered()
        {
            //(Math.Abs(rhx - rkx) > .1)
            return (Math.Abs(csx - wstlx) > .1 || Math.Abs(csx - wstrx) > .1);
        }

        private bool LeftBendHipShakeTriggered()
        {
            return FaceLeftTriggered() && BendLeftTriggered();
        }

        #endregion

        private double distance2d(float x1, float x2, float y1, float y2)
        {
            double ydis = y1 - y2;
            double xdis = x1 - x2;
            double result = Math.Sqrt(((xdis * xdis) + (ydis * ydis)));
            return result;
        }

        public void UpdateSkeleton(Skeleton skeleton)
        {
            //if time has not passed since last skeleton update? return

            //otherwise set up all the values
            //Knees and ankles
             lky = skeleton.Joints[JointType.KneeLeft].Position.Y;
             lkx = skeleton.Joints[JointType.KneeLeft].Position.X;
             lkz = skeleton.Joints[JointType.KneeLeft].Position.Z;
             rkx = skeleton.Joints[JointType.KneeRight].Position.X;
             rky = skeleton.Joints[JointType.KneeRight].Position.Y;
             rkz = skeleton.Joints[JointType.KneeRight].Position.Z;
             lax = skeleton.Joints[JointType.AnkleLeft].Position.X;
             lay = skeleton.Joints[JointType.AnkleLeft].Position.Y;
             laz = skeleton.Joints[JointType.AnkleLeft].Position.Z;
             rax = skeleton.Joints[JointType.AnkleRight].Position.X;
             ray = skeleton.Joints[JointType.AnkleRight].Position.Y;
             raz = skeleton.Joints[JointType.AnkleRight].Position.Z;

            //Hands
             lhy = skeleton.Joints[JointType.HandLeft].Position.Y;
             lhx = skeleton.Joints[JointType.HandLeft].Position.X;
             lhz = skeleton.Joints[JointType.HandLeft].Position.Z;
             rhy = skeleton.Joints[JointType.HandRight].Position.Y;
             rhx = skeleton.Joints[JointType.HandRight].Position.X;

            //Spine is the cnter of the torso
             spineX = skeleton.Joints[JointType.Spine].Position.X;
             spineY = skeleton.Joints[JointType.Spine].Position.Y;

            //Shoulders
             rsx = skeleton.Joints[JointType.ShoulderRight].Position.X;
             rsy = skeleton.Joints[JointType.ShoulderRight].Position.Y;
             rsz = skeleton.Joints[JointType.ShoulderRight].Position.Z;
             lsx = skeleton.Joints[JointType.ShoulderLeft].Position.X;
             lsy = skeleton.Joints[JointType.ShoulderLeft].Position.Y;
             lsz = skeleton.Joints[JointType.ShoulderLeft].Position.Z;
             csx = skeleton.Joints[JointType.ShoulderCenter].Position.X;
             csy = skeleton.Joints[JointType.ShoulderCenter].Position.Y;

            //Elbows
             rex = skeleton.Joints[JointType.ElbowRight].Position.X;
             rey = skeleton.Joints[JointType.ElbowRight].Position.Y;
             rez = skeleton.Joints[JointType.ElbowRight].Position.Z;
             lex = skeleton.Joints[JointType.ElbowLeft].Position.X;
             ley = skeleton.Joints[JointType.ElbowLeft].Position.Y;
             lez = skeleton.Joints[JointType.ElbowLeft].Position.Z;

            //Waist
             wstrx = skeleton.Joints[JointType.HipRight].Position.X;
             wstry = skeleton.Joints[JointType.HipRight].Position.Y;
             wstrz = skeleton.Joints[JointType.HipRight].Position.Z;
             wstlx = skeleton.Joints[JointType.HipLeft].Position.X;
             wstly = skeleton.Joints[JointType.HipLeft].Position.Y;
             wstlz = skeleton.Joints[JointType.HipLeft].Position.Z;

            //Head pos
             hx = skeleton.Joints[JointType.Head].Position.X;
             hy = skeleton.Joints[JointType.Head].Position.Y;
        }
    }
}
