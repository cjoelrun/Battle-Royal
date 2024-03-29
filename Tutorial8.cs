/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;
using GoblinXNA.UI.UI2D;
using GoblinXNA.UI;



namespace Tutorial8___Optical_Marker_Tracking
{
    class Card
    {
        ///variables

        /*type will store one of three values: 'M', 'S' or 'T'
         *defining the card as type monster, spell or trap respectively.*/
        static char type;

        /*model will store the data pertaining to the display of the monster
         * on the marker representing this paricular Card.*/
        MarkerNode model;
        //TransformNode sprite;
        static int baseAtk;
        int lastAtk;
        int attackPower;
        int health;
        int defaultHealth;
        bool ko;
        string name;
        string effect;

        /*
         * contructor: initializes Card data structure.
         * parameters:
         *              marker (int) - marker number that displays card
         *              type (char) - type of monster, can be 'M', 'S', or 'T'
         *              model (TransformNode) - TransformNode displayed on read marker
         *              atk (int) - attack power of monster
         *              health (int) - health of monster
         *              
         * */
        public Card(char ntype, MarkerNode nmodel, int atk, int nhealth, string nname, string neffect)
        {
            type = ntype;
            model = nmodel;
            //sprite = nsprite; --> add sprite parameter?
            attackPower = atk;
            baseAtk = atk;
            lastAtk = atk;
            health = nhealth;
            defaultHealth = health;
            ko = false;
            name = nname;
            effect = neffect;
        }

        //getType fetches type and returns it.
        public char getType()
        {
            return type;
        }

        //getModel fetches model and returns it
        public MarkerNode getModel()
        {
            return model;
        }

        /* DEBATABLE METHOD:
         * In the case that the model of the monster would have to be changed somehow
         * (i.e color of monster needs to be changed),
         * this method exists to change the model in its entirety.
         * parameter: newModel (TransformNode)
         * 
         * 
        public void setSprite(TransformNode newSprite)
        {
            sprite = newSprite;
        }*/

        /* isAttackingMonster exists to update damage given to another monster
         * parameter: target (Card data structure)
         * NOTE: no need for an isBeingAttacked method since this method will 
         * always be called by the attacking monster to edit values in the target monster.
         * */
        public void attacking(Card target)
        {
            target.takeDamage(attackPower);
        }

        /* takeDamage is used to register damage taken and subtract from the health pool of a given monster.
         * parameter: amount (int)
         * return: int value holding amount of damage done to owner of destroyed card. 
         * */
        public void takeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                destroy();
            }
        }

        public void setHealth(int amount)
        {
            health += amount;
            if (health > defaultHealth)
                health = defaultHealth;
        }
        public int getDefaultHealth()
        {
            return defaultHealth;
        }

        public int getAttackPower()
        {
            return attackPower;
        }

        public void buff(int amount)
        {
            lastAtk = attackPower;
            attackPower += amount;
        }
        public void debuff(int amount)
        {
            lastAtk = attackPower;
            attackPower -= amount;
            if (attackPower < 0)
                attackPower = 0;
        }

        public void setDefault()
        {
            attackPower = baseAtk;
        }

        public void setLast()
        {
            attackPower = lastAtk;
            lastAtk = baseAtk;
        }

        public string getName()
        {
            return name;
        }

        public string getEffect()
        {
            return effect;
        }

        public int getHealth()
        {
            return health;
        }

        public void destroy()
        {
            ko = true;
        }

        public bool isKO()
        {
            return ko;
        }

    }
    /// <summary>
    /// This tutorial demonstrates how to use both the ALVAR and ARTag optical marker tracker 
    /// library with our Goblin XNA framework. Please read the README.pdf included with this 
    /// project before running this tutorial. If you're using ARTag, please remove calib.xml
    /// from the solution explorer so that you can successfully build the project. 
    /// </summary>
    public class Tutorial8 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;
        MarkerNode cylinderMarkerNode100,
            cylinderMarkerNode101, cylinderMarkerNode102, cylinderMarkerNode103, cylinderMarkerNode104,
            cylinderMarkerNode105, cylinderMarkerNode106, cylinderMarkerNode107, cylinderMarkerNode108,
            cylinderMarkerNode109, cylinderMarkerNode110, cylinderMarkerNode111, cylinderMarkerNode112,
            cylinderMarkerNode113, cylinderMarkerNode114;

        MarkerNode cylinderMarkerNode115, cylinderMarkerNode116, cylinderMarkerNode117,
            cylinderMarkerNode118, cylinderMarkerNode119, cylinderMarkerNode120, cylinderMarkerNode121,
            cylinderMarkerNode122, cylinderMarkerNode123, cylinderMarkerNode124, cylinderMarkerNode125,
            cylinderMarkerNode126, cylinderMarkerNode127, cylinderMarkerNode128, cylinderMarkerNode129;

        MarkerNode cylinderMarkerNode130, cylinderMarkerNode131;

        MarkerNode cylinderMarkerNode132, cylinderMarkerNode133, cylinderMarkerNode134, 
            cylinderMarkerNode135,cylinderMarkerNode136, cylinderMarkerNode137, cylinderMarkerNode138,
            cylinderMarkerNode139, cylinderMarkerNode140, cylinderMarkerNode141;

        bool useStaticImage = false;
        SpriteFont uiFont;
        Card p1Monster1;
        Card p1Monster2;
        Card p1Monster3;
        Card p2Monster1;
        Card p2Monster2;
        Card p2Monster3;
        Card p1Spell;
        Card p1Trap;
        Card p2Spell;
        Card p2Trap;
        bool p1Turn = true;
        bool p1NoTrap = false;
        bool p2NoTrap = false;
        bool p1NoMagic = false;
        bool p2NoMagic = false;
        bool p1NoAttack = false;
        bool p2NoAttack = false;
        int p1TrapEffectCnt = 0;
        int p2TrapEffectCnt = 0;
        int p1life = 50, p2life = 50;
        string text = "";
        bool p1Winner = false;
        public Tutorial8()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        Card[] cards = new Card[30];
        string p1TrapFlag = "none";
        string p2TrapFlag = "none";
        string p1SpellFlag = "none";
        string p2SpellFlag = "none";
        /*state machine creation: 
         *  * 0 = start splash page 
         *  * 1 = turn in progress (game on!)
         *  * 2 = end phase
         *  * 3 = cleanup phase
         *  * 4 = end game
         */
        int state = 1;
        G2DPanel p1Frame, p2Frame;
        G2DLabel p1LifeLab, p1m1LifeLab, p1m2LifeLab, p1m3LifeLab; 
        G2DLabel p2LifeLab, p2m1LifeLab, p2m2LifeLab, p2m3LifeLab;
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            scene = new Scene(this);

            // Use the newton physics engine to perform collision detection
            scene.PhysicsEngine = new NewtonPhysics();

            // Multi-thread the marker tracking process
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;

            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

            // Set up the lights used in the scene
            CreateLights();

            // Create 3D objects
            CreateObjects();

            // Create the ground that represents the physical ground marker array
            CreateGround();

            Create2DGUI();

            // Use per pixel lighting for better quality (If you using non NVidia graphics card,
            // setting this to true may reduce the performance significantly)
            scene.PreferPerPixelLighting = true;

            // Enable shadow mapping
            // NOTE: In order to use shadow mapping, you will need to add 'PostScreenShadowBlur.fx'
            // and 'ShadowMap.fx' shader files as well as 'ShadowDistanceFadeoutMap.dds' texture file
            // to your 'Content' directory
            scene.EnableShadowMapping = true;

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            scene.RootNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking()
        {
            IVideoCapture captureDevice = null;

            if (useStaticImage)
            {
                captureDevice = new NullCapture();
                captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._800x600,
                    ImageFormat.R8G8B8_24, false);
                ((NullCapture)captureDevice).StaticImageFile = "testImage800x600.jpg";
            }
            else
            {
                // Create our video capture device that uses DirectShow library. Note that 
                // the combinations of resolution and frame rate that are allowed depend on 
                // the particular video capture device. Thus, setting incorrect resolution 
                // and frame rate values may cause exceptions or simply be ignored, depending 
                // on the device driver.  The values set here will work for a Microsoft VX 6000, 
                // and many other webcams.
                captureDevice = new DirectShowCapture2();
                captureDevice.InitVideoCapture(0, FrameRate._60Hz, Resolution._640x480,
                    ImageFormat.R8G8B8_24, false);
            }

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            // Create an optical marker tracker that uses ALVAR library
            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "calib.xml", 9.0);

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            // Display the camera image in the background. Note that this parameter should
            // be set after adding at least one video capture device to the Scene class.
            scene.ShowCameraImage = true;
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");

            groundNode.Model = new Box(95, 59, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // CastShadows set to true
            groundNode.Model.ReceiveShadows = true;

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.Gray.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

        }

        private void Create2DGUI()
        {
            p1Frame = new G2DPanel();
            p1Frame.Bounds = new Rectangle(0, 0, 150, 80);
            p1Frame.Border = GoblinEnums.BorderFactory.LineBorder;
            p1Frame.Transparency = 1.0f;
            p1Frame.BackgroundColor = Color.CornflowerBlue;

            //Generic Label: P1
            G2DLabel p1Label = new G2DLabel("P1: "); 
            p1Label.Bounds = new Rectangle(10, 10, 10, 10);
            p1Label.TextFont = uiFont;
            
            //Life Point Label: P1
            p1LifeLab = new G2DLabel();
            p1LifeLab.Bounds = new Rectangle(25, 10, 20, 40);
            p1LifeLab.Text = p1life.ToString() + " LP";
            p1LifeLab.TextFont = uiFont;

            //Generic Label: P1M1
            G2DLabel p1m1StatLab = new G2DLabel("M1: ");
            p1m1StatLab.Bounds = new Rectangle(10, 25, 10, 10);
            p1m1StatLab.TextFont = uiFont;

            //Status Label: P1M1
            p1m1LifeLab = new G2DLabel();
            p1m1LifeLab.Bounds = new Rectangle(25, 25, 20, 40);
            p1m1LifeLab.TextFont = uiFont;

            //Generic Label: P1M2
            G2DLabel p1m2StatLab = new G2DLabel("M2: ");
            p1m2StatLab.Bounds = new Rectangle(10, 40, 10, 10);
            p1m2StatLab.TextFont = uiFont;

            //Status Label: P1M2
            p1m2LifeLab = new G2DLabel();
            p1m2LifeLab.Bounds = new Rectangle(25, 40, 20, 40);
            p1m2LifeLab.TextFont = uiFont;

            //Generic Label: P1M3
            G2DLabel p1m3StatLab = new G2DLabel("M3: ");
            p1m3StatLab.Bounds = new Rectangle(10, 55, 10, 10);
            p1m3StatLab.TextFont = uiFont;

            //Status Label: P1M3
            p1m3LifeLab = new G2DLabel();
            p1m3LifeLab.Bounds = new Rectangle(25, 55, 20, 40);
            p1m3LifeLab.TextFont = uiFont;

            
            p1Frame.AddChild(p1Label);
            p1Frame.AddChild(p1LifeLab);
            p1Frame.AddChild(p1m1StatLab);
            p1Frame.AddChild(p1m1LifeLab);
            p1Frame.AddChild(p1m2StatLab);
            p1Frame.AddChild(p1m2LifeLab);
            p1Frame.AddChild(p1m3StatLab);
            p1Frame.AddChild(p1m3LifeLab);
            scene.UIRenderer.Add2DComponent(p1Frame);
            
            
            p2Frame = new G2DPanel();
            p2Frame.Bounds = new Rectangle(650, 0, 150, 80);
            p2Frame.Border = GoblinEnums.BorderFactory.LineBorder;
            p2Frame.Transparency = 1.0f;
            p2Frame.BackgroundColor = Color.CornflowerBlue;

            //Generic Label: P2
            G2DLabel p2Label = new G2DLabel("P2: ");
            p2Label.Bounds = new Rectangle(15, 10, 10, 10);
            p2Label.TextFont = uiFont;

            //Life Point Label: P2
            p2LifeLab = new G2DLabel();
            p2LifeLab.Bounds = new Rectangle(35, 10, 40, 40);
            p2LifeLab.Text = p2life.ToString() + " LP";
            p2LifeLab.TextFont = uiFont;

            //Generic Label: P2M1
            G2DLabel p2m1StatLab = new G2DLabel("M1: ");
            p2m1StatLab.Bounds = new Rectangle(15, 25, 10, 10);
            p2m1StatLab.TextFont = uiFont;

            //Status Label: P2M1
            p2m1LifeLab = new G2DLabel();
            p2m1LifeLab.Bounds = new Rectangle(35, 25, 20, 40);
            p2m1LifeLab.TextFont = uiFont;

            //Generic Label: P2M2
            G2DLabel p2m2StatLab = new G2DLabel("M2: ");
            p2m2StatLab.Bounds = new Rectangle(15, 40, 10, 10);
            p2m2StatLab.TextFont = uiFont;

            //Status Label: P2M2
            p2m2LifeLab = new G2DLabel();
            p2m2LifeLab.Bounds = new Rectangle(35, 40, 20, 40);
            p2m2LifeLab.TextFont = uiFont;

            //Generic Label: P2M3
            G2DLabel p2m3StatLab = new G2DLabel("M3: ");
            p2m3StatLab.Bounds = new Rectangle(15, 55, 10, 10);
            p2m3StatLab.TextFont = uiFont;

            //Status Label: P2M3
            p2m3LifeLab = new G2DLabel();
            p2m3LifeLab.Bounds = new Rectangle(35, 55, 20, 40);
            p2m3LifeLab.TextFont = uiFont;

            
            p2Frame.AddChild(p2Label);
            p2Frame.AddChild(p2LifeLab);
            p2Frame.AddChild(p2m1StatLab);
            p2Frame.AddChild(p2m1LifeLab);
            p2Frame.AddChild(p2m2StatLab);
            p2Frame.AddChild(p2m2LifeLab);
            p2Frame.AddChild(p2m3StatLab);
            p2Frame.AddChild(p2m3LifeLab);
            scene.UIRenderer.Add2DComponent(p2Frame);

        }

        private void CreateObjects()
        {

            // Create a material to apply to the sphere model
            Material sphereMaterial = new Material();
            sphereMaterial.Diffuse = new Vector4(0, 0.5f, 0, 1);
            sphereMaterial.Specular = Color.Green.ToVector4();
            sphereMaterial.SpecularPower = 10;

            Material sphereMaterial2 = new Material();
            sphereMaterial2.Diffuse = new Vector4(0, 0, 0.5f, 1);
            sphereMaterial2.Specular = Color.Blue.ToVector4();
            sphereMaterial2.SpecularPower = 10;

            Material sphereMaterial3 = new Material();
            sphereMaterial3.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial3.Specular = Color.Red.ToVector4();
            sphereMaterial3.SpecularPower = 10;

            Material sphereMaterial4 = new Material();
            sphereMaterial4.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial4.Specular = Color.Cyan.ToVector4();
            sphereMaterial4.SpecularPower = 10;

            Material sphereMaterial5 = new Material();
            sphereMaterial5.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial5.Specular = Color.Magenta.ToVector4();
            sphereMaterial5.SpecularPower = 10;

            Material sphereMaterial6 = new Material();
            sphereMaterial6.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial6.Specular = Color.Yellow.ToVector4();
            sphereMaterial6.SpecularPower = 10;

            Material sphereMaterial7 = new Material();
            sphereMaterial7.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial7.Specular = Color.DarkGray.ToVector4();
            sphereMaterial7.SpecularPower = 10;

            Material sphereMaterial8 = new Material();
            sphereMaterial8.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial8.Specular = Color.Orange.ToVector4();
            sphereMaterial8.SpecularPower = 10;

            Material sphereMaterial9 = new Material();
            sphereMaterial9.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial9.Specular = Color.Purple.ToVector4();
            sphereMaterial9.SpecularPower = 10;

            Material sphereMaterial10 = new Material();
            sphereMaterial10.Diffuse = new Vector4(0.5f, 0, 0, 1);
            sphereMaterial10.Specular = Color.Pink.ToVector4();
            sphereMaterial10.SpecularPower = 10;

            int[] zero = new int[1];
            int[] first = new int[1];
            int[] two = new int[1];
            int[] three = new int[1];
            int[] four = new int[1];
            int[] five = new int[1];
            int[] six = new int[1];
            int[] seven = new int[1];
            int[] eight = new int[1];
            int[] nine = new int[1];
            int[] ten = new int[1];
            int[] eleven = new int[1];
            int[] twelve = new int[1];
            int[] thirteen = new int[1];
            int[] fourteen = new int[1];
            int[] fifteen = new int[1];
            int[] sixteen = new int[1];
            int[] seventeen = new int[1];
            int[] eighteen = new int[1];
            int[] nineteen = new int[1];
            int[] twenty = new int[1];
            int[] twentyone = new int[1];
            int[] twentytwo = new int[1];
            int[] twentythree = new int[1];
            int[] twentyfour = new int[1];
            int[] twentyfive = new int[1];
            int[] twentysix = new int[1];
            int[] twentyseven = new int[1];
            int[] twentyeight = new int[1];
            int[] twentynine = new int[1];
            int[] thirty = new int[1];
            int[] thirtyone = new int[1];
            int[] thirtytwo = new int[1];
            int[] thirtythree = new int[1]{133};
            int[] thirtyfour = new int[1]{134};
            int[] thirtyfive = new int[1]{135};
            int[] thirtysix = new int[1]{136};
            int[] thirtyseven = new int[1]{137};
            int[] thirtyeight = new int[1] { 138 };
            int[] thirtynine = new int[1] { 139 };
            int[] forty = new int[1] { 140 };
            int[] fortyone = new int[1] { 141 }; 


            zero[0] = 100;
            first[0] = 101;
            two[0] = 102;
            three[0] = 103;
            four[0] = 104;
            five[0] = 105;
            six[0] = 106;
            seven[0] = 107;
            eight[0] = 108;
            nine[0] = 100;
            ten[0] = 110;
            eleven[0] = 111;
            twelve[0] = 112;
            thirteen[0] = 113;
            fourteen[0] = 114;
            fifteen[0] = 115;
            sixteen[0] = 116;
            seventeen[0] = 117;
            eighteen[0] = 118;
            nineteen[0] = 119;
            twenty[0] = 120;
            twentyone[0] = 121;
            twentytwo[0] = 122;
            twentythree[0] = 123;
            twentyfour[0] = 124;
            twentyfive[0] = 125;
            twentysix[0] = 126;
            twentyseven[0] = 127;
            twentyeight[0] = 128;
            twentynine[0] = 129;
            thirty[0] = 130;
            thirtyone[0] = 131;
            thirtytwo[0] = 132;

            ModelLoader loader = new ModelLoader();
            Model trapModel0 = (Model)loader.Load("", "Boxwhite");
            Model trapModel1 = (Model)loader.Load("", "Boxwhite");
            Model trapModel2 = (Model)loader.Load("", "Boxwhite");
            Model trapModel3 = (Model)loader.Load("", "Boxwhite");
            Model trapModel4 = (Model)loader.Load("", "Boxwhite");
            Model trapModel5 = (Model)loader.Load("", "Boxwhite");
            Model trapModel6 = (Model)loader.Load("", "Boxwhite");
            Model trapModel7 = (Model)loader.Load("", "Boxwhite");
            Model trapModel8 = (Model)loader.Load("", "Boxwhite");
            Model trapModel9 = (Model)loader.Load("", "Boxwhite");

            Model spellModel0 = (Model)loader.Load("", "MagicHat");
            Model spellModel1 = (Model)loader.Load("", "MagicHat");
            Model spellModel2 = (Model)loader.Load("", "MagicHat");
            Model spellModel3 = (Model)loader.Load("", "MagicHat");
            Model spellModel4 = (Model)loader.Load("", "MagicHat");
            Model spellModel5 = (Model)loader.Load("", "MagicHat");
            Model spellModel6 = (Model)loader.Load("", "MagicHat");
            Model spellModel7 = (Model)loader.Load("", "MagicHat");
            Model spellModel8 = (Model)loader.Load("", "MagicHat");
            Model spellModel9 = (Model)loader.Load("", "MagicHat");

            Model monsterModel0 = (Model)loader.Load("", "Slime");
            Model monsterModel1 = (Model)loader.Load("", "Slime");
            Model monsterModel2 = (Model)loader.Load("", "Slime");
            Model monsterModel3 = (Model)loader.Load("", "Slime");
            Model monsterModel4 = (Model)loader.Load("", "Slime");
            Model monsterModel5 = (Model)loader.Load("", "Slime");
            Model monsterModel6 = (Model)loader.Load("", "Slime");
            Model monsterModel7 = (Model)loader.Load("", "Slime");
            Model monsterModel8 = (Model)loader.Load("", "Slime");
            Model monsterModel9 = (Model)loader.Load("", "Slime");

            //Marker 100
            cylinderMarkerNode100 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML100.xml", zero);
            GeometryNode cylinderNode0 = new GeometryNode("Cylinder");

            cylinderNode0.Model = monsterModel0;
            ((Model)cylinderNode0.Model).UseInternalMaterials = false;
            

            cylinderNode0.Material = sphereMaterial;
            
            TransformNode cylinderTransNode0 = new TransformNode();
            cylinderTransNode0.Scale = new Vector3(5, 5, 5);
            cylinderTransNode0.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);

            cylinderMarkerNode100.AddChild(cylinderTransNode0);

            cylinderTransNode0.AddChild(cylinderNode0);

            //add to Card array here: generic monster card here
            cards[0] = new Card('M', cylinderMarkerNode100, 90, 125, "Tethys, Goddess of Light", "Attack");

            //Marker 101
            cylinderMarkerNode101 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML101.xml", first);
            GeometryNode cylinderNode1 = new GeometryNode("Cylinder");

            cylinderNode1.Model = monsterModel1;
            ((Model)cylinderNode1.Model).UseInternalMaterials = false;

            cylinderNode1.Material = sphereMaterial2;

            TransformNode cylinderTransNode1 = new TransformNode();

            cylinderTransNode1.Scale = new Vector3(5, 5, 5);
            cylinderTransNode1.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode101.AddChild(cylinderTransNode1);

            cylinderTransNode1.AddChild(cylinderNode1);

            //add to Card array here: generic monster card here
            cards[1] = new Card('M', cylinderMarkerNode101, 120, 150, "Athena", "Attack");

            //Marker 102
            cylinderMarkerNode102 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML102.xml", two);
            GeometryNode cylinderNode2 = new GeometryNode("Cylinder");

            cylinderNode2.Model = monsterModel2;
            ((Model)cylinderNode2.Model).UseInternalMaterials = false;

            cylinderNode2.Material = sphereMaterial3;

            TransformNode cylinderTransNode2 = new TransformNode();

            cylinderTransNode2.Scale = new Vector3(5, 5, 5);
            cylinderTransNode2.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode102.AddChild(cylinderTransNode2);

            cylinderTransNode2.AddChild(cylinderNode2);

            //add to Card array here: generic monster card here
            cards[2] = new Card('M', cylinderMarkerNode102, 105, 120, "Victoria", "Attack");

            //Marker 103
            cylinderMarkerNode103 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML103.xml", three);
            GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

            cylinderNode3.Model = monsterModel3;
            ((Model)cylinderNode3.Model).UseInternalMaterials = false;
            cylinderNode3.Material = sphereMaterial4;

            TransformNode cylinderTransNode3 = new TransformNode();

            cylinderTransNode3.Scale = new Vector3(5, 5, 5);
            cylinderTransNode3.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode103.AddChild(cylinderTransNode3);

            cylinderTransNode3.AddChild(cylinderNode3);

            //add to Card array here: generic monster card here
            cards[3] = new Card('M', cylinderMarkerNode103, 130, 160, "The Agent of Force - Mars", "Attack");

            //Marker 104
            cylinderMarkerNode104 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML104.xml", four);
            GeometryNode cylinderNode4 = new GeometryNode("Cylinder");

            cylinderNode4.Model = monsterModel4;
            ((Model)cylinderNode4.Model).UseInternalMaterials = false;

            cylinderNode4.Material = sphereMaterial5;

            TransformNode cylinderTransNode4 = new TransformNode();

            cylinderTransNode4.Scale = new Vector3(5, 5, 5);
            cylinderTransNode4.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode104.AddChild(cylinderTransNode4);

            cylinderTransNode4.AddChild(cylinderNode4);

            //add to Card array here: generic monster card here
            cards[4] = new Card('M', cylinderMarkerNode104, 115, 155, "The Agent of Wisdom - Mercury", "Attack");

            //Marker 105
            cylinderMarkerNode105 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML105.xml", five);
            GeometryNode cylinderNode5 = new GeometryNode("Cylinder");

            cylinderNode5.Model = monsterModel5;
            ((Model)cylinderNode5.Model).UseInternalMaterials = false;

            cylinderNode5.Material = sphereMaterial6;

            TransformNode cylinderTransNode5 = new TransformNode();

            cylinderTransNode5.Scale = new Vector3(5, 5, 5);
            cylinderTransNode5.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode105.AddChild(cylinderTransNode5);

            cylinderTransNode5.AddChild(cylinderNode5);

            //add to Card array here: generic monster card here
            cards[5] = new Card('M', cylinderMarkerNode105, 95, 145, "The Agent of Mystery - Earth", "Attack");

            //Marker 106
            cylinderMarkerNode106 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML106.xml", six);
            GeometryNode cylinderNode6 = new GeometryNode("Cylinder");

            cylinderNode6.Model = monsterModel6;
            ((Model)cylinderNode6.Model).UseInternalMaterials = false;

            cylinderNode6.Material = sphereMaterial7;

            TransformNode cylinderTransNode6 = new TransformNode();

            cylinderTransNode6.Scale = new Vector3(5, 5, 5);
            cylinderTransNode6.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode106.AddChild(cylinderTransNode6);

            cylinderTransNode6.AddChild(cylinderNode6);

            //add to Card array here: generic monster card here
            cards[6] = new Card('M', cylinderMarkerNode106, 130, 175, "The Agent of Miracles - Jupiter", "Attack");

            //Marker 107
            cylinderMarkerNode107 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML107.xml", seven);
            GeometryNode cylinderNode7 = new GeometryNode("Cylinder");

            cylinderNode7.Model = monsterModel7;
            ((Model)cylinderNode7.Model).UseInternalMaterials = false;

            cylinderNode7.Material = sphereMaterial8;

            TransformNode cylinderTransNode7 = new TransformNode();

            cylinderTransNode7.Scale = new Vector3(5, 5, 5);
            cylinderTransNode7.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode107.AddChild(cylinderTransNode7);

            cylinderTransNode7.AddChild(cylinderNode7);

            //add to Card array here: generic monster card here
            cards[7] = new Card('M', cylinderMarkerNode107, 80, 120, "The Agent of Judgment - Saturn", "Attack");

            //Marker 108
            cylinderMarkerNode108 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML108.xml", eight);
            GeometryNode cylinderNode8 = new GeometryNode("Cylinder");

            cylinderNode8.Model = monsterModel8;
            ((Model)cylinderNode8.Model).UseInternalMaterials = false;

            cylinderNode8.Material = sphereMaterial9;

            TransformNode cylinderTransNode8 = new TransformNode();

            cylinderTransNode8.Scale = new Vector3(5, 5, 5);
            cylinderTransNode8.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode108.AddChild(cylinderTransNode8);

            cylinderTransNode8.AddChild(cylinderNode8);

            //add to Card array here: generic monster card here
            cards[8] = new Card('M', cylinderMarkerNode108, 75, 140, "The Agent of Creation - Venus", "Attack");

            //Marker 109
            cylinderMarkerNode109 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML109.xml", nine);
            GeometryNode cylinderNode9 = new GeometryNode("Cylinder");

            cylinderNode9.Model = monsterModel9;
            ((Model)cylinderNode9.Model).UseInternalMaterials = false;

            cylinderNode9.Material = sphereMaterial10;

            TransformNode cylinderTransNode9 = new TransformNode();

            cylinderTransNode9.Scale = new Vector3(5, 5, 5);
            cylinderTransNode9.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode109.AddChild(cylinderTransNode9);

            cylinderTransNode9.AddChild(cylinderNode9);

            //add to Card array here: generic monster card here
            cards[9] = new Card('M', cylinderMarkerNode109, 150, 200, "Master Hyperion", "Attack");

            //Marker 110
            cylinderMarkerNode110 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML110.xml", ten);
            GeometryNode cylinderNode10 = new GeometryNode("Cylinder");

            cylinderNode10.Model = spellModel0;
            ((Model)cylinderNode10.Model).UseInternalMaterials = false;

            cylinderNode10.Material = sphereMaterial;

            TransformNode cylinderTransNode10 = new TransformNode();

            cylinderTransNode10.Scale = new Vector3(5, 5, 5);
            cylinderTransNode10.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode110.AddChild(cylinderTransNode10);

            cylinderTransNode10.AddChild(cylinderNode10);

            //add to Card array here: generic spell card here
            cards[10] = new Card('S', cylinderMarkerNode110, 100, 100, "Cards from the Sky", "All of your monsters are healed for 100 hp.");

            //Marker 111
            cylinderMarkerNode111 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML111.xml", eleven);
            GeometryNode cylinderNode11 = new GeometryNode("Cylinder");

            cylinderNode11.Model = spellModel1;
            ((Model)cylinderNode11.Model).UseInternalMaterials = false;

            cylinderNode11.Material = sphereMaterial2;

            TransformNode cylinderTransNode11 = new TransformNode();

            cylinderTransNode11.Scale = new Vector3(5, 5, 5);
            cylinderTransNode11.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode111.AddChild(cylinderTransNode11);

            cylinderTransNode11.AddChild(cylinderNode11);

            //add to Card array here: generic spell card here
            cards[11] = new Card('S', cylinderMarkerNode111, 100, 100, "Valhalla, Hall of the Fallen", "All of your monsters are completly healed.");

            //Marker 112
            cylinderMarkerNode112 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML112.xml", twelve);
            GeometryNode cylinderNode12 = new GeometryNode("Cylinder");

            cylinderNode12.Model = spellModel2;
            ((Model)cylinderNode12.Model).UseInternalMaterials = false;

            cylinderNode12.Material = sphereMaterial3;

            TransformNode cylinderTransNode12 = new TransformNode();

            cylinderTransNode12.Scale = new Vector3(5, 5, 5);
            cylinderTransNode12.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode112.AddChild(cylinderTransNode12);

            cylinderTransNode12.AddChild(cylinderNode12);

            //add to Card array here: generic spell card here
            cards[12] = new Card('S', cylinderMarkerNode112, 100, 100, "Terraforming", "All of your monsters are healed for 1 hp.");

            //Marker 113
            cylinderMarkerNode113 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML113.xml", thirteen);
            GeometryNode cylinderNode13 = new GeometryNode("Cylinder");

            cylinderNode13.Model = spellModel3;
            ((Model)cylinderNode13.Model).UseInternalMaterials = false;

            cylinderNode13.Material = sphereMaterial4;

            TransformNode cylinderTransNode13 = new TransformNode();

            cylinderTransNode13.Scale = new Vector3(5, 5, 5);
            cylinderTransNode13.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode113.AddChild(cylinderTransNode13);

            cylinderTransNode13.AddChild(cylinderNode13);

            //add to Card array here: generic spell card here
            cards[13] = new Card('S', cylinderMarkerNode113, 100, 100, "Smashing Ground", "All of your monsters are healed for 20 hp.");

            //Marker 114
            cylinderMarkerNode114 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML114.xml", fourteen);
            GeometryNode cylinderNode14 = new GeometryNode("Cylinder");

            cylinderNode14.Model = spellModel4;
            ((Model)cylinderNode14.Model).UseInternalMaterials = false;

            cylinderNode14.Material = sphereMaterial5;

            TransformNode cylinderTransNode14 = new TransformNode();

            cylinderTransNode14.Scale = new Vector3(5, 5, 5);
            cylinderTransNode14.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode114.AddChild(cylinderTransNode14);

            cylinderTransNode14.AddChild(cylinderNode14);

            //add to Card array here: generic spell card here
            cards[14] = new Card('S', cylinderMarkerNode114, 100, 100, "The Sanctuary in the Sky", "All of your monsters are healed for 75 hp.");

            //Marker 115
            cylinderMarkerNode115 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML115.xml", fifteen);
            GeometryNode cylinderNode15 = new GeometryNode("Cylinder");

            cylinderNode15.Model = spellModel5;
            ((Model)cylinderNode15.Model).UseInternalMaterials = false;

            cylinderNode15.Material = sphereMaterial6;

            TransformNode cylinderTransNode15 = new TransformNode();

            cylinderTransNode15.Scale = new Vector3(5, 5, 5);
            cylinderTransNode15.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderMarkerNode115.AddChild(cylinderTransNode15);

            cylinderTransNode15.AddChild(cylinderNode15);

            //add to Card array here: generic spell card here
            cards[15] = new Card('S', cylinderMarkerNode115, 100, 100, "The Sanctuary in the Sky", "All of your monsters are healed for 75 hp.");

            //Marker 116
            cylinderMarkerNode116 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML116.xml", sixteen);
            GeometryNode cylinderNode16 = new GeometryNode("Cylinder");

            cylinderNode16.Model = spellModel6;
            ((Model)cylinderNode16.Model).UseInternalMaterials = false;

            cylinderNode16.Material = sphereMaterial7;

            TransformNode cylinderTransNode16 = new TransformNode();

            cylinderTransNode16.Scale = new Vector3(5, 5, 5);
            cylinderTransNode16.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode16.AddChild(cylinderNode16);

            cylinderMarkerNode116.AddChild(cylinderTransNode16);

            //add to Card array here: generic spell card here
            cards[16] = new Card('S', cylinderMarkerNode116, 100, 100, "Celestial Transformation", "All of your monsters are healed for half of their hp.");

            //Marker 117
            cylinderMarkerNode117 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML117.xml", seventeen);
            GeometryNode cylinderNode17 = new GeometryNode("Cylinder");

            cylinderNode17.Model = spellModel7;
            ((Model)cylinderNode17.Model).UseInternalMaterials = false;

            cylinderNode17.Material = sphereMaterial8;

            TransformNode cylinderTransNode17 = new TransformNode();

            cylinderTransNode17.Scale = new Vector3(5, 5, 5);
            cylinderTransNode17.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode17.AddChild(cylinderNode17);

            cylinderMarkerNode117.AddChild(cylinderTransNode17);

            //add to Card array here: generic spell card here
            cards[17] = new Card('S', cylinderMarkerNode117, 100, 100, "Burial from a Different Dimension", "You're protected from 1 trap.");

            //Marker 118
            cylinderMarkerNode118 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML118.xml", eighteen);
            GeometryNode cylinderNode18 = new GeometryNode("Cylinder");

            cylinderNode18.Model = spellModel8;
            ((Model)cylinderNode18.Model).UseInternalMaterials = false;

            cylinderNode18.Material = sphereMaterial9;

            TransformNode cylinderTransNode18 = new TransformNode();

            cylinderTransNode18.Scale = new Vector3(5, 5, 5);
            cylinderTransNode18.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode18.AddChild(cylinderNode18);

            cylinderMarkerNode118.AddChild(cylinderTransNode18);

            //add to Card array here: generic spell card here
            cards[18] = new Card('S', cylinderMarkerNode118, 100, 100, "Mausoleum of the Emperor", "All of your monsters are healed for 75% of their HP.");

            //Marker 119
            cylinderMarkerNode119 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML119.xml", nineteen);
            GeometryNode cylinderNode19 = new GeometryNode("Cylinder");

            cylinderNode19.Model = spellModel9;
            ((Model)cylinderNode19.Model).UseInternalMaterials = false;

            cylinderNode19.Material = sphereMaterial10;

            TransformNode cylinderTransNode19 = new TransformNode();

            cylinderTransNode19.Scale = new Vector3(5, 5, 5);
            cylinderTransNode19.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode19.AddChild(cylinderNode19);

            cylinderMarkerNode119.AddChild(cylinderTransNode19);

            //add to Card array here: generic spell card here
            cards[19] = new Card('S', cylinderMarkerNode119, 100, 100, "The Fountain in the Sky ", "All of your monsters are healed for 25% of their HP.");

            //Marker 120
            cylinderMarkerNode120 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML120.xml", twenty);
            GeometryNode cylinderNode20 = new GeometryNode("Cylinder");

            cylinderNode20.Model = trapModel0;
            ((Model)cylinderNode20.Model).UseInternalMaterials = false;

            cylinderNode20.Material = sphereMaterial;

            TransformNode cylinderTransNode20 = new TransformNode();
            cylinderTransNode20.Scale = new Vector3(5, 5, 5);
            cylinderTransNode20.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode20.AddChild(cylinderNode20);

            cylinderMarkerNode120.AddChild(cylinderTransNode20);

            //add to Card array here: generic trap card here
            cards[20] = new Card('T', cylinderMarkerNode120, 0, 100, "Divine Punishment", 
                "All of your opponent's monsters take damage equal to half of their current health.");

            //Marker 121
            cylinderMarkerNode121 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML121.xml", twentyone);
            GeometryNode cylinderNode21 = new GeometryNode("Cylinder");

            cylinderNode21.Model = trapModel1;
            ((Model)cylinderNode21.Model).UseInternalMaterials = false;
            cylinderNode21.Material = sphereMaterial2;


            TransformNode cylinderTransNode21 = new TransformNode();
            cylinderTransNode21.Scale = new Vector3(5, 5, 5);
            cylinderTransNode21.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode21.AddChild(cylinderNode21);

            cylinderMarkerNode121.AddChild(cylinderTransNode21);

            //add to Card array here: generic trap card here
            cards[21] = new Card('T', cylinderMarkerNode121, 0, 100, "Return from the Different Dimension",
                "Your opponent may not attack for the remainder of their turn.");

            //Marker 122
            cylinderMarkerNode122 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML122.xml", twentytwo);
            GeometryNode cylinderNode22 = new GeometryNode("Cylinder");

            cylinderNode22.Model = trapModel2;
            ((Model)cylinderNode22.Model).UseInternalMaterials = false;
            cylinderNode22.Material = sphereMaterial3;

            TransformNode cylinderTransNode22 = new TransformNode();
            cylinderTransNode22.Scale = new Vector3(5, 5, 5);
            cylinderTransNode22.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode22.AddChild(cylinderNode22);

            cylinderMarkerNode122.AddChild(cylinderTransNode22);

            //add to Card array here: generic trap card here
            cards[22] = new Card('T', cylinderMarkerNode122, 0, 100, "Torrential Tribute", "Destroy a spell card.");

            //Marker 123
            cylinderMarkerNode123 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML123.xml", twentythree);
            GeometryNode cylinderNode23 = new GeometryNode("Cylinder");

            cylinderNode23.Model = trapModel3;
            ((Model)cylinderNode23.Model).UseInternalMaterials = false;
            cylinderNode23.Material = sphereMaterial4;

            TransformNode cylinderTransNode23 = new TransformNode();

            cylinderTransNode23.Scale = new Vector3(5, 5, 5);
            cylinderTransNode23.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode23.AddChild(cylinderNode23);

            cylinderMarkerNode123.AddChild(cylinderTransNode23);

            //add to Card array here: generic trap card here
            cards[23] = new Card('T', cylinderMarkerNode123, 0, 100, "Beckoning Light",
                "Your opponent may not activate a trap during your next turn.");

            //Marker 124
            cylinderMarkerNode124 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML124.xml", twentyfour);
            GeometryNode cylinderNode24 = new GeometryNode("Cylinder");

            cylinderNode24.Model = trapModel4;
            ((Model)cylinderNode24.Model).UseInternalMaterials = false;
            cylinderNode24.Material = sphereMaterial5;

            TransformNode cylinderTransNode24 = new TransformNode();

            cylinderTransNode24.Scale = new Vector3(5, 5, 5);
            cylinderTransNode24.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode24.AddChild(cylinderNode24);

            cylinderMarkerNode124.AddChild(cylinderTransNode24);

            //add to Card array here: generic trap card here
            cards[24] = new Card('T', cylinderMarkerNode124, 0, 100, "Miraculous Descent",
                "Reduce the damage taken to your life points to 0 for the remainder of your opponent's turn.");

            //Marker 125
            cylinderMarkerNode125 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML125.xml", twentyfive);
            GeometryNode cylinderNode25 = new GeometryNode("Cylinder");

            cylinderNode25.Model = trapModel5;
            ((Model)cylinderNode25.Model).UseInternalMaterials = false;

            cylinderNode25.Material = sphereMaterial6;

            TransformNode cylinderTransNode25 = new TransformNode();

            cylinderTransNode25.Scale = new Vector3(5, 5, 5);
            cylinderTransNode25.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode25.AddChild(cylinderNode25);

            cylinderMarkerNode125.AddChild(cylinderTransNode25);

            //add to Card array here: generic trap card here
            cards[25] = new Card('T', cylinderMarkerNode125, 0, 100, "Miraculous Descent",
                "Reduce the damage taken to your life points to 0 for the remainder of your opponent's turn.");

            //Marker 126
            cylinderMarkerNode126 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML126.xml", twentysix);
            GeometryNode cylinderNode26 = new GeometryNode("Cylinder");

            cylinderNode26.Model = trapModel6;
            ((Model)cylinderNode26.Model).UseInternalMaterials = false;

            cylinderNode26.Material = sphereMaterial7;

            TransformNode cylinderTransNode26 = new TransformNode();

            cylinderTransNode26.Scale = new Vector3(5, 5, 5);
            cylinderTransNode26.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode26.AddChild(cylinderNode26);

            cylinderMarkerNode126.AddChild(cylinderTransNode26);

            //add to Card array here: generic trap card here
            cards[26] = new Card('T', cylinderMarkerNode126, 0, 100, "Solemn Judgment",
                "Your opponent may not activate spells until the end of their turn.");

            //Marker 127
            cylinderMarkerNode127 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML127.xml", twentyseven);
            GeometryNode cylinderNode27 = new GeometryNode("Cylinder");

            cylinderNode27.Model = trapModel7;
            ((Model)cylinderNode27.Model).UseInternalMaterials = false;
            cylinderNode27.Material = sphereMaterial8;

            TransformNode cylinderTransNode27 = new TransformNode();

            cylinderTransNode27.Scale = new Vector3(5, 5, 5);
            cylinderTransNode27.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode27.AddChild(cylinderNode27);

            cylinderMarkerNode127.AddChild(cylinderTransNode27);


            //add to Card array here: generic trap card here
            cards[27] = new Card('T', cylinderMarkerNode127, 0, 100, "Power Break",
                "Reduce a monster's attack by 50 for the remainder of your opponent's turn.");

            //Marker 128
            cylinderMarkerNode128 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML128.xml", twentyeight);
            GeometryNode cylinderNode28 = new GeometryNode("Cylinder");

            cylinderNode28.Model = trapModel8;
            ((Model)cylinderNode28.Model).UseInternalMaterials = false;

            cylinderNode28.Material = sphereMaterial9;

            TransformNode cylinderTransNode28 = new TransformNode();

            
            cylinderTransNode28.Scale = new Vector3(5, 5, 5);
            cylinderTransNode28.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode28.AddChild(cylinderNode28);

            cylinderMarkerNode128.AddChild(cylinderTransNode28);

            //add to Card array here: generic trap card here
            cards[28] = new Card('T', cylinderMarkerNode128, 0, 100, "Reinforcements",
                "Increase a target monster's attack by 40 for the remainder of your opponent's turn.");

            //Marker 129
            cylinderMarkerNode129 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML129.xml", twentynine);
            GeometryNode cylinderNode29 = new GeometryNode("Cylinder");

            cylinderNode29.Model = trapModel9;
            ((Model)cylinderNode29.Model).UseInternalMaterials = false;
            cylinderNode29.Material = sphereMaterial10;

            TransformNode cylinderTransNode29 = new TransformNode();

            
            cylinderTransNode29.Scale = new Vector3(5, 5, 5);
            cylinderTransNode29.Rotation = Quaternion.CreateFromYawPitchRoll(0, 1.5f, 0);
            cylinderTransNode29.AddChild(cylinderNode29);

            cylinderMarkerNode129.AddChild(cylinderTransNode29);

            //add to Card array here: generic trap card here
            cards[29] = new Card('T', cylinderMarkerNode129, 0, 100, "Earthshaker",
                "For the remainder of your opponent's turn, reduce the attack of a monster to 0.");

            //Marker 30 = Player 1 Marker
            cylinderMarkerNode130 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML130.xml", thirty);

            GeometryNode cylinderNode30 = new GeometryNode("Sphere");

            cylinderNode30.Model = new Sphere(5, 20, 20);

            cylinderNode30.Material = sphereMaterial3;

            TransformNode cylinderTransNode30 = new TransformNode();

            cylinderTransNode30.Translation = new Vector3(0, 0, 3);

            cylinderTransNode30.AddChild(cylinderNode30);

            cylinderMarkerNode130.AddChild(cylinderTransNode30);

            //Marker 31 = Player 2 Marker
            cylinderMarkerNode131 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML131.xml", thirtyone);

            GeometryNode cylinderNode31 = new GeometryNode("Sphere");

            cylinderNode31.Model = new Sphere(5, 20, 20);

            cylinderNode31.Material = sphereMaterial;

            TransformNode cylinderTransNode31 = new TransformNode();

            cylinderTransNode31.Translation = new Vector3(0, 0, 3);

            cylinderTransNode31.AddChild(cylinderNode31);

            cylinderMarkerNode131.AddChild(cylinderTransNode31);

            //Marker 32 = P1 Monster Position Marker 1
            cylinderMarkerNode132 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML132.xml", thirtytwo);

            //Marker 33 = P1 Monster Position Marker 2
            cylinderMarkerNode133 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML133.xml", thirtythree);

            //Marker 34 = P1 Monster Position Marker 3
            cylinderMarkerNode134 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML134.xml", thirtyfour);

            //Marker 35 = P2 Monster Position Marker 1
            cylinderMarkerNode135 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML135.xml", thirtyfive);

            //Marker 36 = P2 Monster Position Marker 2
            cylinderMarkerNode136 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML136.xml", thirtysix);

            //Marker 37 = P2 Monster Position Marker 3
            cylinderMarkerNode137 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML137.xml", thirtyseven);

            //Marker 38 = P1 Spell Position Marker
            cylinderMarkerNode138 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML138.xml", thirtyeight);

            //Marker 39 = P2 Spell Position Marker
            cylinderMarkerNode139 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML139.xml", thirtynine);

            //Marker 40 = P1 Trap Position Marker
            cylinderMarkerNode140 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML140.xml", forty);

            //Marker 41 = P2 Trap Position Marker 
            cylinderMarkerNode141 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML141.xml", fortyone);




            scene.RootNode.AddChild(cylinderMarkerNode100);
            scene.RootNode.AddChild(cylinderMarkerNode101);
            scene.RootNode.AddChild(cylinderMarkerNode102);
            scene.RootNode.AddChild(cylinderMarkerNode103);
            scene.RootNode.AddChild(cylinderMarkerNode104);
            scene.RootNode.AddChild(cylinderMarkerNode105);
            scene.RootNode.AddChild(cylinderMarkerNode106);
            scene.RootNode.AddChild(cylinderMarkerNode107);
            scene.RootNode.AddChild(cylinderMarkerNode108);
            scene.RootNode.AddChild(cylinderMarkerNode109);
            scene.RootNode.AddChild(cylinderMarkerNode110);
            scene.RootNode.AddChild(cylinderMarkerNode111);
            scene.RootNode.AddChild(cylinderMarkerNode112);
            scene.RootNode.AddChild(cylinderMarkerNode113);
            scene.RootNode.AddChild(cylinderMarkerNode114);

            scene.RootNode.AddChild(cylinderMarkerNode115);
            scene.RootNode.AddChild(cylinderMarkerNode116);
            scene.RootNode.AddChild(cylinderMarkerNode117);
            scene.RootNode.AddChild(cylinderMarkerNode118);
            scene.RootNode.AddChild(cylinderMarkerNode119);
            scene.RootNode.AddChild(cylinderMarkerNode120);
            scene.RootNode.AddChild(cylinderMarkerNode121);
            scene.RootNode.AddChild(cylinderMarkerNode122);
            scene.RootNode.AddChild(cylinderMarkerNode123);
            scene.RootNode.AddChild(cylinderMarkerNode124);
            scene.RootNode.AddChild(cylinderMarkerNode125);
            scene.RootNode.AddChild(cylinderMarkerNode126);
            scene.RootNode.AddChild(cylinderMarkerNode127);
            scene.RootNode.AddChild(cylinderMarkerNode128);
            scene.RootNode.AddChild(cylinderMarkerNode129);

            scene.RootNode.AddChild(cylinderMarkerNode130);
            scene.RootNode.AddChild(cylinderMarkerNode131);
            scene.RootNode.AddChild(cylinderMarkerNode132);
            scene.RootNode.AddChild(cylinderMarkerNode133);
            scene.RootNode.AddChild(cylinderMarkerNode134);
            scene.RootNode.AddChild(cylinderMarkerNode135);
            scene.RootNode.AddChild(cylinderMarkerNode136);
            scene.RootNode.AddChild(cylinderMarkerNode137);
            scene.RootNode.AddChild(cylinderMarkerNode138);
            scene.RootNode.AddChild(cylinderMarkerNode139);
            scene.RootNode.AddChild(cylinderMarkerNode140);
            scene.RootNode.AddChild(cylinderMarkerNode141);

        }

        private void processTrap(Card trap)
        {
            //Traps can only be engaged during the opponent's turn
            //i.e. P1 can only engage traps when P2's turn. 

            if (trap == null)
                return; 
            if (!p1Turn) //P2 turn
            {
                if (p1NoTrap) //check if P2 has NoTrap effect active on P1
                {
                    if(p2SpellFlag=="BDD")
                    {
                        p2SpellFlag = "none";
                        p1NoTrap = false;
                        p2Spell.destroy();
                    }
                    return;
                }
                else if(p1TrapFlag!="none")
                    return;
            }
            else //P1 turn
            {
                if (p2NoTrap)
                {
                    if(p1SpellFlag=="BDD")
                    {
                        p1SpellFlag = "none";
                        p2NoTrap = false;
                        p1Spell.destroy();
                    }
                    return;
                }
                else if(p2TrapFlag!="none")
                    return;
            }
            string name = trap.getName();
                if (name == "Divine Punishment")
                {
                    if(p1Turn)
                    {
                        if(p1Monster1!=null)
                            p1Monster1.debuff((int)Math.Ceiling(p1Monster1.getHealth()/2.0));
                        if (p1Monster2 != null)
                            p1Monster2.debuff((int)Math.Ceiling(p1Monster2.getHealth()/2.0));
                        if (p1Monster3 != null)
                            p1Monster3.debuff((int)Math.Ceiling(p1Monster3.getHealth()/2.0));
                        p2TrapFlag = "DP";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.debuff((int)Math.Ceiling(p2Monster1.getHealth()/2.0));
                        if (p2Monster2 != null)
                            p2Monster2.debuff((int)Math.Ceiling(p2Monster2.getHealth()/2.0));
                        if (p2Monster3 != null)
                            p2Monster3.debuff((int)Math.Ceiling(p2Monster3.getHealth()/2.0));
                        p1TrapFlag = "DP";
                    }
                }
                else if (name == "Power Break")
                {
                    if(p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.debuff(50);
                        if (p1Monster2 != null)
                            p1Monster2.debuff(50);
                        if (p1Monster3 != null)
                            p1Monster3.debuff(50);
                        p2TrapEffectCnt = 1;
                        p2TrapFlag = "PB";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.debuff(50);
                        if (p2Monster2 != null)
                            p2Monster2.debuff(50);
                        if (p2Monster3 != null)
                            p2Monster3.debuff(50);
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "PB";
                    }
                }
                else if (name == "Reinforcements")
                {
                    if(p1Turn)
                    {
                        if(p2Monster1!=null)
                            p2Monster1.buff(40);
                        if (p2Monster2!= null)
                            p2Monster2.buff(40);
                        if (p2Monster3 != null)
                            p2Monster3.buff(40);
                        p2TrapEffectCnt = 1;
                        p2TrapFlag = "RE";
                    }
                    else
                    {
                        if (p1Monster1 != null)
                            p1Monster1.debuff(40);
                        if(p1Monster2!=null)
                            p1Monster2.debuff(40);
                        if(p1Monster3!=null)
                            p1Monster3.debuff(40);
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "RE";
                    }
                }
                else if (name == "Earthshaker")
                {
                    if(p1Turn)
                    {
                        if(p1Monster1!=null)
                            p1Monster1.debuff(p1Monster1.getAttackPower());
                        if (p1Monster2 != null)
                            p1Monster2.debuff(p1Monster2.getAttackPower());
                        if (p1Monster3 != null)
                            p1Monster3.debuff(p1Monster3.getAttackPower());
                        p2TrapEffectCnt = 1;
                        p2TrapFlag = "ES";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.debuff(p2Monster1.getAttackPower());
                        if (p2Monster2 != null)
                            p2Monster2.debuff(p2Monster2.getAttackPower());
                        if (p2Monster3 != null)
                            p2Monster3.debuff(p2Monster3.getAttackPower());
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "ES"; 
                    }
                }
                else if (name == "Torrential Tribute")
                {
                    if(p1Turn)
                    {
                        if(p1Spell!=null)
                            p1Spell.destroy();
                        p2TrapFlag = "TT";
                    }
                    else
                    {
                        if (p2Spell != null)
                            p2Spell.destroy();
                        p1TrapFlag = "TT";
                    }
                }
                else if (name == "Solemn Judgment")
                {
                    if(p1Turn)
                    {
                        p1NoMagic = true;
                        p2TrapEffectCnt = 1; 
                        p2TrapFlag = "SJ";
                    }
                    else
                    {
                        p2NoMagic = true;
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "SJ";
                    }
                }
                else if (name == "Miraculous Descent")
                {
                    if(p1Turn)
                        p2TrapFlag = "MD";
                    else
                        p1TrapFlag = "MD";
                }
                else if (name == "Return from the Different Dimension")
                {
                    if(p1Turn)
                    {
                        p1NoAttack = true;
                        p2TrapEffectCnt = 1;
                        p2TrapFlag = "RD";
                    }
                    else
                    {
                        p2NoAttack = true;
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "RD";
                    }
                }
                else if (name == "Beckoning Light")
                {
                    if(p1Turn)
                    {
                        p1NoTrap = true;
                        p2TrapEffectCnt = 1;
                        p2TrapFlag = "BL";
                    }
                    else
                    {
                        p2NoTrap = true;
                        p1TrapEffectCnt = 1;
                        p1TrapFlag = "BL";
                    }
                }
        }

        private void processSpell(Card spell)
        {
            if (spell == null)
                return;
            if (p1Turn && p1SpellFlag!="none")
                return;
            else if(p2SpellFlag!="none")
                return;
            if (p1Turn && p1NoMagic)
                return;
            else if (p2NoMagic)
                return;
            string name = spell.getName();
                if (name == "Cards from the Sky")
                {
                    if (p1Turn)
                    {
                        if(p1Monster1 != null)
                            p1Monster1.setHealth((int)100);
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)100);
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)100);
                        p1SpellFlag = "CS";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)100);
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)100);
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)100);
                        p2SpellFlag = "CS";
                    }
                }
                else if (name == "Valhalla, Hall of the Fallen")
                {
                    if(p1Turn){
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)p1Monster1.getDefaultHealth());
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)p1Monster2.getDefaultHealth());
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)p1Monster3.getDefaultHealth());
                        p1SpellFlag = "V"; 
                    }
                    else{
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)p2Monster1.getDefaultHealth());
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)p2Monster2.getDefaultHealth());
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)p2Monster3.getDefaultHealth());
                        p2SpellFlag = "V";
                    }
                }
                else if (name == "Terraforming")
                {
                    if (p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)1);
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)1);
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)1);
                        p1SpellFlag = "TF";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)1);
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)1);
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)1);
                        p2SpellFlag = "TF";
                    }
                }
                else if (name == "Smashing Ground")
                {
                    if (p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)20);
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)20);
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)20);
                        p1SpellFlag = "SG";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)20);
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)20);
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)20);
                        p2SpellFlag = "SG";
                    }
                }
                else if (name == "The Sanctuary in the Sky")
                {
                    if (p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)75);
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)75);
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)75);
                        p1SpellFlag = "SS";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)75);
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)75);
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)75);
                        p2SpellFlag = "SS";
                    }
                }
                else if (name == "Celestial Transformation")
                {
                    if (p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)p1Monster1.getDefaultHealth()/2);
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)p1Monster2.getDefaultHealth()/2);
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)p1Monster3.getDefaultHealth()/2);
                        p1SpellFlag = "CT";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)p2Monster1.getDefaultHealth()/2);
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)p2Monster2.getDefaultHealth()/2);
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)p2Monster3.getDefaultHealth()/2);
                        p2SpellFlag = "CT";
                    }
                }
                else if (name == "Burial from a Different Dimension")
                {
                    if (p1Turn)
                    {
                        if (p2Trap != null)
                            p2Trap.destroy();
                        p1SpellFlag = "BDD";
                    }
                    else
                    {

                        if (p1Trap != null)
                            p1Trap.destroy();
                        p2SpellFlag = "BDD";
                    }
                }
                else if (name == "Mausoleum of the Emperor")
                {
                    if (p1Turn)
                    {
                        if(p1Monster1 != null)
                            p1Monster1.setHealth((int)(p1Monster1.getDefaultHealth() * .75));
                        if(p1Monster2 != null)
                            p1Monster2.setHealth((int)(p1Monster2.getDefaultHealth() * .75));
                        if(p1Monster3 != null)
                            p1Monster3.setHealth((int)(p1Monster3.getDefaultHealth() * .75));
                        p1SpellFlag = "ME";
                    }
                    else
                    {
                        if(p2Monster1 != null)
                            p2Monster1.setHealth((int)(p2Monster1.getDefaultHealth() * .75));
                        if(p2Monster2 != null)
                            p2Monster2.setHealth((int)(p2Monster2.getDefaultHealth() * .75));
                        if(p2Monster3 != null)
                            p2Monster3.setHealth((int)(p2Monster3.getDefaultHealth() * .75));
                        p2SpellFlag = "ME";
                    }
                }
                else if (name == "The Fountain in the Sky")
                {
                    if (p1Turn)
                    {
                        if (p1Monster1 != null)
                            p1Monster1.setHealth((int)(p1Monster1.getDefaultHealth() * .25));
                        if (p1Monster2 != null)
                            p1Monster2.setHealth((int)(p1Monster2.getDefaultHealth() * .25));
                        if (p1Monster3 != null)
                            p1Monster3.setHealth((int)(p1Monster3.getDefaultHealth() * .25));
                        p1SpellFlag = "FS";
                    }
                    else
                    {
                        if (p2Monster1 != null)
                            p2Monster1.setHealth((int)(p2Monster1.getDefaultHealth() * .25));
                        if (p2Monster2 != null)
                            p2Monster2.setHealth((int)(p2Monster2.getDefaultHealth() * .25));
                        if (p2Monster3 != null)
                            p2Monster3.setHealth((int)(p2Monster3.getDefaultHealth() * .25));
                        p1SpellFlag = "FS";
                    }
                    
                }
        }

        private void endTurn()
        {
            state = 3; 
            if (p1Turn)
            {
                if(p2Trap!=null && !p2Trap.isKO())
                    processTrap(p2Trap);
                if (p1Spell != null && !p1Spell.isKO())
                    processSpell(p1Spell);
                Console.WriteLine("End of P1 Turn!");
            }
            else
            {
                if (p1Trap != null && !p1Trap.isKO())
                    processTrap(p1Trap);
                if (p2Spell != null && !p2Spell.isKO())
                    processSpell(p2Spell);
            }

            /*Checking for collisions:
            * All that matters:
            *      1) P1M1 v P2M1
            *      2) P1M1 v P2M2
            *      3) P1M1 v P2M3
            *      4) P1M2 v P2M1
            *      5) P1M2 v P2M2
            *      6) P1M2 V P2M3
            *      7) P1M3 V P2M1
            *      8) P1M3 V P2M2
            *      9) P1M3 V P2M3
            *      
            *      NOTE:"10" is a placeholder number, can (and probably will) be adjusted. 
            */

            if (p1Monster1 != null)
            {
                //case 1
                if (p2Monster1 != null)
                {
                    if ((p1Monster1.getModel().WorldTransformation.Translation -
                        p2Monster1.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster1, p2Monster1);
                            if (p2Monster1.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                {
                                    p2life += p2Monster1.getHealth();
                                }
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck1;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster1, p1Monster1);
                            if (p1Monster1.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster1.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            nextCheck1:
                //case 2:
                if (p2Monster2 != null)
                {
                    if ((p1Monster1.getModel().WorldTransformation.Translation -
                        p2Monster2.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster1, p2Monster2);
                            if (p2Monster2.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck2;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster2, p1Monster1);
                            if (p1Monster1.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster1.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            nextCheck2:
                //case 3:
                if (p2Monster3 != null)
                {
                    if ((p1Monster1.getModel().WorldTransformation.Translation -
                        p2Monster3.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster1, p2Monster3);
                            if (p2Monster3.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck3;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster3, p1Monster1);
                            if (p1Monster1.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster1.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            }
            nextCheck3:
            if (p1Monster2 != null)
            {
                //case 4:
                if (p2Monster1 != null)
                {
                    if ((p1Monster2.getModel().WorldTransformation.Translation -
                        p2Monster1.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster2, p2Monster1);
                            if (p2Monster1.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster1.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck4;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster1, p1Monster2);
                            if (p1Monster2.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
                //case 5:
            nextCheck4: 
                if (p2Monster2 != null)
                {
                    if ((p1Monster2.getModel().WorldTransformation.Translation -
                        p2Monster2.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster2, p2Monster2);
                            if (p2Monster2.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck5;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster2, p1Monster2);
                            if (p1Monster2.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            nextCheck5:
                //case 6:
                if (p2Monster3 != null)
                {
                    if ((p1Monster2.getModel().WorldTransformation.Translation -
                        p2Monster3.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster2, p2Monster3);
                            if (p2Monster3.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck6;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster3, p1Monster2);
                            if (p1Monster2.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            }
            nextCheck6:
            if (p1Monster3 != null)
            {
                //case 7: 
                if (p2Monster1 != null)
                {
                    if ((p1Monster3.getModel().WorldTransformation.Translation -
                        p2Monster1.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster3, p2Monster1);
                            if (p2Monster1.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster1.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck7;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster1, p1Monster3);
                            if (p1Monster3.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            nextCheck7: 
                if (p2Monster2 != null)
                {
                    //case 8: 
                    if ((p1Monster3.getModel().WorldTransformation.Translation -
                        p2Monster2.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster3, p2Monster2);
                            if (p2Monster2.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster2.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextCheck8;
                        else if (!p2NoAttack)
                        {
                            registerAttack(p2Monster2, p1Monster3);
                            if (p1Monster3.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            nextCheck8:
                if (p2Monster3 != null)
                {
                    //case 9: 
                    if ((p1Monster3.getModel().WorldTransformation.Translation -
                        p2Monster3.getModel().WorldTransformation.Translation).Length() <= 20)
                    {
                        if (p1Turn && !p1NoAttack)
                        {
                            registerAttack(p1Monster3, p2Monster3);
                            if (p2Monster3.isKO())
                            {
                                if (p2TrapFlag != "MD")
                                    p2life += p2Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                        else if (p1NoAttack)
                            goto nextPart;
                        else if (!p2NoAttack && p1NoAttack)
                        {
                            registerAttack(p2Monster3, p1Monster3);
                            if (p1Monster3.isKO())
                            {
                                if (p1TrapFlag != "MD")
                                    p1life += p1Monster3.getHealth();
                            }
                            goto nextPart;
                        }
                    }
                }
            }

        nextPart:
            //if needed, restore defaults.
            if (p1Turn)
            {
                p1Turn = false;
                if (p1SpellFlag != "none")
                {
                    p1Spell.destroy();
                    p1SpellFlag = "none";
                }
            }
            else
            {
                p1Turn = true;
                if (p2SpellFlag != "none")
                {
                    p2SpellFlag = "none";
                    p2Spell.destroy();
                }
            }
            //NOTE: At this point, if it WAS P1's turn, p1Turn is now set to false. 
            //Use this property in the rest of the code in this method to restore defaults.
            p1TrapEffectCnt--;
            p2TrapEffectCnt--;
            Console.WriteLine("{0} = p2tecnt", p2TrapEffectCnt);
            if (p1TrapEffectCnt == 0)
            {
                if (p1TrapFlag != "none")
                {
                    p1Trap.destroy();
                    Console.WriteLine("P1T destroyed.");
                }
                    p1TrapEffectCnt = 0;
                    switch (p1TrapFlag)
                    {
                        case "none": return;
                        case "PB":
                            if (p2Monster1 != null)
                                p2Monster1.setLast();
                            if (p2Monster2 != null)
                                p2Monster2.setLast();
                            if (p2Monster3 != null)
                                p2Monster3.setLast();
                            break;
                        case "ES":
                            if(p2Monster1!=null)
                                p2Monster1.setLast();
                            if (p2Monster2 != null)
                                p2Monster2.setLast();
                            if (p2Monster3 != null)
                                p2Monster3.setLast();
                            break;
                        case "RE":
                            if (p1Monster1 != null)
                                p1Monster1.setLast();
                            if (p1Monster2 != null)
                                p1Monster2.setLast();
                            if (p1Monster3 != null)
                                p1Monster3.setLast();
                            break;
                        case "RD":
                            p2NoAttack = false;
                            break;
                        case "BL":
                            /*Note here: trap activated on P2's turn, 
                             *P1 turn began, then at end on P1's turn, this will be reset to P2
                            */
                            p2NoTrap = false;
                            break;
                        case "SJ":
                            p2NoMagic = false;
                            break;
                    }
                    p1TrapFlag = "none";
                    
            }
            else if (p1TrapEffectCnt < 0)
            {
                p1TrapEffectCnt = 0;
            }
            if (p2TrapEffectCnt == 0)
            {
                if (p2TrapFlag != "none")
                {
                    p2Trap.destroy();
                    Console.WriteLine("P2T destroyed.");
                }
                    p2TrapEffectCnt = 0;
                    switch (p2TrapFlag)
                    {
                        case "none": return;
                        case "PB": 
                            if (p1Monster1 != null)
                                p1Monster1.setLast();
                            if (p1Monster2 != null)
                                p1Monster2.setLast();
                            if (p1Monster3 != null)
                                p1Monster3.setLast();
                            break;
                        case "ES":
                            if (p1Monster1 != null)
                                p1Monster1.setLast();
                            if (p1Monster2 != null)
                                p1Monster2.setLast();
                            if (p1Monster3 != null)
                                p1Monster3.setLast();
                            break;
                        case "RE":
                            if (p2Monster1 != null)
                                p2Monster1.setLast();
                            if (p2Monster2 != null)
                                p2Monster2.setLast();
                            if (p2Monster3 != null)
                                p2Monster3.setLast();
                            break;
                        case "RD":
                            p1NoAttack = false;
                            break;
                        case "BL":
                            /*Note here: trap activated on P1's turn, 
                             *P2 turn began, then at end on P2's turn, this will be reset to P1
                            */
                            p1NoTrap = false;
                            break;
                        case "SJ":
                            p2NoMagic = false;
                            break;

                    }
                    p2TrapFlag = "none";
            }
            else if (p2TrapEffectCnt < 0)
            {
                Console.WriteLine("asdfkal");
                p2TrapEffectCnt = 0;
            }
            

            

        }

        protected override void LoadContent()
        {
            uiFont = Content.Load<SpriteFont>("UIFont");
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (p1Turn && cylinderMarkerNode131.MarkerFound)
            {
                endTurn();
            }
            else if (!p1Turn && cylinderMarkerNode130.MarkerFound)
            {
                endTurn();
            }

            //checking for end game at end of each turn: p1Turn reflects winner.
            if (p1life <= 0)
            {
                state = 4;
                p1Winner = false;
            }
            else if (p2life <= 0)
            {
                state = 4;
                p1Winner = true;
            }

            if (state == 3)
            {
                Console.WriteLine("Entered state 3 clause.");
                int checker = 0; //when checker = 0, break from state. 
                if (p1Monster1 != null && p1Monster1.isKO())
                {
                    if (p1Monster1.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p1LifeLab.Text = p1life.ToString() + " LP";
                        p1Monster1 = null;
                        p1m1LifeLab.Text = "";
                    }
                }
                if (p1Monster2 != null && p1Monster2.isKO())
                {
                    if (p1Monster2.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p1LifeLab.Text = p1life.ToString() + " LP";
                        p1Monster2 = null;
                        p1m2LifeLab.Text = "";
                    }
                }

                if (p1Monster3 != null && p1Monster3.isKO())
                {
                    if (p1Monster3.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p1LifeLab.Text = p1life.ToString() + " LP";
                        p1Monster3 = null;
                        p1m3LifeLab.Text = "";
                    }
                }

                if (p2Monster1 != null && p2Monster1.isKO())
                {
                    if (p2Monster1.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p2LifeLab.Text = p2life.ToString() + " LP";
                        p2Monster1 = null;
                        p2m1LifeLab.Text = "";
                    }
                }

                if (p2Monster2 != null && p2Monster2.isKO())
                {
                    if (p2Monster2.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p2LifeLab.Text = p2life.ToString() + " LP";
                        p2Monster2 = null;
                        p2m2LifeLab.Text = "";
                    }
                }

                if (p2Monster3 != null && p2Monster3.isKO())
                {
                    if (p2Monster3.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p2LifeLab.Text = p2life.ToString() + " LP";
                        p2Monster3 = null;
                        p2m3LifeLab.Text = "";
                    }
                }

                if (p1Spell != null && p1Spell.isKO())
                {
                    if (p1Spell.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p1Spell = null;
                    }
                }

                if (p1Trap != null && p1Trap.isKO())
                {
                    if (p1Trap.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p1Trap = null;
                    }
                }

                if (p2Spell != null && p2Spell.isKO())
                {
                    if (p2Spell.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p2Spell = null;
                    }
                }

                if (p2Trap != null && p2Trap.isKO())
                {
                    if (p2Trap.getModel().MarkerFound)
                    {
                        //change model color to red
                        checker++;
                    }
                    else
                    {
                        p2Trap = null;
                    }
                }
                
                    if (checker != 0)
                        text = "There are destroyed cards on the board that have not been removed. Please remove them to continue.";
                    else
                    {
                        text = "";
                        state = 1; 
                    }
                    Console.WriteLine("checker = " + checker.ToString());
                    UI2DRenderer.WriteText(Vector2.Zero, text, Color.Red,
                        uiFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top);

                    if (p1Monster1 != null)
                        p1m1LifeLab.Text = p1Monster1.getAttackPower().ToString() + " ATK / " +
                            p1Monster1.getHealth().ToString() + " HP";
                    if (p1Monster2 != null)
                        p1m2LifeLab.Text = p1Monster2.getAttackPower().ToString() + " ATK / " +
                            p1Monster2.getHealth().ToString() + " HP";
                    if (p1Monster3 != null)
                        p1m3LifeLab.Text = p1Monster3.getAttackPower().ToString() + " ATK / " +
                            p1Monster3.getHealth().ToString() + " HP";

                    if (p2Monster1 != null)
                        p2m1LifeLab.Text = p2Monster1.getAttackPower().ToString() + " ATK / " +
                            p2Monster1.getHealth().ToString() + " HP";
                    if (p2Monster2 != null)
                        p2m2LifeLab.Text = p2Monster2.getAttackPower().ToString() + " ATK / " +
                            p2Monster2.getHealth().ToString() + " HP";
                    if (p2Monster3 != null)
                        p2m3LifeLab.Text = p2Monster3.getAttackPower().ToString() + " ATK / " +
                            p2Monster3.getHealth().ToString() + " HP";
                p1LifeLab.Text = p1life.ToString() + " LP";    
                p2LifeLab.Text = p2life.ToString() + " LP";
                    return;

            }
            /*Checking for player cards via proximity checks
             * Checking:
             *      1) P1 Monster Marker (3) -> monsters (Markers 32, 33, 34)
                    2) P2 Monster Marker (3) -> monsters (Markers 35, 36, 37)
             *      3) P1 Spell Marker (1) -> spell (Marker 38)
             *      4) P2 Spell Marker (1) -> spell (Marker 39)
             *      5) P1 Trap Marker (1) -> trap (Marker 40)
             *      6) P2 Trap Marker (1) -> trap (Marker 41)
             */
            int x;

            //Monster marker proximity checks
            Vector3 m32Pos = cylinderMarkerNode132.WorldTransformation.Translation;
            Vector3 m33Pos = cylinderMarkerNode133.WorldTransformation.Translation;
            Vector3 m34Pos = cylinderMarkerNode134.WorldTransformation.Translation;
            Vector3 m35Pos = cylinderMarkerNode135.WorldTransformation.Translation;
            Vector3 m36Pos = cylinderMarkerNode136.WorldTransformation.Translation;
            Vector3 m37Pos = cylinderMarkerNode137.WorldTransformation.Translation;
            Vector3 monCardPos; 
            for (x = 0; x < 10; x++)
            {
                if (!(cards[x].getModel().MarkerFound))
                {
                    continue; 
                }
                monCardPos = cards[x].getModel().WorldTransformation.Translation;
                if (p1Monster1==null && (m32Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster2 == cards[x])
                        break;
                    if (p1Monster3 == cards[x])
                        break;
                    if (p2Monster1 == cards[x])
                        break;
                    if (p2Monster2 == cards[x])
                        break;
                    if (p2Monster3 == cards[x])
                        break;
                    p1Monster1 = cards[x];
                }
                else if (p1Monster2 == null && (m33Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster1 == cards[x])
                        break;
                    if (p1Monster3 == cards[x])
                        break;
                    if (p2Monster1 == cards[x])
                        break;
                    if (p2Monster2 == cards[x])
                        break;
                    if (p2Monster3 == cards[x])
                        break;
                    p1Monster2 = cards[x];
                }
                else if (p1Monster3 == null && (m34Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster2 == cards[x])
                        break;
                    if (p1Monster1 == cards[x])
                        break;
                    if (p2Monster1 == cards[x])
                        break;
                    if (p2Monster2 == cards[x])
                        break;
                    if (p2Monster3 == cards[x])
                        break;
                    p1Monster3 = cards[x];
                }
                else if (p2Monster1 == null && (m35Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster2 == cards[x])
                        break;
                    if (p1Monster3 == cards[x])
                        break;
                    if (p1Monster1 == cards[x])
                        break;
                    if (p2Monster2 == cards[x])
                        break;
                    if (p2Monster3 == cards[x])
                        break;
                    p2Monster1 = cards[x];
                }
                else if (p2Monster2 == null && (m36Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster2 == cards[x])
                        break;
                    if (p1Monster3 == cards[x])
                        break;
                    if (p2Monster1 == cards[x])
                        break;
                    if (p1Monster1 == cards[x])
                        break;
                    if (p2Monster3 == cards[x])
                        break;
                    p2Monster2 = cards[x];
                }
                else if (p2Monster3 == null && (m37Pos - monCardPos).Length() <= 20)
                {
                    if (p1Monster2 == cards[x])
                        break;
                    if (p1Monster3 == cards[x])
                        break;
                    if (p2Monster1 == cards[x])
                        break;
                    if (p2Monster2 == cards[x])
                        break;
                    if (p1Monster1 == cards[x])
                        break;
                    p2Monster3 = cards[x];
                }
            }

            //spell marker proximity checks
            Vector3 m38Pos = cylinderMarkerNode138.WorldTransformation.Translation;
            Vector3 m39Pos = cylinderMarkerNode139.WorldTransformation.Translation;
            Vector3 spellCardPos;
            for (x = 10; x < 20; x++)
            {
                if (!(cards[x].getModel().MarkerFound))
                {
                    continue;
                }
                spellCardPos = cards[x].getModel().WorldTransformation.Translation;
                if (p1Spell==null && (m38Pos - spellCardPos).Length() <= 20)
                {
                    p1Spell = cards[x];
                }
                else if (p2Spell==null && (m39Pos - spellCardPos).Length() <= 20)
                {
                    p2Spell = cards[x];
                }
            }

            //spell marker proximity checks
            Vector3 m40Pos = cylinderMarkerNode140.WorldTransformation.Translation;
            Vector3 m41Pos = cylinderMarkerNode141.WorldTransformation.Translation;
            Vector3 trapCardPos;
            for (x = 20; x < 30; x++)
            {
                if (!(cards[x].getModel().MarkerFound))
                    continue;
                trapCardPos = cards[x].getModel().WorldTransformation.Translation;
                if (p1Trap==null &&(m40Pos - trapCardPos).Length() <= 20)
                {
                    p1Trap = cards[x];
                }
                else if (p2Trap==null && (m41Pos - trapCardPos).Length() <= 20)
                {
                    p2Trap = cards[x];
                }
            }
            if(p1Monster1!=null)
                p1m1LifeLab.Text = p1Monster1.getAttackPower().ToString() + " ATK / " +
                    p1Monster1.getHealth().ToString() + " HP";
            if(p1Monster2!=null)
                p1m2LifeLab.Text = p1Monster2.getAttackPower().ToString() + " ATK / " +
                    p1Monster2.getHealth().ToString() + " HP";
            if(p1Monster3!=null)
                p1m3LifeLab.Text = p1Monster3.getAttackPower().ToString() + " ATK / " +
                    p1Monster3.getHealth().ToString() + " HP";

            if(p2Monster1!=null)
                p2m1LifeLab.Text = p2Monster1.getAttackPower().ToString() + " ATK / " +
                    p2Monster1.getHealth().ToString() + " HP";
            if(p2Monster2!=null)
                p2m2LifeLab.Text = p2Monster2.getAttackPower().ToString() + " ATK / " +
                    p2Monster2.getHealth().ToString() + " HP";
            if(p2Monster3!=null)
                p2m3LifeLab.Text = p2Monster3.getAttackPower().ToString() + " ATK / " +
                    p2Monster3.getHealth().ToString() + " HP";

            

            base.Update(gameTime);
        }


        private void registerAttack(Card attacking, Card attacked)
        {
            attacking.attacking(attacked);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Draw(GameTime gameTime)
        {
            if (state == 4)
            {
                if (p1Winner)
                {
                    UI2DRenderer.FillRectangle(new Rectangle(100, 100, 600, 400), null, Color.CornflowerBlue);
                    UI2DRenderer.WriteText(Vector2.Zero, "Player 1 has won the game!", Color.Black, uiFont,
                        GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Center); 
                }
                else
                {
                    UI2DRenderer.FillRectangle(new Rectangle(100, 100, 600, 400), null, Color.CornflowerBlue);
                    UI2DRenderer.WriteText(Vector2.Zero, "Player 2 has won the game!", Color.Black, uiFont,
                        GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Center); 
                }
            }
            base.Draw(gameTime);
        }
    }
}
