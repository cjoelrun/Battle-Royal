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

    MarkerNode cylinderMarkerNode130, cylinderMarkerNode131, cylinderMarkerNode132,
      cylinderMarkerNode133, cylinderMarkerNode134, cylinderMarkerNode135, cylinderMarkerNode136,
      cylinderMarkerNode137, cylinderMarkerNode138, cylinderMarkerNode139, cylinderMarkerNode140,
      cylinderMarkerNode141, cylidnerMarkerNode142, cylinderMarkerNode143, cylinderMarkerNode144; 

    MarkerNode cylinderMarkerNode145, cylinderMarkerNode146, cylinderMarkerNode147,
      cylinderMarkerNode148, cylinderMarkerNode149, cylinderMarkerNode150, cylinderMarkerNode151,
      cylinderMarkerNode152, cylinderMarkerNode153, cylinderMarkerNode154, cylinderMarkerNode155,
      cylinderMarkerNode156, cylidnerMarkerNode157, cylinderMarkerNode158, cylinderMarkerNode159; 

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

      int[] zero = new int[1];
      int[] one = new int[1];
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

      zero[0] = 100;
      one[0] = 101;
      two[0] = 102;
      three[0] = 103;
      four[0] = 104;
      five[0] = 105;
      six[0] = 106;
      seven[0] = 107;
      eight[0] = 108;
      nine[0] = 109;
      ten[0] = 110;
      eleven[0] = 111;
      twelve[0] = 112;
      thirteen[0] = 113;
      fourteen[0] = 114;

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

      //Markers 130-144
      int[] thirty = new int[1];
      int[] thirtyone = new int[1];
      int[] thirtytwo = new int[1];
      int[] thirtythree = new int[1];
      int[] thirtyfour = new int[1];
      int[] thirtyfive = new int[1];
      int[] thirtysiz = new int[1];
      int[] thirtyseven = new int[1];
      int[] thirtyeight = new int[1];
      int[] thirtynine = new int[1];
      int[] forty = new int[1];
      int[] fortyone = new int[1];
      int[] fortytwo = new int[1];
      int[] fortythree = new int[1];
      int[] fortyfour = new int[1];

      thirty[0] = 130;
      thirtyone[0] = 131;
      thirtytwo[0] = 132;
      thirtythree[0] = 133;
      thirtyfour[0] = 134;
      thirtyfive[0] = 135;
      thirtysiz[0] = 136;
      thirtyseven[0] = 137;
      thirtyeight[0] = 138;
      thirtynine[0] = 139;
      forty[0] = 140;
      fortyone[0] = 141;
      fortytwo[0] = 142;
      fortythree[0] = 143;
      fortyfour[0] = 144

	//Markers 145-159
	int[] fortyfive = new int[1];
      int[] fortysix = new int[1];
      int[] fortyseven = new int[1];
      int[] fortyeight = new int[1];
      int[] fortynine = new int[1];
      int[] fifty = new int[1];
      int[] fiftyone = new int[1];
      int[] fiftytwo = new int[1];
      int[] fiftythree = new int[1];
      int[] fiftyfour = new int[1];
      int[] fiftyfive = new int[1];
      int[] fiftysix = new int[1];
      int[] fiftyseven = new int[1];
      int[] fiftyeight = new int[1];
      int[] fiftynine = new int[1];

      fortyfive[0] = 145;
      fortysix[0] = 146;
      fortyseven[0] = 147;
      fortyeight[0] = 148;
      fortynine[0] = 149;
      fifty[0] = 150;
      fiftyone[0] = 151;
      fiftytwo[0] = 152;
      fiftythree[0] = 153;
      fiftyfour[0] = 154;
      fiftyfive[0] = 155;
      fiftysix[0] = 156;
      fiftyseven[0] = 157;
      fiftyeight[0] = 158;
      fiftynine[0] = 159;

      //Marker 100
      cylinderMarkerNode100 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML100.xml", zero);
      GeometryNode cylinderNode1 = new GeometryNode("Cylinder");

      cylinderNode1.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode1.Material = sphereMaterial;

      TransformNode cylinderTransNode1 = new TransformNode();

      cylinderTransNode1.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode100.AddChild(cylinderTransNode1);

      cylinderTransNode1.AddChild(cylinderNode1);

      //Marker 101
      cylinderMarkerNode101 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML101.xml", one);
      GeometryNode cylinderNode2 = new GeometryNode("Cylinder");

      cylinderNode2.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode2.Material = sphereMaterial;

      TransformNode cylinderTransNode2 = new TransformNode();

      cylinderTransNode2.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode101.AddChild(cylinderTransNode2);

      cylinderTransNode2.AddChild(cylinderNode2);

      //Marker 102
      cylinderMarkerNode102 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML102.xml", two);
      GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

      cylinderNode3.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode3.Material = sphereMaterial;

      TransformNode cylinderTransNode3 = new TransformNode();

      cylinderTransNode3.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode102.AddChild(cylinderTransNode3);

      cylinderTransNode3.AddChild(cylinderNode3);

      //Marker 103
      cylinderMarkerNode103 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML103.xml", three);
      GeometryNode cylinderNode3 = new GeometryNode("Cylinder");

      cylinderNode3.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode3.Material = sphereMaterial;

      TransformNode cylinderTransNode2 = new TransformNode();

      cylinderTransNode3.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode103.AddChild(cylinderTransNode3);

      cylinderTransNode3.AddChild(cylinderNode2);

      //Marker 104
      cylinderMarkerNode104 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML104.xml", four);
      GeometryNode cylinderNode4 = new GeometryNode("Cylinder");

      cylinderNode4.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode4.Material = sphereMaterial;

      TransformNode cylinderTransNode4 = new TransformNode();

      cylinderTransNode4.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode104.AddChild(cylinderTransNode4);

      cylinderTransNode4.AddChild(cylinderNode4);

      //Marker 105
      cylinderMarkerNode105 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML105.xml", five);
      GeometryNode cylinderNode5 = new GeometryNode("Cylinder");

      cylinderNode5.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode5.Material = sphereMaterial;

      TransformNode cylinderTransNode5 = new TransformNode();

      cylinderTransNode5.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode105.AddChild(cylinderTransNode5);

      cylinderTransNode5.AddChild(cylinderNode5);

      //Marker 106
      cylinderMarkerNode106 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML106.xml", six);
      GeometryNode cylinderNode6 = new GeometryNode("Cylinder");

      cylinderNode6.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode6.Material = sphereMaterial;

      TransformNode cylinderTransNode6 = new TransformNode();

      cylinderTransNode6.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode106.AddChild(cylinderTransNode6);

      cylinderTransNode6.AddChild(cylinderNode6);

      //Marker 107
      cylinderMarkerNode107 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML107.xml", seven);
      GeometryNode cylinderNode7 = new GeometryNode("Cylinder");

      cylinderNode7.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode7.Material = sphereMaterial;

      TransformNode cylinderTransNode7 = new TransformNode();

      cylinderTransNode7.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode107.AddChild(cylinderTransNode7);

      cylinderTransNode7.AddChild(cylinderNode7);

      //Marker 108
      cylinderMarkerNode108 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML108.xml", eight);
      GeometryNode cylinderNode8 = new GeometryNode("Cylinder");

      cylinderNode8.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode8.Material = sphereMaterial;

      TransformNode cylinderTransNode8 = new TransformNode();

      cylinderTransNode8.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode108.AddChild(cylinderTransNode8);

      cylinderTransNode8.AddChild(cylinderNode8);

      //Marker 109
      cylinderMarkerNode109 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML109.xml", nine);
      GeometryNode cylinderNode9 = new GeometryNode("Cylinder");

      cylinderNode9.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode9.Material = sphereMaterial;

      TransformNode cylinderTransNode9 = new TransformNode();

      cylinderTransNode9.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode109.AddChild(cylinderTransNode9);

      cylinderTransNode9.AddChild(cylinderNode9);

      //Marker 110
      cylinderMarkerNode110 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML110.xml", ten);
      GeometryNode cylinderNode10 = new GeometryNode("Cylinder");

      cylinderNode10.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode10.Material = sphereMaterial;

      TransformNode cylinderTransNode10 = new TransformNode();

      cylinderTransNode10.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode110.AddChild(cylinderTransNode10);

      cylinderTransNode10.AddChild(cylinderNode10);

      //Marker 111
      cylinderMarkerNode111 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML111.xml", eleven);
      GeometryNode cylinderNode11 = new GeometryNode("Cylinder");

      cylinderNode11.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode11.Material = sphereMaterial;

      TransformNode cylinderTransNode11 = new TransformNode();

      cylinderTransNode11.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode111.AddChild(cylinderTransNode11);

      cylinderTransNode11.AddChild(cylinderNode11);

      //Marker 112
      cylinderMarkerNode112 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML112.xml", twelve);
      GeometryNode cylinderNode12 = new GeometryNode("Cylinder");

      cylinderNode12.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode12.Material = sphereMaterial;

      TransformNode cylinderTransNode12 = new TransformNode();

      cylinderTransNode12.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode112.AddChild(cylinderTransNode12);

      cylinderTransNode12.AddChild(cylinderNode12);

      //Marker 113
      cylinderMarkerNode113 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML113.xml", thirteen);
      GeometryNode cylinderNode13 = new GeometryNode("Cylinder");

      cylinderNode13.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode13.Material = sphereMaterial;

      TransformNode cylinderTransNode13 = new TransformNode();

      cylinderTransNode13.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode113.AddChild(cylinderTransNode13);

      cylinderTransNode13.AddChild(cylinderNode13);

      //Marker 114
      cylinderMarkerNode114 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML114.xml", fourteen);
      GeometryNode cylinderNode14 = new GeometryNode("Cylinder");

      cylinderNode14.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode14.Material = sphereMaterial;

      TransformNode cylinderTransNode14 = new TransformNode();

      cylinderTransNode14.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode114.AddChild(cylinderTransNode14);

      cylinderTransNode14.AddChild(cylinderNode14);

      //Marker 115
      cylinderMarkerNode115 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML115.xml", fifteen);
      GeometryNode cylinderNode15 = new GeometryNode("Cylinder");

      cylinderNode15.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode15.Material = sphereMaterial;

      TransformNode cylinderTransNode15 = new TransformNode();

      cylinderTransNode15.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode115.AddChild(cylinderTransNode15);

      cylinderTransNode15.AddChild(cylinderNode15);

      //Marker 116
      cylinderMarkerNode116 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML116.xml", sixteen);
      GeometryNode cylinderNode16 = new GeometryNode("Cylinder");

      cylinderNode16.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode16.Material = sphereMaterial;

      TransformNode cylinderTransNode16 = new TransformNode();

      cylinderTransNode16.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode116.AddChild(cylinderTransNode16);

      cylinderTransNode16.AddChild(cylinderNode16);

      //Marker 117
      cylinderMarkerNode117 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML117.xml", seventeen);
      GeometryNode cylinderNode17 = new GeometryNode("Cylinder");

      cylinderNode17.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode17.Material = sphereMaterial;

      TransformNode cylinderTransNode17 = new TransformNode();

      cylinderTransNode17.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode117.AddChild(cylinderTransNode17);

      cylinderTransNode17.AddChild(cylinderNode17);

      //Marker 118
      cylinderMarkerNode118 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML118.xml", eighteen);
      GeometryNode cylinderNode18 = new GeometryNode("Cylinder");

      cylinderNode18.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode18.Material = sphereMaterial;

      TransformNode cylinderTransNode18 = new TransformNode();

      cylinderTransNode18.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode118.AddChild(cylinderTransNode18);

      cylinderTransNode18.AddChild(cylinderNode18);

      //Marker 119
      cylinderMarkerNode119 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML119.xml", nineteen);
      GeometryNode cylinderNode19 = new GeometryNode("Cylinder");

      cylinderNode19.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode19.Material = sphereMaterial;

      TransformNode cylinderTransNode19 = new TransformNode();

      cylinderTransNode19.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode119.AddChild(cylinderTransNode19);

      cylinderTransNode19.AddChild(cylinderNode19);

      //Marker 120
      cylinderMarkerNode120 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML120.xml", twenty);
      GeometryNode cylinderNode20 = new GeometryNode("Cylinder");

      cylinderNode20.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode20.Material = sphereMaterial;

      TransformNode cylinderTransNode20 = new TransformNode();

      cylinderTransNode20.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode120.AddChild(cylinderTransNode20);

      cylinderTransNode20.AddChild(cylinderNode20);

      //Marker 121
      cylinderMarkerNode121 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML121.xml", twentyone);
      GeometryNode cylinderNode21 = new GeometryNode("Cylinder");

      cylinderNode21.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode21.Material = sphereMaterial;

      TransformNode cylinderTransNode21 = new TransformNode();

      cylinderTransNode21.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode121.AddChild(cylinderTransNode21);

      cylinderTransNode21.AddChild(cylinderNode21);

      //Marker 122
      cylinderMarkerNode122 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML122.xml", twentytwo);
      GeometryNode cylinderNode22 = new GeometryNode("Cylinder");

      cylinderNode22.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode22.Material = sphereMaterial;

      TransformNode cylinderTransNode22 = new TransformNode();

      cylinderTransNode22.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode122.AddChild(cylinderTransNode22);

      cylinderTransNode22.AddChild(cylinderNode22);

      //Marker 123
      cylinderMarkerNode123 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML123.xml", twentythree);
      GeometryNode cylinderNode23 = new GeometryNode("Cylinder");

      cylinderNode23.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode23.Material = sphereMaterial;

      TransformNode cylinderTransNode23 = new TransformNode();

      cylinderTransNode23.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode123.AddChild(cylinderTransNode23);

      cylinderTransNode23.AddChild(cylinderNode23);

      //Marker 124
      cylinderMarkerNode124 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML124.xml", twentyfour);
      GeometryNode cylinderNode24 = new GeometryNode("Cylinder");

      cylinderNode24.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode24.Material = sphereMaterial;

      TransformNode cylinderTransNode24 = new TransformNode();

      cylinderTransNode24.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode124.AddChild(cylinderTransNode24);

      cylinderTransNode24.AddChild(cylinderNode24);

      //Marker 125
      cylinderMarkerNode125 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML125.xml", twentyfive);
      GeometryNode cylinderNode25 = new GeometryNode("Cylinder");

      cylinderNode25.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode25.Material = sphereMaterial;

      TransformNode cylinderTransNode25 = new TransformNode();

      cylinderTransNode25.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode125.AddChild(cylinderTransNode25);

      cylinderTransNode25.AddChild(cylinderNode25);

      //Marker 126
      cylinderMarkerNode126 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML126.xml", twentysix);
      GeometryNode cylinderNode26 = new GeometryNode("Cylinder");

      cylinderNode26.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode26.Material = sphereMaterial;

      TransformNode cylinderTransNode26 = new TransformNode();

      cylinderTransNode26.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode126.AddChild(cylinderTransNode26);

      cylinderTransNode26.AddChild(cylinderNode26);

      //Marker 127
      cylinderMarkerNode127 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML127.xml", twentyseven);
      GeometryNode cylinderNode27 = new GeometryNode("Cylinder");

      cylinderNode27.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode27.Material = sphereMaterial;

      TransformNode cylinderTransNode27 = new TransformNode();

      cylinderTransNode27.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode127.AddChild(cylinderTransNode27);

      cylinderTransNode27.AddChild(cylinderNode27);

      //Marker 128
      cylinderMarkerNode128 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML128.xml", twentyeight);
      GeometryNode cylinderNode28 = new GeometryNode("Cylinder");

      cylinderNode28.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode28.Material = sphereMaterial;

      TransformNode cylinderTransNode28 = new TransformNode();

      cylinderTransNode28.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode128.AddChild(cylinderTransNode28);

      cylinderTransNode28.AddChild(cylinderNode28);

      //Marker 129
      cylinderMarkerNode129 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML129.xml", twentynine);
      GeometryNode cylinderNode29 = new GeometryNode("Cylinder");

      cylinderNode29.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode29.Material = sphereMaterial;

      TransformNode cylinderTransNode29 = new TransformNode();

      cylinderTransNode29.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode129.AddChild(cylinderTransNode29);

      cylinderTransNode29.AddChild(cylinderNode29);


      //Marker 130
      cylinderMarkerNode130 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML130.xml", thirty);
      GeometryNode cylinderNode30 = new GeometryNode("Cylinder");

      cylinderNode30.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode30.Material = sphereMaterial;

      TransformNode cylinderTransNode30 = new TransformNode();

      cylinderTransNode30.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode130.AddChild(cylinderTransNode30);

      cylinderTransNode30.AddChild(cylinderNode30);

      //Marker 131
      cylinderMarkerNode131 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML131.xml", thirtyone);
      GeometryNode cylinderNode31 = new GeometryNode("Cylinder");

      cylinderNode31.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode31.Material = sphereMaterial;

      TransformNode cylinderTransNode31 = new TransformNode();

      cylinderTransNode31.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode131.AddChild(cylinderTransNode31);

      cylinderTransNode31.AddChild(cylinderNode31);

      //Marker 132
      cylinderMarkerNode132 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML132.xml", thirtytwo);
      GeometryNode cylinderNode32 = new GeometryNode("Cylinder");

      cylinderNode32.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode32.Material = sphereMaterial;

      TransformNode cylinderTransNode32 = new TransformNode();

      cylinderTransNode32.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode132.AddChild(cylinderTransNode32);

      cylinderTransNode32.AddChild(cylinderNode32);

      //Marker 133
      cylinderMarkerNode133 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML133.xml", thirtythree);
      GeometryNode cylinderNode33 = new GeometryNode("Cylinder");

      cylinderNode33.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode33.Material = sphereMaterial;

      TransformNode cylinderTransNode33 = new TransformNode();

      cylinderTransNode33.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode133.AddChild(cylinderTransNode33);

      cylinderTransNode33.AddChild(cylinderNode33);

      //Marker 134
      cylinderMarkerNode134 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML134.xml", thirtyfour);
      GeometryNode cylinderNode34 = new GeometryNode("Cylinder");

      cylinderNode34.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode34.Material = sphereMaterial;

      TransformNode cylinderTransNode34 = new TransformNode();

      cylinderTransNode34.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode134.AddChild(cylinderTransNode34);

      cylinderTransNode34.AddChild(cylinderNode34);

      //Marker 135
      cylinderMarkerNode135 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML135.xml", thirtyfive);
      GeometryNode cylinderNode35 = new GeometryNode("Cylinder");

      cylinderNode35.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode35.Material = sphereMaterial;

      TransformNode cylinderTransNode35 = new TransformNode();

      cylinderTransNode35.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode135.AddChild(cylinderTransNode35);

      cylinderTransNode35.AddChild(cylinderNode35);

      //Marker 136
      cylinderMarkerNode136 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML136.xml", thirtysix);
      GeometryNode cylinderNode36 = new GeometryNode("Cylinder");

      cylinderNode36.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode36.Material = sphereMaterial;

      TransformNode cylinderTransNode36 = new TransformNode();

      cylinderTransNode36.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode136.AddChild(cylinderTransNode36);

      cylinderTransNode36.AddChild(cylinderNode36);

      //Marker 137
      cylinderMarkerNode137 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML137.xml", thirtyseven);
      GeometryNode cylinderNode37 = new GeometryNode("Cylinder");

      cylinderNode37.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode37.Material = sphereMaterial;

      TransformNode cylinderTransNode37 = new TransformNode();

      cylinderTransNode37.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode137.AddChild(cylinderTransNode37);

      cylinderTransNode37.AddChild(cylinderNode37);

      //Marker 138
      cylinderMarkerNode138 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML138.xml", thirtyeight);
      GeometryNode cylinderNode38 = new GeometryNode("Cylinder");

      cylinderNode38.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode38.Material = sphereMaterial;

      TransformNode cylinderTransNode38 = new TransformNode();

      cylinderTransNode38.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode138.AddChild(cylinderTransNode38);

      cylinderTransNode38.AddChild(cylinderNode38);

      //Marker 139
      cylinderMarkerNode139 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML139.xml", thirtynine);
      GeometryNode cylinderNode39 = new GeometryNode("Cylinder");

      cylinderNode39.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode39.Material = sphereMaterial;

      TransformNode cylinderTransNode39 = new TransformNode();

      cylinderTransNode39.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode139.AddChild(cylinderTransNode39);

      cylinderTransNode39.AddChild(cylinderNode39);

      //The following line is to be placed before the 140th node to hange color to Blue
      sphereMaterial.Diffuse = new Vector4(0, 0, 0.5f, 1);

      //Marker 140
      cylinderMarkerNode140 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML140.xml", forty);
      GeometryNode cylinderNode40 = new GeometryNode("Cylinder");

      cylinderNode40.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode40.Material = sphereMaterial;

      TransformNode cylinderTransNode40 = new TransformNode();

      cylinderTransNode40.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode140.AddChild(cylinderTransNode40);

      cylinderTransNode40.AddChild(cylinderNode40);

      //Marker 141
      cylinderMarkerNode141 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML141.xml", fortyone);
      GeometryNode cylinderNode41 = new GeometryNode("Cylinder");

      cylinderNode41.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode41.Material = sphereMaterial;

      TransformNode cylinderTransNode41 = new TransformNode();

      cylinderTransNode41.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode141.AddChild(cylinderTransNode41);

      cylinderTransNode41.AddChild(cylinderNode41);

      //Marker 142
      cylinderMarkerNode142 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML142.xml", fortytwo);
      GeometryNode cylinderNode42 = new GeometryNode("Cylinder");

      cylinderNode42.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode42.Material = sphereMaterial;

      TransformNode cylinderTransNode42 = new TransformNode();

      cylinderTransNode42.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode142.AddChild(cylinderTransNode42);

      cylinderTransNode42.AddChild(cylinderNode42);

      //Marker 143
      cylinderMarkerNode143 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML143.xml", fortythree);
      GeometryNode cylinderNode43 = new GeometryNode("Cylinder");

      cylinderNode43.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode43.Material = sphereMaterial;

      TransformNode cylinderTransNode43 = new TransformNode();

      cylinderTransNode43.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode143.AddChild(cylinderTransNode43);

      cylinderTransNode43.AddChild(cylinderNode43);

      //Marker 144
      cylinderMarkerNode144 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML144.xml", fortyfour);
      GeometryNode cylinderNode44 = new GeometryNode("Cylinder");

      cylinderNode44.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode44.Material = sphereMaterial;

      TransformNode cylinderTransNode44 = new TransformNode();

      cylinderTransNode44.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode144.AddChild(cylinderTransNode44);

      cylinderTransNode44.AddChild(cylinderNode44);

      //Marker 145
      cylinderMarkerNode145 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML145.xml", fourfive);
      GeometryNode cylinderNode45 = new GeometryNode("Cylinder");

      cylinderNode45.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode45.Material = sphereMaterial;

      TransformNode cylinderTransNode45 = new TransformNode();

      cylinderTransNode45.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode145.AddChild(cylinderTransNode45);

      cylinderTransNode45.AddChild(cylinderNode45);

      //Marker 146
      cylinderMarkerNode146 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML146.xml", fortysix);
      GeometryNode cylinderNode46 = new GeometryNode("Cylinder");

      cylinderNode46.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode46.Material = sphereMaterial;

      TransformNode cylinderTransNode46 = new TransformNode();

      cylinderTransNode46.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode146.AddChild(cylinderTransNode46);

      cylinderTransNode46.AddChild(cylinderNode46);

      //Marker 147
      cylinderMarkerNode147 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML147.xml", fortyseven);
      GeometryNode cylinderNode47 = new GeometryNode("Cylinder");

      cylinderNode47.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode47.Material = sphereMaterial;

      TransformNode cylinderTransNode47 = new TransformNode();

      cylinderTransNode47.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode147.AddChild(cylinderTransNode47);

      cylinderTransNode47.AddChild(cylinderNode47);

      //Marker 148
      cylinderMarkerNode148 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML148.xml", fortyeight);
      GeometryNode cylinderNode48 = new GeometryNode("Cylinder");

      cylinderNode48.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode48.Material = sphereMaterial;

      TransformNode cylinderTransNode48 = new TransformNode();

      cylinderTransNode48.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode148.AddChild(cylinderTransNode48);

      cylinderTransNode48.AddChild(cylinderNode48);

      //Marker 149
      cylinderMarkerNode149 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML149.xml", fortynine);
      GeometryNode cylinderNode49 = new GeometryNode("Cylinder");

      cylinderNode49.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode49.Material = sphereMaterial;

      TransformNode cylinderTransNode49 = new TransformNode();

      cylinderTransNode49.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode149.AddChild(cylinderTransNode49);

      cylinderTransNode49.AddChild(cylinderNode49);

      //changing color to Red
      sphereMaterial.Diffuse = new Vector4(0.5f, 0, 0, 1);

      //Marker 150
      cylinderMarkerNode150 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML150.xml", fifty);
      GeometryNode cylinderNode50 = new GeometryNode("Cylinder");

      cylinderNode50.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode50.Material = sphereMaterial;

      TransformNode cylinderTransNode50 = new TransformNode();

      cylinderTransNode50.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode150.AddChild(cylinderTransNode50);

      cylinderTransNode50.AddChild(cylinderNode50);

      //Marker 151
      cylinderMarkerNode151 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML151.xml", fiftyone);
      GeometryNode cylinderNode51 = new GeometryNode("Cylinder");

      cylinderNode51.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode51.Material = sphereMaterial;

      TransformNode cylinderTransNode51 = new TransformNode();

      cylinderTransNode51.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode151.AddChild(cylinderTransNode51);

      cylinderTransNode51.AddChild(cylinderNode51);

      //Marker 152
      cylinderMarkerNode152 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML152.xml", fiftytwo);
      GeometryNode cylinderNode52 = new GeometryNode("Cylinder");

      cylinderNode52.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode52.Material = sphereMaterial;

      TransformNode cylinderTransNode52 = new TransformNode();

      cylinderTransNode52.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode152.AddChild(cylinderTransNode52);

      cylinderTransNode52.AddChild(cylinderNode52);

      //Marker 153
      cylinderMarkerNode153 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML153.xml", fiftythree);
      GeometryNode cylinderNode53 = new GeometryNode("Cylinder");

      cylinderNode53.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode53.Material = sphereMaterial;

      TransformNode cylinderTransNode53 = new TransformNode();

      cylinderTransNode53.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode153.AddChild(cylinderTransNode53);

      cylinderTransNode53.AddChild(cylinderNode53);

      //Marker 154
      cylinderMarkerNode154 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML154.xml", fiftyfour);
      GeometryNode cylinderNode54 = new GeometryNode("Cylinder");

      cylinderNode54.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode54.Material = sphereMaterial;

      TransformNode cylinderTransNode54 = new TransformNode();

      cylinderTransNode54.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode154.AddChild(cylinderTransNode54);

      cylinderTransNode54.AddChild(cylinderNode54);

      //Marker 155
      cylinderMarkerNode155 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML155.xml", fiftyfive);
      GeometryNode cylinderNode55 = new GeometryNode("Cylinder");

      cylinderNode55.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode55.Material = sphereMaterial;

      TransformNode cylinderTransNode55 = new TransformNode();

      cylinderTransNode55.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode155.AddChild(cylinderTransNode55);

      cylinderTransNode55.AddChild(cylinderNode55);

      //Marker 156
      cylinderMarkerNode156 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML156.xml", fiftysix);
      GeometryNode cylinderNode56 = new GeometryNode("Cylinder");

      cylinderNode56.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode56.Material = sphereMaterial;

      TransformNode cylinderTransNode56 = new TransformNode();

      cylinderTransNode56.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode156.AddChild(cylinderTransNode56);

      cylinderTransNode56.AddChild(cylinderNode56);

      //Marker 157
      cylinderMarkerNode157 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML157.xml", fiftyseven);
      GeometryNode cylinderNode57 = new GeometryNode("Cylinder");

      cylinderNode57.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode57.Material = sphereMaterial;

      TransformNode cylinderTransNode57 = new TransformNode();

      cylinderTransNode57.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode157.AddChild(cylinderTransNode57);

      cylinderTransNode57.AddChild(cylinderNode57);

      //Marker 158
      cylinderMarkerNode158 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML158.xml", fiftyeight);
      GeometryNode cylinderNode58 = new GeometryNode("Cylinder");

      cylinderNode58.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode58.Material = sphereMaterial;

      TransformNode cylinderTransNode58 = new TransformNode();

      cylinderTransNode58.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode158.AddChild(cylinderTransNode58);

      cylinderTransNode58.AddChild(cylinderNode58);

      //Marker 159
      cylinderMarkerNode159 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML159.xml", fiftynine);
      GeometryNode cylinderNode59 = new GeometryNode("Cylinder");

      cylinderNode59.Model = new Cylinder(3, 3, 6, 10);

      cylinderNode59.Material = sphereMaterial;

      TransformNode cylinderTransNode59 = new TransformNode();

      cylinderTransNode59.Translation = new Vector3(0, 0, 3);

      cylinderMarkerNode159.AddChild(cylinderTransNode59);

      cylinderTransNode59.AddChild(cylinderNode59);

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
      scene.RootNode.AddChild(cylinderMarkerNode142);
      scene.RootNode.AddChild(cylinderMarkerNode143);
      scene.RootNode.AddChild(cylinderMarkerNode144);

      scene.RootNode.AddChild(cylinderMarkerNode145);
      scene.RootNode.AddChild(cylinderMarkerNode146);
      scene.RootNode.AddChild(cylinderMarkerNode147);
      scene.RootNode.AddChild(cylinderMarkerNode148);
      scene.RootNode.AddChild(cylinderMarkerNode149);
      scene.RootNode.AddChild(cylinderMarkerNode150);
      scene.RootNode.AddChild(cylinderMarkerNode151);
      scene.RootNode.AddChild(cylinderMarkerNode152);
      scene.RootNode.AddChild(cylinderMarkerNode153);
      scene.RootNode.AddChild(cylinderMarkerNode154);
      scene.RootNode.AddChild(cylinderMarkerNode155);
      scene.RootNode.AddChild(cylinderMarkerNode156);
      scene.RootNode.AddChild(cylinderMarkerNode157);
      scene.RootNode.AddChild(cylinderMarkerNode158);
      scene.RootNode.AddChild(cylinderMarkerNode159);
            
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
