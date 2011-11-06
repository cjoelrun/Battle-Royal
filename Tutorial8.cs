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


namespace Tutorial8___Optical_Marker_Tracking
{
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
        MarkerNode groundMarkerNode, toolbarMarkerNode, cylinderMarkerNode100,
            cylinderMarkerNode101, cylinderMarkerNode102, cylinderMarkerNode103, cylinderMarkerNode104,
            cylinderMarkerNode105, cylinderMarkerNode106, cylinderMarkerNode107, cylinderMarkerNode108,
            cylinderMarkerNode109, cylinderMarkerNode110, cylinderMarkerNode111, cylinderMarkerNode112,
            cylinderMarkerNode113, cylinderMarkerNode114;

        MarkerNode cylinderMarkerNode115, cylinderMarkerNode116, cylinderMarkerNode117,
            cylinderMarkerNode118, cylinderMarkerNode119, cylinderMarkerNode120, cylinderMarkerNode121,
            cylinderMarkerNode122, cylinderMarkerNode123, cylinderMarkerNode124, cylinderMarkerNode125,
            cylinderMarkerNode126, cylinderMarkerNode127, cylinderMarkerNode128, cylinderMarkerNode129;

        GeometryNode boxNode;
        bool useStaticImage = false;
        GeometryNode[] blah = new GeometryNode[30];
        public Tutorial8()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private struct Card
        {
            ///variables
            /*markerNum will store the number of the marker that a particular 
             *Card object is referenced to.*/
            static int markerNum;

            /*type will store one of three values: 'M', 'S' or 'T'
             *defining the card as type monster, spell or trap respectively.*/
            static char type;

            /*model will store the data pertaining to the display of the monster
             * on the marker representing this paricular Card.*/
            TransformNode model;

            int attackPower;
            int health;
            bool ko;

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
            public Card(char type, TransfromNode model, int atk, int health)
            {
                this.type = type;
                this.model = model;
                attackPower = atk;
                this.health = health;
                ko = false;
            }

            //getType fetches type and returns it.
            public char getType()
            {
                return type;
            }

            //getModel fetches model and returns it
            public TransformNode getModel()
            {
                return model;
            }

            /* DEBATABLE METHOD:
             * In the case that the model of the monster would have to be changed somehow
             * (i.e color of monster needs to be changed),
             * this method exists to change the model in its entirety.
             * parameter: newModel (TransformNode)
             * 
             * */
            public void setModel(TransformNode newModel)
            {
                model = newModel;
            }

            /* isAttackingMonster exists to update damage given to another monster
             * parameter: target (Card data structure)
             * NOTE: no need for an isBeingAttacked method since this method will 
             * always be called by the attacking monster to edit values in the target monster.
             * */
            public int attacking(Card target)
            {
                return target.takeDamage(attackPower);
            }

            /* takeDamage is used to register damage taken and subtract from the health pool of a given monster.
             * parameter: amount (int)
             * return: int value holding amount of damage done to owner of destroyed card. 
             * */
            public int takeDamage(int amount)
            {
                health -= amount;
                if (health <= 0)
                {
                    ko = true;
                }
            }

            public void applyBuff(int amount)
            {
                attackPower += amount;
            }
            public void removeBuff(int amount)
            {
                attackPower -= amount;
            }

        }
        Card[] cards = new Card[30];
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

            groundNode.Material = groundMaterial;

            groundMarkerNode.AddChild(groundNode);
        }

        private void CreateObjects()
        {
            // Create a geometry node with a model of a sphere that will be overlaid on
            // top of the ground marker array
            GeometryNode sphereNode = new GeometryNode("Sphere");
            sphereNode.Model = new Sphere(3, 20, 20);

            // Add this sphere model to the physics engine for collision detection
            sphereNode.AddToPhysicsEngine = true;
            sphereNode.Physics.Shape = ShapeType.Sphere;
            // Make this sphere model cast and receive shadowsf
            sphereNode.Model.CastShadows = true;
            sphereNode.Model.ReceiveShadows = true;

            // Create a marker node to track a ground marker array.
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");

            // Since the ground marker's size is 80x52 ARTag units, in order to move the sphere model
            // to the center of the ground marker, we shift it by 40x26 units and also make it
            // float from the ground marker's center
            TransformNode sphereTransNode = new TransformNode();
            sphereTransNode.Translation = new Vector3(40, 26, 10);

            // Create a material to apply to the sphere model
            Material sphereMaterial = new Material();
            sphereMaterial.Diffuse = new Vector4(0, 0.5f, 0, 1);
            sphereMaterial.Specular = Color.Green.ToVector4();
            sphereMaterial.SpecularPower = 10;

            sphereNode.Material = sphereMaterial;

            // Now add the above nodes to the scene graph in the appropriate order.
            // Note that only the nodes added below the marker node are affected by 
            // the marker transformation.
            scene.RootNode.AddChild(groundMarkerNode);
            groundMarkerNode.AddChild(sphereTransNode);
            sphereTransNode.AddChild(sphereNode);

            // Create a geometry node with a model of a box that will be overlaid on
            // top of the ground marker array initially. (When the toolbar marker array is
            // detected, it will be overlaid on top of the toolbar marker array.)
            boxNode = new GeometryNode("Box");
            boxNode.Model = new Box(8);

            // Add this box model to the physics engine for collision detection
            boxNode.AddToPhysicsEngine = true;
            boxNode.Physics.Shape = ShapeType.Box;
            // Make this box model cast and receive shadows
            boxNode.Model.CastShadows = true;
            boxNode.Model.ReceiveShadows = true;

            // Create a marker node to track a toolbar marker array.
            toolbarMarkerNode = new MarkerNode(scene.MarkerTracker, "Toolbar.txt");

            scene.RootNode.AddChild(toolbarMarkerNode);

            // Create a material to apply to the box model
            Material boxMaterial = new Material();
            boxMaterial.Diffuse = new Vector4(0.5f, 0, 0, 1);
            boxMaterial.Specular = Color.White.ToVector4();
            boxMaterial.SpecularPower = 10;

            boxNode.Material = boxMaterial;

            // Add this box model node to the ground marker node
            groundMarkerNode.AddChild(boxNode);

            // Create a collision pair and add a collision callback function that will be
            // called when the pair collides
            NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(boxNode.Physics, sphereNode.Physics);
            ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, BoxSphereCollision);

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


            //Marker 100
            cylinderMarkerNode100 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML100.xml", zero);
            GeometryNode cylinderNode0 = new GeometryNode("Cylinder");

            cylinderNode0.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode0.Material = sphereMaterial;

            TransformNode cylinderTransNode0 = new TransformNode();

            cylinderTransNode0.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode100.AddChild(cylinderTransNode0);

            cylinderTransNode0.AddChild(cylinderNode0);

            //add to Card array here: generic monster card here
            cards[0] = new Card('M', cylinderTransNode100, 100, 100);

            //Marker 101
            cylinderMarkerNode101 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML101.xml", first);
            GeometryNode cylinderNode1 = new GeometryNode("Cylinder");

            cylinderNode1.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode1.Material = sphereMaterial;

            TransformNode cylinderTransNode1 = new TransformNode();

            cylinderTransNode1.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode101.AddChild(cylinderTransNode1);

            cylinderTransNode1.AddChild(cylinderNode1);

            //add to Card array here: generic monster card here
            cards[1] = new Card('M', cylinderTransNode101, 100, 100);

            //Marker 102
            cylinderMarkerNode102 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML102.xml", two);
            GeometryNode cylinderNode2 = new GeometryNode("Cylinder");

            cylinderNode2.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode2.Material = sphereMaterial;

            TransformNode cylinderTransNode2 = new TransformNode();

            cylinderTransNode2.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode102.AddChild(cylinderTransNode2);

            cylinderTransNode2.AddChild(cylinderNode2);

            //add to Card array here: generic monster card here
            cards[2] = new Card('M', cylinderTransNode102, 100, 100);

            //Marker 103
            cylinderMarkerNode103 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML103.xml", three);
            GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

            cylinderNode3.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode3.Material = sphereMaterial;

            TransformNode cylinderTransNode3 = new TransformNode();

            cylinderTransNode3.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode103.AddChild(cylinderTransNode3);

            cylinderTransNode3.AddChild(cylinderNode3);

            //add to Card array here: generic monster card here
            cards[3] = new Card('M', cylinderTransNode103, 100, 100);

            //Marker 104
            cylinderMarkerNode104 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML104.xml", four);
            GeometryNode cylinderNode4 = new GeometryNode("Cylinder");

            cylinderNode4.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode4.Material = sphereMaterial;

            TransformNode cylinderTransNode4 = new TransformNode();

            cylinderTransNode4.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode104.AddChild(cylinderTransNode4);

            cylinderTransNode4.AddChild(cylinderNode4);

            //add to Card array here: generic monster card here
            cards[4] = new Card('M', cylinderTransNode104, 100, 100);

            //Marker 105
            cylinderMarkerNode105 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML105.xml", five);
            GeometryNode cylinderNode5 = new GeometryNode("Cylinder");

            cylinderNode5.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode5.Material = sphereMaterial;

            TransformNode cylinderTransNode5 = new TransformNode();

            cylinderTransNode5.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode105.AddChild(cylinderTransNode5);

            cylinderTransNode5.AddChild(cylinderNode5);

            //add to Card array here: generic monster card here
            cards[5] = new Card('M', cylinderTransNode105, 100, 100);

            //Marker 106
            cylinderMarkerNode106 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML106.xml", six);
            GeometryNode cylinderNode6 = new GeometryNode("Cylinder");

            cylinderNode6.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode6.Material = sphereMaterial;

            TransformNode cylinderTransNode6 = new TransformNode();

            cylinderTransNode6.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode106.AddChild(cylinderTransNode6);

            cylinderTransNode6.AddChild(cylinderNode6);

            //add to Card array here: generic monster card here
            cards[6] = new Card('M', cylinderTransNode6, 100, 100);

            //Marker 107
            cylinderMarkerNode107 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML107.xml", seven);
            GeometryNode cylinderNode7 = new GeometryNode("Cylinder");

            cylinderNode7.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode7.Material = sphereMaterial;

            TransformNode cylinderTransNode7 = new TransformNode();

            cylinderTransNode7.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode107.AddChild(cylinderTransNode7);

            cylinderTransNode7.AddChild(cylinderNode7);

            //add to Card array here: generic monster card here
            cards[7] = new Card('M', cylinderTransNode107, 100, 100);

            //Marker 108
            cylinderMarkerNode108 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML108.xml", eight);
            GeometryNode cylinderNode8 = new GeometryNode("Cylinder");

            cylinderNode8.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode8.Material = sphereMaterial;

            TransformNode cylinderTransNode8 = new TransformNode();

            cylinderTransNode8.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode108.AddChild(cylinderTransNode8);

            cylinderTransNode8.AddChild(cylinderNode8);

            //add to Card array here: generic monster card here
            cards[8] = new Card('M', cylinderTransNode108, 100, 100);

            //Marker 109
            cylinderMarkerNode109 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML109.xml", nine);
            GeometryNode cylinderNode9 = new GeometryNode("Cylinder");

            cylinderNode9.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode9.Material = sphereMaterial;

            TransformNode cylinderTransNode9 = new TransformNode();

            cylinderTransNode9.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode109.AddChild(cylinderTransNode9);

            cylinderTransNode9.AddChild(cylinderNode9);

            //add to Card array here: generic monster card here
            cards[9] = new Card('M', cylinderTransNode109, 100, 100);

            //The following line is to be placed before the 110th node to hange color to Blue
            sphereMaterial.Diffuse = new Vector4(0, 0, 0.5f, 1);

            //Marker 110
            cylinderMarkerNode110 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML110.xml", ten);
            GeometryNode cylinderNode10 = new GeometryNode("Cylinder");

            cylinderNode10.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode10.Material = sphereMaterial;

            TransformNode cylinderTransNode10 = new TransformNode();

            cylinderTransNode10.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode110.AddChild(cylinderTransNode10);

            cylinderTransNode10.AddChild(cylinderNode10);

            //add to Card array here: generic spell card here
            cards[10] = new Card('S', cylinderTransNode110, 100, 100);

            //Marker 111
            cylinderMarkerNode111 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML111.xml", eleven);
            GeometryNode cylinderNode11 = new GeometryNode("Cylinder");

            cylinderNode11.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode11.Material = sphereMaterial;

            TransformNode cylinderTransNode11 = new TransformNode();

            cylinderTransNode11.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode111.AddChild(cylinderTransNode11);

            cylinderTransNode11.AddChild(cylinderNode11);

            //add to Card array here: generic spell card here
            cards[11] = new Card('S', cylinderTransNode111, 100, 100);

            //Marker 112
            cylinderMarkerNode112 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML112.xml", twelve);
            GeometryNode cylinderNode12 = new GeometryNode("Cylinder");

            cylinderNode12.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode12.Material = sphereMaterial;

            TransformNode cylinderTransNode12 = new TransformNode();

            cylinderTransNode12.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode112.AddChild(cylinderTransNode12);

            cylinderTransNode12.AddChild(cylinderNode12);

            //add to Card array here: generic spell card here
            cards[12] = new Card('S', cylinderTransNode112, 100, 100);

            //Marker 113
            cylinderMarkerNode113 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML113.xml", thirteen);
            GeometryNode cylinderNode13 = new GeometryNode("Cylinder");

            cylinderNode13.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode13.Material = sphereMaterial;

            TransformNode cylinderTransNode13 = new TransformNode();

            cylinderTransNode13.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode113.AddChild(cylinderTransNode13);

            cylinderTransNode13.AddChild(cylinderNode13);

            //add to Card array here: generic spell card here
            cards[13] = new Card('S', cylinderTransNode113, 100, 100);

            //Marker 114
            cylinderMarkerNode114 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML114.xml", fourteen);
            GeometryNode cylinderNode14 = new GeometryNode("Cylinder");

            cylinderNode14.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode14.Material = sphereMaterial;

            TransformNode cylinderTransNode14 = new TransformNode();

            cylinderTransNode14.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode114.AddChild(cylinderTransNode14);

            cylinderTransNode14.AddChild(cylinderNode14);

            //add to Card array here: generic spell card here
            cards[14] = new Card('S', cylinderTransNode114, 100, 100);

            //Marker 115
            cylinderMarkerNode115 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML115.xml", fifteen);
            GeometryNode cylinderNode15 = new GeometryNode("Cylinder");

            cylinderNode15.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode15.Material = sphereMaterial;

            TransformNode cylinderTransNode15 = new TransformNode();

            cylinderTransNode15.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode115.AddChild(cylinderTransNode15);

            cylinderTransNode15.AddChild(cylinderNode15);

            //add to Card array here: generic spell card here
            cards[15] = new Card('S', cylinderTransNode115, 100, 100);

            //Marker 116
            cylinderMarkerNode116 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML116.xml", sixteen);
            GeometryNode cylinderNode16 = new GeometryNode("Cylinder");

            cylinderNode16.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode16.Material = sphereMaterial;

            TransformNode cylinderTransNode16 = new TransformNode();

            cylinderTransNode16.Translation = new Vector3(0, 0, 3);

            cylinderTransNode16.AddChild(cylinderNode16);

            cylinderMarkerNode116.AddChild(cylinderTransNode16);

            //add to Card array here: generic spell card here
            cards[16] = new Card('S', cylinderTransNode116, 100, 100);

            //Marker 117
            cylinderMarkerNode117 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML117.xml", seventeen);
            GeometryNode cylinderNode17 = new GeometryNode("Cylinder");

            cylinderNode17.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode17.Material = sphereMaterial;

            TransformNode cylinderTransNode17 = new TransformNode();

            cylinderTransNode17.Translation = new Vector3(0, 0, 3);

            cylinderTransNode17.AddChild(cylinderNode17);

            cylinderMarkerNode117.AddChild(cylinderTransNode17);

            //add to Card array here: generic spell card here
            cards[17] = new Card('S', cylinderTransNode117, 100, 100);

            //Marker 118
            cylinderMarkerNode118 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML118.xml", eighteen);
            GeometryNode cylinderNode18 = new GeometryNode("Cylinder");

            cylinderNode18.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode18.Material = sphereMaterial;

            TransformNode cylinderTransNode18 = new TransformNode();

            cylinderTransNode18.Translation = new Vector3(0, 0, 3);

            cylinderTransNode18.AddChild(cylinderNode18);

            cylinderMarkerNode118.AddChild(cylinderTransNode18);

            //add to Card array here: generic spell card here
            cards[18] = new Card('S', cylinderTransNode118, 100, 100);

            //Marker 119
            cylinderMarkerNode119 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML119.xml", nineteen);
            GeometryNode cylinderNode19 = new GeometryNode("Cylinder");

            cylinderNode19.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode19.Material = sphereMaterial;

            TransformNode cylinderTransNode19 = new TransformNode();

            cylinderTransNode19.Translation = new Vector3(0, 0, 3);

            cylinderTransNode19.AddChild(cylinderNode19);

            cylinderMarkerNode119.AddChild(cylinderTransNode19);

            //add to Card array here: generic spell card here
            cards[19] = new Card('S', cylinderTransNode119, 100, 100);

            //changing color to Red
            sphereMaterial.Diffuse = new Vector4(0.5f, 0, 0, 1);

            //Marker 120
            cylinderMarkerNode120 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML120.xml", twenty);
            GeometryNode cylinderNode20 = new GeometryNode("Cylinder");

            cylinderNode20.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode20.Material = sphereMaterial;

            TransformNode cylinderTransNode20 = new TransformNode();

            cylinderTransNode20.Translation = new Vector3(0, 0, 3);

            cylinderTransNode20.AddChild(cylinderNode20);

            cylinderMarkerNode120.AddChild(cylinderTransNode20);

            //add to Card array here: generic trap card here
            cards[20] = new Card('T', cylinderTransNode120, 0, 100);

            //Marker 121
            cylinderMarkerNode121 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML121.xml", twentyone);
            GeometryNode cylinderNode21 = new GeometryNode("Cylinder");

            cylinderNode21.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode21.Material = sphereMaterial;

            TransformNode cylinderTransNode21 = new TransformNode();

            cylinderTransNode21.Translation = new Vector3(0, 0, 3);

            cylinderTransNode21.AddChild(cylinderNode21);

            cylinderMarkerNode121.AddChild(cylinderTransNode21);

            //add to Card array here: generic trap card here
            cards[21] = new Card('T', cylinderTransNode121, 0, 100);

            //Marker 122
            cylinderMarkerNode122 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML122.xml", twentytwo);
            GeometryNode cylinderNode22 = new GeometryNode("Cylinder");

            cylinderNode22.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode22.Material = sphereMaterial;

            TransformNode cylinderTransNode22 = new TransformNode();

            cylinderTransNode22.Translation = new Vector3(0, 0, 3);

            cylinderTransNode22.AddChild(cylinderNode22);

            cylinderMarkerNode122.AddChild(cylinderTransNode22);

            //add to Card array here: generic trap card here
            cards[22] = new Card('T', cylinderTransNode122, 0, 100);

            //Marker 123
            cylinderMarkerNode123 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML123.xml", twentythree);
            GeometryNode cylinderNode23 = new GeometryNode("Cylinder");

            cylinderNode23.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode23.Material = sphereMaterial;

            TransformNode cylinderTransNode23 = new TransformNode();

            cylinderTransNode23.Translation = new Vector3(0, 0, 3);

            cylinderTransNode23.AddChild(cylinderNode23);

            cylinderMarkerNode123.AddChild(cylinderTransNode23);

            //add to Card array here: generic trap card here
            cards[23] = new Card('T', cylinderTransNode123, 0, 100);

            //Marker 124
            cylinderMarkerNode124 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML124.xml", twentyfour);
            GeometryNode cylinderNode24 = new GeometryNode("Cylinder");

            cylinderNode24.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode24.Material = sphereMaterial;

            TransformNode cylinderTransNode24 = new TransformNode();

            cylinderTransNode24.Translation = new Vector3(0, 0, 3);

            cylinderTransNode24.AddChild(cylinderNode24);

            cylinderMarkerNode124.AddChild(cylinderTransNode24);

            //add to Card array here: generic trap card here
            cards[24] = new Card('T', cylinderTransNode124, 0, 100);

            //Marker 125
            cylinderMarkerNode125 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML125.xml", twentyfive);
            GeometryNode cylinderNode25 = new GeometryNode("Cylinder");

            cylinderNode25.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode25.Material = sphereMaterial;

            TransformNode cylinderTransNode25 = new TransformNode();

            cylinderTransNode25.Translation = new Vector3(0, 0, 3);

            cylinderTransNode25.AddChild(cylinderNode25);

            cylinderMarkerNode125.AddChild(cylinderTransNode25);

            //add to Card array here: generic trap card here
            cards[25] = new Card('T', cylinderTransNode125, 0, 100);

            //Marker 126
            cylinderMarkerNode126 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML126.xml", twentysix);
            GeometryNode cylinderNode26 = new GeometryNode("Cylinder");

            cylinderNode26.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode26.Material = sphereMaterial;

            TransformNode cylinderTransNode26 = new TransformNode();

            cylinderTransNode26.Translation = new Vector3(0, 0, 3);

            cylinderTransNode26.AddChild(cylinderNode26);

            cylinderMarkerNode126.AddChild(cylinderTransNode26);

            //add to Card array here: generic trap card here
            cards[26] = new Card('T', cylinderTransNode126, 0, 100);

            //Marker 127
            cylinderMarkerNode127 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML127.xml", twentyseven);
            GeometryNode cylinderNode27 = new GeometryNode("Cylinder");

            cylinderNode27.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode27.Material = sphereMaterial;

            TransformNode cylinderTransNode27 = new TransformNode();

            cylinderTransNode27.Translation = new Vector3(0, 0, 3);

            cylinderTransNode27.AddChild(cylinderNode27);

            cylinderMarkerNode127.AddChild(cylinderTransNode27);


            //add to Card array here: generic trap card here
            cards[27] = new Card('T', cylinderTransNode127, 0, 100);

            //Marker 128
            cylinderMarkerNode128 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML128.xml", twentyeight);
            GeometryNode cylinderNode28 = new GeometryNode("Cylinder");

            cylinderNode28.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode28.Material = sphereMaterial;

            TransformNode cylinderTransNode28 = new TransformNode();

            cylinderTransNode28.Translation = new Vector3(0, 0, 3);

            cylinderTransNode28.AddChild(cylinderNode28);

            cylinderMarkerNode128.AddChild(cylinderTransNode28);

            //add to Card array here: generic trap card here
            cards[28] = new Card('T', cylinderTransNode128, 0, 100);

            //Marker 129
            cylinderMarkerNode129 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML129.xml", twentynine);
            GeometryNode cylinderNode29 = new GeometryNode("Cylinder");

            cylinderNode29.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode29.Material = sphereMaterial;

            TransformNode cylinderTransNode29 = new TransformNode();

            cylinderTransNode29.Translation = new Vector3(0, 0, 3);

            cylinderTransNode29.AddChild(cylinderNode29);

            cylinderMarkerNode129.AddChild(cylinderTransNode29);

            //add to Card array here: generic trap card here
            cards[29] = new Card('T', cylinderTransNode129, 0, 100);

            blah[0] = cylinderNode0;
            blah[1] = cylinderNode1;
            blah[2] = cylinderNode2;
            blah[3] = cylinderNode3;
            blah[4] = cylinderNode4;
            blah[5] = cylinderNode5;
            blah[6] = cylinderNode6;
            blah[7] = cylinderNode7;
            blah[8] = cylinderNode8;
            blah[9] = cylinderNode9;
            blah[10] = cylinderNode10;
            blah[11] = cylinderNode11;
            blah[12] = cylinderNode12;
            blah[13] = cylinderNode13;
            blah[14] = cylinderNode14;
            blah[15] = cylinderNode15;
            blah[16] = cylinderNode16;
            blah[17] = cylinderNode17;
            blah[18] = cylinderNode18;
            blah[19] = cylinderNode19;
            blah[20] = cylinderNode20;
            blah[21] = cylinderNode21;
            blah[22] = cylinderNode22;
            blah[23] = cylinderNode23;
            blah[24] = cylinderNode24;
            blah[25] = cylinderNode25;
            blah[26] = cylinderNode26;
            blah[27] = cylinderNode27;
            blah[28] = cylinderNode28;
            blah[29] = cylinderNode29;

            NewtonPhysics.CollisionPair[] blah2 = new NewtonPhysics.CollisionPair[435];
            int k = 0;

            for (int i = 0; i < 30; i++)
            {
                for (int j = i + 1; j < 30; j++)
                {
                    blah2[k] = new NewtonPhysics.CollisionPair(blah[i].Physics, blah[j].Physics);
                    k++;
                }
            }

            for (int i = 0; i < blah2.Length; i++)
            {
                ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(blah2[i], arCollision);
            }

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

        }

        /// <summary>
        /// A callback function that will be called when the box and sphere model collides
        /// </summary>
        /// <param name="pair"></param>
        private void BoxSphereCollision(NewtonPhysics.CollisionPair pair)
        {
            Console.WriteLine("Box and Sphere has collided");
        }

        private void arCollision(NewtonPhysics.CollisionPair pair)
        {
            //Console.WriteLine("We have a collision!");
            int index1 = -1, index2 = -1;

            for (int x = 0; x < 30; x++)
            {
                if (pair.CollisionObject1.Equals(blah[x].Physics))
                    index1 = x;
                else if (pair.CollisionObject2.Equals(blah[x].Physics))
                    index2 = x;
                if (index1 != -1 && index2 != -1)
                    break;
            }
            if ((index1 == -1 || index2 == -1) && x == 30)
            {
                //error!
                return;
            }
            Card cardOne = cards[index1];
            Card cardTwo = cards[index2];
            if (cardOne.getType() == 'M' && cardTwo.getType() == 'M')
            {
                //monster v. monster logic here
            }
            else if ((cardOne.getType() == 'M' && cardTwo.getType() == 'T') ||
                    (cardOne.getType() == 'T' && cardTwo.getType() == 'M'))
            {
                //moneter v. trap logic here
            }
            else if ((cardOne.getType() == 'M' && cardTwo.getType() == 'S') ||
                    (cardOne.getType() == 'S' && cardTwo.getType() == 'M'))
            {
                //monster v. spell logic here
            }
            else if (cardOne.getType() == 'S' && cardTwo.getType() == 'S')
            {
                //spell v. spell logic here
            }
            else if ((cardOne.getType() == 'S' && cardTwo.getType() == 'T') ||
                    (cardOne.getType() == 'T' && cardTwo.getType() == 'S'))
            {
                //spell v. trap logic here
            }
            else if (cardOne.getType() == 'T' && cardTwo.getType() == 'T')
            {
                //trap v. trap logic here
            }

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
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // If ground marker array is detected
            if (groundMarkerNode.MarkerFound)
            {
                // If the toolbar marker array is detected, then overlay the box model on top
                // of the toolbar marker array; otherwise, overlay the box model on top of
                // the ground marker array
                if (toolbarMarkerNode.MarkerFound)
                {
                    // The box model is overlaid on the ground marker array, so in order to
                    // make the box model appear overlaid on the toolbar marker array, we need
                    // to offset the ground marker array's transformation. Thus, we multiply
                    // the toolbar marker array's transformation with the inverse of the ground marker
                    // array's transformation, which becomes T*G(inv)*G = T*I = T as a result, 
                    // where T is the transformation of the toolbar marker array, G is the 
                    // transformation of the ground marker array, and I is the identity matrix. 
                    // The Vector3(4, 4, 4) is a shift translation to make the box overlaid right 
                    // on top of the toolbar marker. The top-left corner of the left marker of the 
                    // toolbar marker array is defined as (0, 0, 0), so in order to make the box model
                    // appear right on top of the left marker of the toolbar marker array, we shift by
                    // half of each dimension of the 8x8x8 box model.  The approach used here requires that
                    // the ground marker array remains visible at all times.
                    Vector3 shiftVector = new Vector3(4, -4, 4);
                    Matrix mat = Matrix.CreateTranslation(shiftVector) *
                        toolbarMarkerNode.WorldTransformation *
                        Matrix.Invert(groundMarkerNode.WorldTransformation);

                    // Modify the transformation in the physics engine
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics, mat);
                }
                else
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics,
                        Matrix.CreateTranslation(Vector3.One * 4));
            }

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
