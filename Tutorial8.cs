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
        GeometryNode boxNode;
        bool useStaticImage = false;

        public Tutorial8()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

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
            // Make this sphere model cast and receive shadows
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
            sphereMaterial.Specular = Color.White.ToVector4();
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

            first[0] = 100;
            two[0] = 101;
            three[0] = 102;
            four[0] = 103;
            five[0] = 104;
            six[0] = 105;
            seven[0] = 106;
            eight[0] = 107;
            nine[0] = 108;
            ten[0] = 109;
            eleven[0] = 110;
            twelve[0] = 111;
            thirteen[0] = 112;
            fourteen[0] = 113;
            fifteen[0] = 114;
          

            //Marker 100
            cylinderMarkerNode100 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML100.xml", first);
            GeometryNode cylinderNode1 = new GeometryNode("Cylinder");

            cylinderNode1.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode1.Material = sphereMaterial;

            TransformNode cylinderTransNode1 = new TransformNode();

            cylinderTransNode1.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode100.AddChild(cylinderTransNode1);

            cylinderTransNode1.AddChild(cylinderNode1);

            //Marker 101
            cylinderMarkerNode101 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML101.xml", two);
            GeometryNode cylinderNode2 = new GeometryNode("Cylinder");

            cylinderNode2.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode2.Material = sphereMaterial;

            TransformNode cylinderTransNode2 = new TransformNode();

            cylinderTransNode2.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode101.AddChild(cylinderTransNode2);

            cylinderTransNode2.AddChild(cylinderNode2);

            //Marker 102
            cylinderMarkerNode102 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML102.xml", three);
            GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

            cylinderNode3.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode3.Material = sphereMaterial;

            TransformNode cylinderTransNode3 = new TransformNode();

            cylinderTransNode3.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode102.AddChild(cylinderTransNode3);

            cylinderTransNode3.AddChild(cylinderNode3);

            //Marker 103
            cylinderMarkerNode103 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML103.xml", four);
            GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

            cylinderNode3.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode3.Material = sphereMaterial;

            TransformNode cylinderTransNode2 = new TransformNode();

            cylinderTransNode3.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode103.AddChild(cylinderTransNode3);

            cylinderTransNode3.AddChild(cylinderNode2);

            //Marker 104
            cylinderMarkerNode104 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML104.xml", five);
            GeometryNode cylinderNode4 = new GeometryNode("Cylinder");

            cylinderNode4.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode4.Material = sphereMaterial;

            TransformNode cylinderTransNode4 = new TransformNode();

            cylinderTransNode4.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode104.AddChild(cylinderTransNode4);

            cylinderTransNode4.AddChild(cylinderNode4);

            //Marker 105
            cylinderMarkerNode105 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML105.xml", six);
            GeometryNode cylinderNode5 = new GeometryNode("Cylinder");

            cylinderNode5.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode5.Material = sphereMaterial;

            TransformNode cylinderTransNode5 = new TransformNode();

            cylinderTransNode5.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode105.AddChild(cylinderTransNode5);

            cylinderTransNode5.AddChild(cylinderNode5);

            //Marker 106
            cylinderMarkerNode106 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML106.xml", seven);
            GeometryNode cylinderNode6 = new GeometryNode("Cylinder");

            cylinderNode6.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode6.Material = sphereMaterial;

            TransformNode cylinderTransNode6 = new TransformNode();

            cylinderTransNode6.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode106.AddChild(cylinderTransNode6);

            cylinderTransNode6.AddChild(cylinderNode6);

            //Marker 107
            cylinderMarkerNode107 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML107.xml", eight);
            GeometryNode cylinderNode7 = new GeometryNode("Cylinder");

            cylinderNode7.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode7.Material = sphereMaterial;

            TransformNode cylinderTransNode7 = new TransformNode();

            cylinderTransNode7.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode107.AddChild(cylinderTransNode7);

            cylinderTransNode7.AddChild(cylinderNode7);

            //Marker 108
            cylinderMarkerNode108 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML108.xml", nine);
            GeometryNode cylinderNode8 = new GeometryNode("Cylinder");

            cylinderNode8.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode8.Material = sphereMaterial;

            TransformNode cylinderTransNode8 = new TransformNode();

            cylinderTransNode8.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode108.AddChild(cylinderTransNode8);

            cylinderTransNode8.AddChild(cylinderNode8);

            //Marker 109
            cylinderMarkerNode109 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML109.xml", ten);
            GeometryNode cylinderNode9 = new GeometryNode("Cylinder");

            cylinderNode9.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode9.Material = sphereMaterial;

            TransformNode cylinderTransNode9 = new TransformNode();

            cylinderTransNode9.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode109.AddChild(cylinderTransNode9);

            cylinderTransNode9.AddChild(cylinderNode9);

            //Marker 110
            cylinderMarkerNode110 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML110.xml", eleven);
            GeometryNode cylinderNode10 = new GeometryNode("Cylinder");

            cylinderNode10.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode10.Material = sphereMaterial;

            TransformNode cylinderTransNode10 = new TransformNode();

            cylinderTransNode10.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode110.AddChild(cylinderTransNode10);

            cylinderTransNode10.AddChild(cylinderNode10);

            //Marker 111
            cylinderMarkerNode111 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML111.xml", twelve);
            GeometryNode cylinderNode11 = new GeometryNode("Cylinder");

            cylinderNode11.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode11.Material = sphereMaterial;

            TransformNode cylinderTransNode11 = new TransformNode();

            cylinderTransNode11.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode111.AddChild(cylinderTransNode11);

            cylinderTransNode11.AddChild(cylinderNode11);

            //Marker 112
            cylinderMarkerNode112 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML112.xml", thirteen);
            GeometryNode cylinderNode12 = new GeometryNode("Cylinder");

            cylinderNode12.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode12.Material = sphereMaterial;

            TransformNode cylinderTransNode12 = new TransformNode();

            cylinderTransNode12.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode112.AddChild(cylinderTransNode12);

            cylinderTransNode12.AddChild(cylinderNode12);

            //Marker 113
            cylinderMarkerNode113 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML113.xml", fourteen);
            GeometryNode cylinderNode13 = new GeometryNode("Cylinder");

            cylinderNode13.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode13.Material = sphereMaterial;

            TransformNode cylinderTransNode13 = new TransformNode();

            cylinderTransNode13.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode113.AddChild(cylinderTransNode13);

            cylinderTransNode13.AddChild(cylinderNode13);

            //Marker 114
            cylinderMarkerNode114 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML114.xml", fifteen);
            GeometryNode cylinderNode14 = new GeometryNode("Cylinder");

            cylinderNode14.Model = new Cylinder(3, 3, 6, 10);

            cylinderNode14.Material = sphereMaterial;

            TransformNode cylinderTransNode14 = new TransformNode();

            cylinderTransNode14.Translation = new Vector3(0, 0, 3);

            cylinderMarkerNode114.AddChild(cylinderTransNode14);

            cylinderTransNode14.AddChild(cylinderNode14);

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
            
        }

        /// <summary>
        /// A callback function that will be called when the box and sphere model collides
        /// </summary>
        /// <param name="pair"></param>
        private void BoxSphereCollision(NewtonPhysics.CollisionPair pair)
        {
            Console.WriteLine("Box and Sphere has collided");
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
