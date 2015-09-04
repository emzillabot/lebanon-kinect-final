#define USE_KINECT // Comment out this line to test without a Kinect!!!
//irene version

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using SkinnedModel;
using System.Timers;

#if USE_KINECT
using Microsoft.Kinect;
#endif


namespace LebaneseKinect
{
    /// <summary>
    /// Lebanese Kinect Game - "Being"
    /// This game teaches the user a simple traditional Arab dance called dabke.
    /// Currently only displays and rotates player model
    /// </summary>
    public class LebaneseKinectGame : Microsoft.Xna.Framework.Game
    {
        
        //Important Drew Hicks Refactor
        enum GameState { ATTRACT, MENU, HOWTOPLAY, DANCE, SCORE, CONTINUE, QUIT };
        int gameState = (int)GameState.ATTRACT;

        Dance dance1 = new Dance("Lebanon");
        Dance dance2 = new Dance("Lebanon2");
        Dance dance3 = new Dance("Lebanon3Actual");
        Dance selectedDance;
        int selectedDanceNum = 0;

        // Declaring variables...
        bool bWantsToQuit = false;
        bool keydown = false;

        const int SCORING_WINDOW_EASY = 600;
        const int SCORING_WINDOW_MEDIUM = 300;
        const int SCORING_WINDOW_HARD = 150;
        //int GLOBALS.SCORING_WINDOW = SCORING_WINDOW_MEDIUM;

#if USE_KINECT
        KinectSensor kinect;
        Skeleton[] skeletonData;

        int P1skeleton = -1; //keep tracking IDs for players
        int P2skeleton = -1; //keep tracking IDs for players

        Skeleton P1skeldata = null;
        Skeleton P2skeldata = null;

        Boolean debugging = true;
#endif

        const int WINDOW_WIDTH = 640;//720;
        const int WINDOW_HEIGHT = 480;

        // Basic XNA 3d variables
        GraphicsDeviceManager graphics;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.5f, -1.35f);

        Effect kinectDepthVisualizer;
        bool needToRedrawBackBuffer;
        Rectangle shadowRect = new Rectangle(0, 0, 64, 64);
        RenderTarget2D backBuffer;
        Texture2D depthTexture;
        short[] depthData;


        // FSM variables
        SpriteBatch spriteBatch; //Draws all 2d images and text
        Texture2D jointTexture, shadowTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Stopwatch videoTime = new Stopwatch();
        Stopwatch loopTime = new Stopwatch();
        Stopwatch playSessionTime = new Stopwatch();
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5), new Vector2(355, 5) }; //currently unused


        // Dancer Sprite Variables
        Texture2D female_BackSpinRightKneeLift, female_crossover, female_Crouch_HipShake, female_Crouch_HipSwivel;
        Texture2D female_ElbowSway, female_HandSwingBack, female_HandSwingFront, female_HandSwingLeft, female_HandSwingRight;
        Texture2D female_HipShakeBack, female_HipShakeFront, female_Home, female_LeftBendHipShake, female_LeftKneeBendCrouch_Left;
        Texture2D female_LeftKneeBendCrouch_Right, female_LeftKneeKick, female_LeftKneeLift, female_LeftKneeLift_FaceLeft;
        Texture2D female_LeftKneeLift_FrontTorso, female_RightHandHigh, female_rightKneeKick, female_RightKneeLift;
        Texture2D female_RightKneeLift_FaceLeft, female_ScrollingHands_Left, female_ScrollingHands_Right;
        Texture2D female_ThrillerhandsLeft, female_WristArcRaise_Left, female_WristArcRaise_Right;

        Texture2D male_crossover, male_KneelandClap, male_LeftHandToFace_Spin, male_LeftKneeBendCrouch_Left;
        Texture2D male_LeftKneeBendCrouch_Right, male_LeftKneeCross, male_LeftKneeKick, male_LeftKneeLift;
        Texture2D male_LeftKneeLift_FaceBack, male_LeftKneeLift_FaceLeft, male_LeftKneeLift_FaceRight;
        Texture2D male_LeftKneeLift_FrontTorso, male_LeftKneeLift_FrontTorso_LeftHand, male_LeftKneeLift_LeftHand;
        Texture2D male_LeftKneeLift_UnderArm, male_RightKneeCross, male_rightKneeKick, male_RightKneeKick_UnderArm;
        Texture2D male_RightKneeKneel_UnderArm, male_RightKneeKneel_UnderArm_HandBehind, male_rightKneeLift;
        Texture2D male_RightKneeLift_FaceRight, male_RightKneeLift_LeftHand, male_shrug, male_WaiterHand;

        Texture2D n_MoveTargetNew;
        Texture2D n_MoveTarget;
        Texture2D n_P1icon;
        Texture2D n_P2icon;
        Texture2D n_P1Join;
        Texture2D n_P2Join;

        /* Keyboard controls
         *  Up arrow: kick
         *  Down arrow: home
         *  Left/Right Arrow: cross
         */

        bool bSpaceKeyPressed = false;
        bool bCrossoverKeyPressed = false;
        bool bHomeKeyPressed = false;
        bool bKickKeyPressed = false;
        bool bDebugKeyPressed = false; //Display text, default is true
        bool bShowDebugText = true;

        /* Calculates the difference between the time at which it detects a user's action, and when the dancing animation does the same action.
         * TimeSpan(0, 0, 0, X, Y), X is seconds, Y is milliseconds.
         */
        //Expected dance move times;
        TimeSpan textFadeOut = new TimeSpan(0, 0, 0); // 2-second fadeout for result text

        bool rightFootCrossed = false;
        bool arcedHands = false;

        int stepsDone = 0;
        int tempStepsDone = 0;
        //
        TimeSpan stepFinished = new TimeSpan(0, 0, 3, 0, 0);
        TimeSpan danceVideoLength = new TimeSpan(0, 0, 0, 5, 0);
        
        TimeSpan femPlayerRecog = new TimeSpan(0, 0, 0, 0, 0);
        TimeSpan malePlayerRecog = new TimeSpan(0, 0, 0, 0, 0);

        bool femPlayingText = false;
        bool malePlayingText = false;

        SpriteFont font;
        SpriteFont resultFont;
        Color resultColor, resultColorF;
        string resultString = " ";
        string resultStringF = " ";
        string displayScoreText = " ";
        string displayScoreTextF = " ";
        string displayRecentScoreText = " ";
        string displayRecentScoreTextF = " ";
        int totalScore = 0;
        int totalScoreF = 0;
        int setScore = 0;
        int setScoreF = 0;
        int displayScore = 0;
        int displayScoreF = 0;
        int tempScore = 0;
        int tempScoreF = 0;
        int femStepsDone = 0;
        int tempFemStepsDone = 0;
     
        static int maleRectDiff = WINDOW_WIDTH / 2 - 130;
   
        TimeSpan gameEnd;
        TimeSpan restartGameLoop = new TimeSpan(0, 0, 0, 10, 000);
        TimeSpan showInstructions = new TimeSpan(0, 0, 0, 10, 0);

        public void setTime()
        {
            #region Scoring TimeSpans
          
            //Set to length of video 
            gameEnd = new TimeSpan(0, 0, 0, 136, 500);//136, 500?
            restartGameLoop = new TimeSpan(0, 0, 0, 10, 000);

            #endregion

            stepsDone = 0;
            femStepsDone = 0;
            tempStepsDone = 0;
            tempFemStepsDone = 0;
            rightFootCrossed = false;
            arcedHands = false;
            totalScore = 0;
            totalScoreF = 0;
            setScore = 0;
            setScoreF = 0;
            displayScore = 0;
            displayScoreF = 0;
            tempScore = 0;
            tempScoreF = 0;
        }


        Color p1JoinColor, p2JoinColor;
        string p1JoinString = "Player 1";
        string p2JoinString = "Player 2";
        TimeSpan p1textFadeOut = new TimeSpan(0, 0, 0);
        TimeSpan p2textFadeOut = new TimeSpan(0, 0, 0);

        Texture2D backgroundDabke, scoreBackground, scoreBackground2, scoreBackground3, continueBackground, howtoplay;
        Rectangle backgroundRect = new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        List<Rectangle> shadowRects = new List<Rectangle>();

        List<string> eventsTriggeredList;

        protected Song song;
        VideoPlayer videoPlayer, videoPlayer2;
        Video video;

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            eventsTriggeredList = new List<string>();
            //for (int i = 0; i < numberOfAnimationPlayers; i++)
            //shadowRects.Add(new Rectangle(0, 0, 64, 64));
        }

        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if USE_KINECT
            kinect = KinectSensor.KinectSensors[0];
            kinect.Start();
#endif

            base.Initialize();  // Base XNA init...

#if USE_KINECT
            try
            {
                // Init Kinect for skeletal tracking
                kinect = KinectSensor.KinectSensors[0];
                kinect.Start();
                Debug.WriteLineIf(debugging, kinect.Status);
                kinect.SkeletonStream.Enable();
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default; // Use Seated Mode
                kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinect_DepthFrameReady);
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_AllFramesReady);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            //kinect.ElevationAngle = 0;
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("myFont");
            resultFont = Content.Load<SpriteFont>("resultFont");

            howtoplay = Content.Load<Texture2D>("Textures\\instructions");

            kinectDepthVisualizer = Content.Load<Effect>("KinectDepthVisualizer");
            // Create and load all the dance step icons
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            shadowTexture = Content.Load<Texture2D>("Textures\\shadow");
            backgroundDabke = Content.Load<Texture2D>("Textures\\backgroundDabke");
            scoreBackground = Content.Load<Texture2D>("Textures\\scoreBackground");
            scoreBackground2 = Content.Load<Texture2D>("Textures\\scoreBackground2");
            scoreBackground3 = Content.Load<Texture2D>("Textures\\scoreBackground3");
            continueBackground = Content.Load<Texture2D>("Textures\\continueBackground");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");
            n_MoveTargetNew = Content.Load<Texture2D>("Sprites\\n_MoveTargetNew");
            n_MoveTarget = Content.Load<Texture2D>("Sprites\\n_MoveTarget");
            n_P1icon = Content.Load<Texture2D>("Textures\\lefthandraise");
            n_P2icon = Content.Load<Texture2D>("Textures\\righthandraise");
            n_P1Join = Content.Load<Texture2D>("Textures\\p1join");
            n_P2Join = Content.Load<Texture2D>("Textures\\p2join");

            song = Content.Load<Song>("Music\\dabke");
            //MediaPlayer.Play(song);

            dance1.LoadContent(Content);
            dance2.LoadContent(Content);
            dance3.LoadContent(Content);

            video = Content.Load<Video>("Video\\Lebanon");
            videoPlayer = new VideoPlayer();
            videoPlayer.Play(video);
            playSessionTime.Start();
            videoPlayer.IsLooped = true;

            switch (selectedDanceNum % 3)
            {
                case 0:
                    selectedDance = dance1;
                    break;
                case 1:
                    selectedDance = dance2;
                    break;
                case 2:
                    selectedDance = dance3;
                    break;
                default:
                    selectedDance = dance1;
                    break;
            }

            gameEnd = selectedDance.GetMovie().Duration;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
            Content.Unload();
        }

        protected void resetDance() 
        {

            gameState = (int)GameState.ATTRACT;
           
            dance1 = new Dance("Lebanon");
            dance2 = new Dance("Lebanon2");
            dance3 = new Dance("Lebanon3Actual");

            dance1.LoadContent(Content);
            dance2.LoadContent(Content);
            dance3.LoadContent(Content);

            resultString = " ";
            resultStringF = " ";
        }

        private void IncrementDance()
        {
            selectedDanceNum++;

            
            switch (selectedDanceNum % 3)
            {
                case 1:
                   resetDance();
                   selectedDance = dance1;
                   break;
                case 2:
                   resetDance();
                   selectedDance = dance2;
                   break;
                case 0:
                   resetDance();
                   selectedDance = dance3;
                   break;
                default:
                   resetDance();
                   selectedDance = dance1;
                   break;
            }

            if (videoPlayer2 != null)
                videoPlayer2.Dispose();

            loopTime.Reset();
            loopTime.Start();
            //GLOBALS.PLAYER_ONE_ACTIVE = true;
            //GLOBALS.PLAYER_TWO_ACTIVE = true;
            gameState = (int)GameState.HOWTOPLAY;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (bWantsToQuit)
                Exit();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                bWantsToQuit = true;
                //FGLOBALS.writer.Close();
#if USE_KINECT
                kinect.Dispose();
                
#endif
            }

            // Press spacebar to advance your current dance steps without actually dancing.
            // Helpful for testing, making videos, ect...
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!bSpaceKeyPressed)
                {
                    if (gameState == (int)GameState.ATTRACT)
                    {
                        // SPACEBAR stops the intro video, if it is playing...
                        //malePlaying = true; //generic man playing
                        videoPlayer.Dispose();
                        IncrementDance();

                    }
                    else if (gameState == (int)GameState.CONTINUE)
                    {
                        IncrementDance();
                    }
                    else
                    {
                        // Otherwise, SPACEBAR is our developer shortcut for getting the current time
                        int currentTime = (int)videoTime.ElapsedMilliseconds;
                        Debug.WriteLine("\nCurrent time in seconds:" + currentTime + "\n");
                        bSpaceKeyPressed = true;
                    }
                }
            }
            else
                bSpaceKeyPressed = false;

            if (gameState == (int)GameState.HOWTOPLAY)
            {
                if (restartGameLoop.CompareTo(loopTime.Elapsed) < 0 || Keyboard.GetState().IsKeyDown(Keys.A)) //when to start the video
                {
                    videoPlayer2 = new VideoPlayer();
                    videoPlayer2.Play(selectedDance.GetMovie());
                    gameState = (int)GameState.DANCE;
                    setTime();
                    loopTime.Reset();
                    videoTime.Start();
                }
            }

            if (gameState == (int)GameState.DANCE)
            {
    
                //easy dance editing
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    if (!keydown)
                    {
                        keydown = true;
                        TimeSpan now = videoTime.Elapsed;
                        //GLOBALS.writer.WriteLine("Gender, MoveName, " + now.Days + "," + now.Hours + "," + now.Minutes + "," + now.Seconds + "," + now.Milliseconds);
                    }
                }
                else
                {
                    keydown = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    if (!bCrossoverKeyPressed)
                    {
                        bCrossoverKeyPressed = true;
                    }
                }
                else
                {
                    bCrossoverKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    if (!bHomeKeyPressed)
                    {
                        bHomeKeyPressed = true;
                    }
                }
                else
                {
                    bHomeKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    if (!bKickKeyPressed)
                    {
                        bKickKeyPressed = true;
                    }
                }
                else
                {
                    bKickKeyPressed = false;
                }

                //crappy testing thing
                int newPts = 0;
                newPts = selectedDance.ScoreMovesFake(videoTime.Elapsed);
                if (newPts > 0)
                {
                    tempStepsDone++;
                    tempScore += newPts;
                }

                //score blocks (requires no kinect)
                if (selectedDance.ScoreBlockFake(videoTime.Elapsed))
                {
                    scorePlayer();
                    scorePlayerF();
                }
          
                //We are at the end of the dance video, time to display scores
               
                if (gameEnd.Subtract(videoTime.Elapsed).Milliseconds < 0)
                {
                    loopTime.Reset();
                    loopTime.Start();
                    videoTime.Reset();
                    videoPlayer2.Stop();
                    gameState = (int)GameState.SCORE;
                    GLOBALS.PLAYER_ONE_ACTIVE = false;
                    GLOBALS.PLAYER_TWO_ACTIVE = false;
                }
                //Cutting the end of Dance 2
                if (selectedDanceNum % 3 == 2 && (int)videoTime.ElapsedMilliseconds >= 134000)
                {
                    loopTime.Reset();
                    loopTime.Start();
                    videoTime.Reset();
                    videoPlayer2.Stop();
                    gameState = (int)GameState.SCORE;
                    GLOBALS.PLAYER_ONE_ACTIVE = false;
                    GLOBALS.PLAYER_TWO_ACTIVE = false;
                }
                //Cutting the end of Dance 3
                if (selectedDanceNum % 3 == 0 && (int)videoTime.ElapsedMilliseconds >= 104000)
                {
                    loopTime.Reset();
                    loopTime.Start();
                    videoTime.Reset();
                    videoPlayer2.Stop();
                    gameState = (int)GameState.SCORE;
                    GLOBALS.PLAYER_ONE_ACTIVE = false;
                    GLOBALS.PLAYER_TWO_ACTIVE = false;
                }

            }

            //Show Score, go to CONTINUE
            if (gameState == (int)GameState.SCORE && restartGameLoop.CompareTo(loopTime.Elapsed) < 0)
            {
                loopTime.Reset();
                loopTime.Start();

                //reset all players
                P1skeleton = -1;
                P2skeleton = -1;
                GLOBALS.PLAYER_ONE_ACTIVE = false;
                GLOBALS.PLAYER_TWO_ACTIVE = false;

                gameState = (int)GameState.CONTINUE;
                
            }

            //Loop back to the start
            if (gameState == (int)GameState.CONTINUE && restartGameLoop.CompareTo(loopTime.Elapsed) < 0)
            {
                //reset all players
                P1skeleton = -1;
                P2skeleton = -1;
                GLOBALS.PLAYER_ONE_ACTIVE = false;
                GLOBALS.PLAYER_TWO_ACTIVE = false;

                videoPlayer2.Dispose();
                videoPlayer = new VideoPlayer();
                videoPlayer.IsLooped = true;
                videoPlayer.Play(video);
                selectedDanceNum = 0;
                gameState = (int)GameState.ATTRACT;
                videoTime.Reset();
                loopTime.Reset();
                playSessionTime.Reset();
                playSessionTime.Start();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (!bDebugKeyPressed)
                {
                    bDebugKeyPressed = true;
                    bShowDebugText = !bShowDebugText;
                }
            }
            else
            {
                bDebugKeyPressed = false;
            }

            if (totalScore > displayScore)
                displayScore += 11;

            if (totalScore < displayScore)
                displayScore = totalScore;

            if (totalScoreF > displayScoreF)
                displayScoreF += 11;

            if (totalScoreF < displayScoreF)
                displayScoreF = totalScoreF;

            base.Update(gameTime);  // Base XNA update...
        }

        private void scorePlayer()
        {
            //if (!malePlaying) return;
            setScore = tempScore;
            tempScore = 0;
            if (setScore > (tempStepsDone * 200))
            {
                resultColor = Color.Cyan;
                resultString = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > (tempStepsDone * 100))
            {
                resultColor = Color.Green;
                resultString = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else// if (setScore > (stepsDone*100))
            {
                resultColor = Color.Yellow;
                resultString = "Okay";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }

            if (setScore > 400)
            {
                displayRecentScoreText = "+" + setScore.ToString();
                totalScore += setScore;
            }
            else
            {
                displayRecentScoreText = " ";
                setScore = 0;
            }
            tempStepsDone = 0;
        }

        private void scorePlayerF()
        {
            if (!GLOBALS.PLAYER_TWO_ACTIVE) return;
            setScoreF = tempScoreF;
            tempScoreF = 0;
            if (setScoreF > (tempFemStepsDone * 200))
            {
                resultColorF = Color.Cyan;
                resultStringF = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScoreF > (tempFemStepsDone * 100))
            {
                resultColorF = Color.Green;
                resultStringF = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else// if (setScore > (stepsDone*100))
            {
                resultColorF = Color.Yellow;
                resultStringF = "Okay";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }

            if (setScoreF > 400)
            {
                displayRecentScoreTextF = "+" + setScoreF.ToString();
                totalScoreF += setScoreF;
            }
            else
            {
                displayRecentScoreTextF = " ";
                setScoreF = 0;
            }
            tempFemStepsDone = 0;
        }

        private void scoreDurationMove(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            int modifiedScore;
            if (earnedScore > 400)
            {
                modifiedScore = 400;
            }
            else
            {
                modifiedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            modifiedScore = Math.Max(modifiedScore, 100);
            tempScore += modifiedScore;
            stepsDone++;
            tempStepsDone++;
        }
        private void scoreDurationMoveF(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            int modifiedScore;
            if (earnedScore > 400)
            {
                modifiedScore = 400;
            }
            else
            {
                modifiedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            modifiedScore = Math.Max(modifiedScore, 100);
            tempScoreF += modifiedScore;
            femStepsDone++;
            tempFemStepsDone++;
        }

        private void scoreMove(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            if (earnedScore > 400)
            {
                tempScore += 400;
            }
            else
            {
                tempScore += Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            stepsDone++;
            tempStepsDone++;
        }
        private void scoreMoveF(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            if (earnedScore > 400)
            {
                tempScoreF += 400;
            }
            else
            {
                tempScoreF += Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            femStepsDone++;
            tempFemStepsDone++;
        }

      

        private void AddEventTriggeredText(string val)
        {
            eventsTriggeredList.Add(val);
            if (eventsTriggeredList.Count > 10)
                eventsTriggeredList.Remove(eventsTriggeredList[0]);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            // Default XNA draw stuff...
            GraphicsDevice.Clear(Color.White);

            //device.Clear(Color.CornflowerBlue);

            // Intro video is playing...
            if (gameState == (int)GameState.ATTRACT)
            {
                spriteBatch.Begin();
                Texture2D texture = videoPlayer.GetTexture();
                if (texture != null)
                {
                    // Draw intro video
                    //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                }
                spriteBatch.End();
                DrawText();
            }
            else if (gameState == (int)GameState.HOWTOPLAY)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(howtoplay, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                spriteBatch.End();
            }
            else if (gameState == (int)GameState.SCORE)
            {
                spriteBatch.Begin();
                if (scoreBackground != null)
                {
                    // Draw score screen over top
                    switch (selectedDanceNum % 3)
                    {
                        case 1:
                            spriteBatch.Draw(scoreBackground, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                            break;
                        case 2:
                            spriteBatch.Draw(scoreBackground2, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                            break;
                        case 0:
                            spriteBatch.Draw(scoreBackground3, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                            break;
                        default:
                            spriteBatch.Draw(scoreBackground, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                            break;
                    }

                }
                //write the score to screen using DisplayScore
                displayScoreTextF = String.Format("{0,5}", displayScoreF);
                DrawDebugString(resultFont, Color.White, (int)(WINDOW_WIDTH - 15 - resultFont.MeasureString(displayScoreTextF).X), 100, displayScoreTextF);

                displayScoreText = String.Format("{0,5}", displayScore);
                DrawDebugString(resultFont, Color.White, (int)(15), 100, displayScoreText);

                String rating, ratingF;
                Color ratingC, ratingFC;
                //Display rating of how they did
                if (displayScore > stepsDone * 180)
                {
                    rating = "DANCE STAR!";
                    ratingC = Color.Cyan;
                }
                else if (displayScore > stepsDone * 100)
                {
                    rating = "Professional";
                    ratingC = Color.Green;
                }
                else
                {
                    rating = "In Training";
                    ratingC = Color.Yellow;
                }
                //Fem
                if (displayScoreF > femStepsDone * 180)
                {
                    ratingF = "DANCE STAR!";
                    ratingFC = Color.Cyan;
                }
                else if (displayScoreF > femStepsDone * 100)
                {
                    ratingF = "Professional";
                    ratingFC = Color.Green;
                }
                else
                {
                    ratingF = "In Training";
                    ratingFC = Color.Yellow;
                }
                DrawDebugString(resultFont, ratingFC, (int)(WINDOW_WIDTH - 5 - resultFont.MeasureString(ratingF).X), 400, ratingF);
                DrawDebugString(resultFont, ratingC, (int)(5), 400, rating);

                spriteBatch.End();
                
            }
            else if (gameState == (int)GameState.CONTINUE)
            {
                spriteBatch.Begin();
                if (continueBackground != null)
                {
                    // Draw score screen over top
                    spriteBatch.Draw(continueBackground, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                }
                spriteBatch.End();
            }
            else if (gameState == (int)GameState.DANCE) // Intro video is no longer playing...
            {


#if USE_KINECT
                //If we don't have a depth target, exit
                if (this.depthTexture == null)
                {
                    return;
                }
#else
                this.needToRedrawBackBuffer = true;
#endif

                //if (depthTexture != null && needToRedrawBackBuffer)
                if (this.needToRedrawBackBuffer)
                {
                    GraphicsDevice.SetRenderTarget(backBuffer);
                    GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
#if USE_KINECT
                    depthTexture.SetData<short>(depthData);
                    SharedSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, kinectDepthVisualizer);
#endif
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                    spriteBatch.Begin();
                    Texture2D texture = videoPlayer2.GetTexture();
                    if (texture != null)
                    {
                        // Draw intro video
                        //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                        spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    }

                    //draw sprites
                    TimeSpan currentTime = videoTime.Elapsed;
                    double diff = 0.0;
                    //malestart
                    int xlocation = 0;

                    selectedDance.Draw(currentTime, spriteBatch); //I can draw moves from a move batch
                    float fadeout = (float)Math.Abs(1000.0f - (currentTime.TotalMilliseconds % 2000)) / 1000.0f; 

                    if (GLOBALS.PLAYER_ONE_ACTIVE)
                        spriteBatch.Draw(n_MoveTargetNew, new Rectangle(GLOBALS.LEFT_TARGET - 10, GLOBALS.DANCE_MOVE_Y - 10, 120, 120), Color.White);
                    else
                        spriteBatch.Draw(n_P1Join, new Rectangle(0, GLOBALS.WINDOW_HEIGHT - 100, 200, 100), Color.White * fadeout);
                    
                    if (GLOBALS.PLAYER_TWO_ACTIVE)
                        spriteBatch.Draw(n_MoveTargetNew, new Rectangle(GLOBALS.RIGHT_TARGET - 10, GLOBALS.DANCE_MOVE_Y - 10, 120, 120), Color.White);
                    else
                        spriteBatch.Draw(n_P2Join, new Rectangle(GLOBALS.WINDOW_WIDTH - 200, GLOBALS.WINDOW_HEIGHT - 100, 200, 100), Color.White * fadeout);
                
                    spriteBatch.End();

#if USE_KINECT
                    SharedSpriteBatch.Draw(depthTexture, Vector2.Zero, Color.White);
#endif
                    DrawText();
                    textFadeOut = textFadeOut.Subtract(gameTime.ElapsedGameTime);
                    p1textFadeOut = p1textFadeOut.Subtract(gameTime.ElapsedGameTime);
                    p2textFadeOut = p2textFadeOut.Subtract(gameTime.ElapsedGameTime);

#if USE_KINECT
                    SharedSpriteBatch.End();
#endif
                    GraphicsDevice.SetRenderTarget(null);
                    needToRedrawBackBuffer = false;
                }
#if USE_KINECT
                SharedSpriteBatch.Begin();
                SharedSpriteBatch.Draw(
                    this.backBuffer,
                    new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT),
                    null,
                    Color.White);
                DrawSkeletons(SharedSpriteBatch, new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT), jointTexture);
                //DrawSkeleton(SharedSpriteBatch, new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT), jointTexture); //draw mocap balls
                SharedSpriteBatch.End();
#endif
            }

            base.Draw(gameTime); // Draw base XNA stuff...
        }

        private void DrawSkeletons(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img)
        {
#if USE_KINECT
            // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect
            if (P1skeldata != null)
            {
                foreach (Joint joint in P1skeldata.Joints)
                {
                   Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Blue);
                }
            }
            if (P2skeldata != null)
            {
                foreach (Joint joint in P2skeldata.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Orange);
                }
            }
#endif
        }

        private double distance2d(float x1, float x2, float y1, float y2)
        {
            double ydis = y1 - y2;
            double xdis = x1 - x2;
            double result = Math.Sqrt(((xdis * xdis) + (ydis * ydis)));
            return result;
        }


        private void DrawDebugString(SpriteFont fontVal, Color colorVal, int x, int y, string stringVal)
        {
            Color textShadowColor = Color.Black;
            textShadowColor.A = colorVal.A;
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x + 2, y + 2), textShadowColor);
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x, y), colorVal);
        }

        private void DrawText()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            //spriteBatch.Begin();
            if (gameState == (int)GameState.ATTRACT)
            {
                String beingTitle = "Being: Lebanese Dance";
                beingTitle = String.Format("{0,5}", beingTitle);
                Vector2 beingSize = font.MeasureString(beingTitle);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH / 2.0 - (beingSize.X / 2.0)), 10, beingTitle);

                String beingTitle2 = "Raise a hand to play";
                beingTitle = String.Format("{0,5}", beingTitle2);
                Vector2 beingSize2 = font.MeasureString(beingTitle2);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH / 2.0 - (beingSize2.X / 2.0)), WINDOW_HEIGHT - 40, beingTitle2);

                spriteBatch.Draw(n_P1icon, new Rectangle((WINDOW_WIDTH / 2) - 270, WINDOW_HEIGHT - 150, 120, 150), Color.White);
                spriteBatch.Draw(n_P2icon, new Rectangle((WINDOW_WIDTH / 2) + 150, WINDOW_HEIGHT - 150, 120, 150), Color.White);

            }
            else if (bShowDebugText)
            {
                displayScoreTextF = String.Format("{0,5}", displayScoreF);
                //Vector2 scoreTextSize = resultFont.MeasureString(displayScoreText);
                //DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreTextSize.X), 10, displayScoreText);
                Vector2 scoreSize = font.MeasureString(displayScoreTextF);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreSize.X - 10), 10, displayScoreTextF);

                displayScoreText = String.Format("{0,5}", displayScore);
                Vector2 scoreSize2 = font.MeasureString(displayScoreText);
                DrawDebugString(font, Color.White, (int)(10), 10, displayScoreText);
            }

            double textFadeOutAmt = textFadeOut.TotalMilliseconds / 2000.0;

            if (gameState == (int)GameState.DANCE)
            {
                resultColor.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
                Vector2 resultSize = resultFont.MeasureString(resultString);
                DrawDebugString(resultFont, resultColor, (int)(10), 25, resultString);

                resultColorF.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
                Vector2 resultSizeF = resultFont.MeasureString(resultStringF);
                DrawDebugString(resultFont, resultColorF, (int)(WINDOW_WIDTH - 10 - resultSizeF.X), 25, resultStringF);
            }

            Color pColor = Color.White;
            double playerFadeOut = p1textFadeOut.TotalMilliseconds / 2000.0;
            pColor.A = (byte)Math.Max((255 * playerFadeOut), 0);
            if (!GLOBALS.PLAYER_ONE_ACTIVE) pColor.A = 0;
            DrawDebugString(resultFont, pColor, (int)(10), 25, p1JoinString);

            double playerFadeOut2 = p2textFadeOut.TotalMilliseconds / 2000.0;
            pColor.A = (byte)Math.Max((255 * playerFadeOut2), 0);
            Vector2 p2size = resultFont.MeasureString(p2JoinString);
            if (!GLOBALS.PLAYER_TWO_ACTIVE) pColor.A = 0;
            DrawDebugString(resultFont, pColor, (int)(WINDOW_WIDTH - p2size.X), 25, p2JoinString);

            spriteBatch.End();
            //GraphicsDevice.BlendState = DefaultBlendState;
        }


#if USE_KINECT
        void kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            //throw new NotImplementedException();
            //DepthImageFrame frame = e.OpenDepthImageFrame();
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                if (frame != null)
                {
                    if (null == depthData || depthData.Length != frame.PixelDataLength)
                    {
                        depthData = new short[frame.PixelDataLength];

                        depthTexture = new Texture2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Bgra4444);

                        backBuffer = new RenderTarget2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Color,
                            DepthFormat.None,
                            GraphicsDevice.PresentationParameters.MultiSampleCount,
                            RenderTargetUsage.PreserveContents);

                        spriteBatch = new SpriteBatch(GraphicsDevice);
                    }


                    frame.CopyPixelDataTo(depthData);
                    needToRedrawBackBuffer = true;
                }
            }
        }

        // This runs every time Kinect has an updated image
        void kinect_AllFramesReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            bool p1seen = false;
            bool p2seen = false;

            // Store all current Kinect skeletal info...
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        
                    }

                    //Copy the skeleton data to our array
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }
            
            // If there is valid skelton data, examine for dance steps
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                        //get whose skeleton it is
                    if (skel.TrackingId == P1skeleton)
                    {
                        P1skeldata = skel;
                        p1seen = true;
                    }
                    else if (skel.TrackingId == P2skeleton)
                    {
                        P2skeldata = skel;
                        p2seen = true;
                    }

                        //collect player data
                        if ((!GLOBALS.PLAYER_ONE_ACTIVE || !GLOBALS.PLAYER_TWO_ACTIVE))
                        {
                            if (skel.TrackingId != P1skeleton && skel.TrackingId != P2skeleton)
                            {
                                selectedDance.mc.UpdateSkeleton(skel);

                                if (!GLOBALS.PLAYER_ONE_ACTIVE && selectedDance.mc.LeftHandRaiseTriggered())
                                {
                                    GLOBALS.PLAYER_ONE_ACTIVE = true;
                                    p1seen = true;
                                    P1skeleton = skel.TrackingId;
                                    malePlayingText = true;
                                    p1textFadeOut = new TimeSpan(0, 0, 3); // 3-second fadeout
                                    malePlayerRecog = new TimeSpan(0, 0, 0, 0, 0);

                                    //do this somewhere else
                                    if (gameState == (int)GameState.ATTRACT)
                                    {
                                        videoPlayer.Dispose();
                                        IncrementDance();
                                    }
                                }

                                //right hand raised = play as female
                                if (!GLOBALS.PLAYER_TWO_ACTIVE && selectedDance.mc.RightHandRaiseTriggered())
                                {
                                    GLOBALS.PLAYER_TWO_ACTIVE = true;
                                    p2seen = true;
                                    P2skeleton = skel.TrackingId;
                                    femPlayingText = true;
                                    p2textFadeOut = new TimeSpan(0, 0, 3); // 3-second fadeout
                                    femPlayerRecog = new TimeSpan(0, 0, 0, 0, 0);

                                    //do this somewhere else
                                    if (gameState == (int)GameState.ATTRACT)
                                    {
                                        videoPlayer.Dispose();
                                        IncrementDance();
                                    }
                                }
                            }

                        }
                        else
                        {
                            //both players active!
                            int boop = 4;
                        }

                        #region replace this with same as above? might not need to since upon going to continue I reset players
                        if (gameState == (int)GameState.CONTINUE)
                        {
                            if (GLOBALS.PLAYER_ONE_ACTIVE || GLOBALS.PLAYER_TWO_ACTIVE)
                                IncrementDance();
                        }
                        #endregion

                        //new scoring code
                        if (gameState == (int)GameState.DANCE)
                        {
                            if (skel.TrackingId != P1skeleton && skel.TrackingId != P2skeleton)
                                continue;

                            int player = 1;
                            if (skel.TrackingId == P2skeleton)
                                player = 2;

                            selectedDance.mc.UpdateSkeleton(skel);
                            int newPts = 0;
                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                newPts = selectedDance.ScoreMoves(videoTime.Elapsed, skel, player);
                            }

                            if (newPts > 0)
                            {
                                if (player == 1)
                                {
                                    tempScore += newPts;
                                    tempStepsDone++;
                                }
                                else
                                {
                                    tempScoreF += newPts;
                                    tempFemStepsDone++;
                                }
                            }
                        }
                    }                           
                }

            if (!p1seen)
            {
                GLOBALS.PLAYER_ONE_ACTIVE = false;
                P1skeldata = null;
                P1skeleton = -1;
            }
            if (!p2seen)
            {
                GLOBALS.PLAYER_TWO_ACTIVE = false;
                P2skeldata = null;
                P2skeleton = -1;
            }
        }

        /// <summary>
        /// This method maps a SkeletonPoint to the depth frame.
        /// </summary>
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the depth frame.</returns>
        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            if ((null != kinect) && (null != kinect.DepthStream))
            {
                // This is used to map a skeleton point to the depth image location
                var depthPt = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(point, kinect.DepthStream.Format);
                return new Vector2(depthPt.X, depthPt.Y);
            }

            return Vector2.Zero;
        }
#endif
    }
}