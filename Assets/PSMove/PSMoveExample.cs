using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class PSMoveExample : MonoBehaviour {
	
	public static string filename = "config.ini";
	public static string ipAddress = "128.2.237.66"; //RPIS 128.2.239.47 //ALICE 128.2.237.66
	public static string port = "7899";
	public GameObject gem,handleDot;
	public GameObject[] handles;
	public bool isMirror = false;
	public bool[] isHandleActive = new bool[3];
	public bool[] isColorSet = new bool[3];

	public float zOffset = 20;
	public float speedMultiplier;
	
	public bool gyroEnabled = true;
	public bool gyroAnalogScale = true;
	public float gyroAnalogMin = 10.0f;
	public float gyroXYSpeed = 1.25f;
	public float gyroXYEpsilon = 8.0f;
	public float gyroRotationSpeed = 1.5f;
	public float gyroRotationEpsilon = 10.0f;
	public float gyroScaleShrinkSpeed = 2.0f;
	public float gyroScaleGrowSpeed = 0.75f;
	public float gyroScaleEpsilon = 0.25f;
	public float gyroScaleCutoffX = 0.25f;
	public float gyroScaleCutoffY = 0.25f;
	public float gyroScaleMin = 1.0f;
	public float gyroScaleMax = 7.0f;
	public PuzzleTracker puzzleTracker;

    public GameObject[] calibrationCubes = new GameObject[3];

	private int psMoveReadyCount = 0;
	private Vector3[] handlePos = new Vector3[3];
	private Vector3[] handleOffset = new Vector3[3];
	private bool[] isOffsetSet = new bool[3];
	private bool[] didCalibrate = new bool[3];
	Quaternion temp = new Quaternion(0,0,0,0);
	
	#region GUI Variables
	string cameraStr = "Camera Switch On";
	string rStr = "0", gStr = "0", bStr = "0";
	string rumbleStr = "0";
	#endregion
	
	
	
	// Use this for initialization
	void Start () {
		
		loadFile(filename);
		
		//connect psmove
		if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.OSXPlayer) {
			PSMoveInput.Connect(ipAddress, int.Parse(port));
			
		}
		
	}
	
	
	
	void loadFile (string filename) {
		if (!File.Exists (filename)) {
			File.CreateText(filename);
			return;
		}
		
		try {
			string line;  
			StreamReader sReader = new StreamReader(filename);
			do
			{
				line = sReader.ReadLine();
				if (line != null)
				{
					// Lines with # are for comments
					if (!line.Contains("#")) {
						// Value property identified by string before the colon.
						string[] data = line.Split(':');
						if (data.Length == 2) {
							switch(data[0]) {
							case "IP Address":
								Debug.Log ("IP Address: " + data[1]);
								ipAddress = data[1].Trim();
								break;
							case "Port":
								Debug.Log ("Port: " + data[1]);
								port = data[1].Trim ();
								break;
							default:
								break;
							}
						}
					}
				}
			}
			while (line != null);
			sReader.Close();
			return;
		} catch (Exception e) {
		}
	}
	
	public void CalibratePlayer(int index){
		MoveController moveController = PSMoveInput.MoveControllers [index];
		//moveController.CalibrateAndTrack(index);
		if(PSMoveInput.IsConnected && moveController.Connected) {
			GameObject  handle = handles[index];
			MoveData moveData = moveController.Data;
			handleOffset[index] = -1 * moveData.Position;
			print("set handle offset: "+ index + ", osset: "+ handleOffset[index]);
			didCalibrate[index] = true;
			isHandleActive[index] = true;
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
		for (int i=0;handles != null && i<handles.Length; i++) {
			MoveController moveController = PSMoveInput.MoveControllers [i];
			//print("handle active:i"+ i +","+ isHandleActive[i]);
			if(PSMoveInput.IsConnected && moveController.Connected) {

				MoveData moveData = PSMoveInput.MoveControllers[i].Data;

				//
				// Title
				//
				if (Application.loadedLevel == 0) {
					if (calibrationCubes[i] != null) {
						GameObject cube = calibrationCubes[i];
						cube.transform.eulerAngles = moveData.Orientation;
						cube.GetComponent<MeshRenderer>().material.color = moveData.SphereColor;

						// NOTE: Some duplication here vs z scale main scene
						float zVelocity = moveData.HandleVelocity.z;
						if (zVelocity < 0)
							zVelocity *= gyroScaleShrinkSpeed;
						else
							zVelocity *= gyroScaleGrowSpeed;

						float analogScale = (gyroEnabled && gyroAnalogScale) ? (float)moveData.ValueT / 255.0f : 1.0f;
						//print ("HANDLE VELOCITY: " + moveData.HandleVelocity);
						if (Mathf.Abs(analogScale) > gyroScaleEpsilon && Mathf.Abs(moveData.HandleVelocity.x) < gyroScaleCutoffX
						    										  && Mathf.Abs(moveData.HandleVelocity.y) < gyroScaleCutoffY) {
							cube.transform.localScale = new Vector3(
								Mathf.Max(Mathf.Min(cube.transform.localScale.x + zVelocity * Time.deltaTime * analogScale, gyroScaleMax), gyroScaleMin),
								Mathf.Max(Mathf.Min(cube.transform.localScale.y + zVelocity * Time.deltaTime * analogScale, gyroScaleMax), gyroScaleMin),
								cube.transform.localScale.z);
						}
					}

				//
				// Main scene
				//
				} else if (isHandleActive[i] == true) {
					//print("update handle position");
					GameObject  handle = handles[i];
					Vector3 gemPos;
					Vector3 prevHandlePos = handlePos[i];
					gemPos = moveData.Position;
					
					if (gyroEnabled) {
						if ((gyroAnalogScale && moveData.ValueT >= gyroAnalogMin) || (!gyroAnalogScale && moveData.Buttons == MoveButton.T)) {

							// If enabled, we scale speeds by trigger's analog output
							float analogScale = (gyroEnabled && gyroAnalogScale) ? (float)moveData.ValueT / 255.0f : 1.0f;
							
							// Position
							float xRVelocity = (Mathf.Abs(moveData.AngularVelocity.x) < gyroXYEpsilon) ? 0 : moveData.AngularVelocity.x;
							float yRVelocity = (Mathf.Abs(moveData.AngularVelocity.y) < gyroXYEpsilon) ? 0 : -moveData.AngularVelocity.y;
							handle.transform.localPosition += new Vector3(yRVelocity * Time.deltaTime * gyroXYSpeed * analogScale,
							                                              xRVelocity * Time.deltaTime * gyroXYSpeed * analogScale, 0);
							Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
							Vector3 normalizedPosition = mainCamera.WorldToViewportPoint(handle.transform.position);
							normalizedPosition = new Vector3(Mathf.Clamp01(normalizedPosition.x), Mathf.Clamp01(normalizedPosition.y), normalizedPosition.z);
							handle.transform.position = mainCamera.ViewportToWorldPoint(normalizedPosition);
							handlePos[i] = handle.transform.position;

							// Rotation
							if(puzzleTracker.currentPuzzleIndex >= 3){
								float zRVelocity = (Mathf.Abs(moveData.AngularVelocity.z) < gyroRotationEpsilon) ? 0 : moveData.AngularVelocity.z;
								handle.transform.Rotate(new Vector3(0, 0, zRVelocity * Time.deltaTime * gyroRotationSpeed * analogScale));
							}

							// Scale
							if(puzzleTracker.currentPuzzleIndex >= 2){
								//print ("HANDLE VELOCITY: " + moveData.HandleVelocity);
								if (Mathf.Abs(analogScale) > gyroScaleEpsilon && Mathf.Abs(moveData.HandleVelocity.x) < gyroScaleCutoffX
								    										  && Mathf.Abs(moveData.HandleVelocity.y) < gyroScaleCutoffY) {

									float zVelocity = moveData.HandleVelocity.z;
									if (zVelocity < 0)
										zVelocity *= gyroScaleShrinkSpeed;
									else
										zVelocity *= gyroScaleGrowSpeed;
									handle.transform.localScale = new Vector3(
										Mathf.Max(Mathf.Min(handle.transform.localScale.x + zVelocity * Time.deltaTime * analogScale, gyroScaleMax), gyroScaleMin),
										Mathf.Max(Mathf.Min(handle.transform.localScale.y + zVelocity * Time.deltaTime * analogScale, gyroScaleMax), gyroScaleMin),
										handle.transform.localScale.z);

								}
							}

							continue;
						} else {
							continue;
						}
					} else {
						if(isOffsetSet[i] == false){
							CalibratePlayer(i);
							isOffsetSet[i] = true;
						}
						
						//m1
						/*
						Vector3 handleVel = moveData.HandleVelocity;
						if(Math.Abs(handleVel.x) < 0.1f){
							handleVel.x = 0;
						}
						if(Math.Abs(handleVel.y) < 0.1f){
							handleVel.y = 0;
						}

						if(prevHandlePos == null){
							handlePos[i] = moveData.HandlePosition;
						}
						else {
							handlePos[i] = prevHandlePos + handleVel*Time.deltaTime*speedMultiplier;
							//handlePos[i].x = (moveData.HandlePosition.x > prevHandlePos.x) ? prevHandlePos.x + Time.deltaTime*speedMultiplier : prevHandlePos.x - Time.deltaTime*speedMultiplier;
							//handlePos[i].y = (moveData.HandlePosition.y > prevHandlePos.y) ? prevHandlePos.y + Time.deltaTime*speedMultiplier : prevHandlePos.y - Time.deltaTime*speedMultiplier;

							if(handlePos[i].x < PlayerController.m_clampMinX || handlePos[i].x > PlayerController.m_clampMaxX){
								handlePos[i].x = prevHandlePos.x;
							}

							if(handlePos[i].y < PlayerController.m_clampMinY || handlePos[i].y > PlayerController.m_clampMaxY){
								handlePos[i].y = prevHandlePos.y;
							}

							handlePos[i].z = 0;

						}
						*/
						
						///////////////////////////////////
						
						//m2
						
						Vector3 nPos = (moveData.Position + handleOffset[i])*speedMultiplier;
						if(didCalibrate[i]){
							handlePos[i] = nPos;
							didCalibrate[i] = false;
						}
						else{
							
							handlePos[i].x = Mathf.Lerp(prevHandlePos.x,nPos.x,Time.deltaTime);
							handlePos[i].y = Mathf.Lerp(prevHandlePos.y,nPos.y,Time.deltaTime);
							
						}
						
						
						bool didExitFrame = false;
						
						if(handlePos[i].x <= PlayerController.m_clampMinX){
							handlePos[i].x = PlayerController.m_clampMaxX;
							didExitFrame = true;
						}
						else if(handlePos[i].x >= PlayerController.m_clampMaxX ){
							handlePos[i].x = PlayerController.m_clampMinX;
							didExitFrame = true;
						}
						
						if(handlePos[i].y <= PlayerController.m_clampMinY){
							handlePos[i].y = PlayerController.m_clampMaxY;
							didExitFrame = true;
						}
						else if(handlePos[i].y >= PlayerController.m_clampMaxY){
							handlePos[i].y = PlayerController.m_clampMinY;
							didExitFrame = true;
						}
						
						handlePos[i].x = Mathf.Clamp(handlePos[i].x,PlayerController.m_clampMinX,PlayerController.m_clampMaxX);
						handlePos[i].y = Mathf.Clamp(handlePos[i].y,PlayerController.m_clampMinY,PlayerController.m_clampMaxY);
						handlePos[i].z = 0;
						
						
						/////
						
						//m3
						//handlePos[i] = (moveData.Position + handleOffset[i])*speedMultiplier;
						//handlePos[i].x = Mathf.Lerp(prevHandlePos.x,moveData.Position.x + handleOffset[i].x,Time.deltaTime);
						//handlePos[i].y = Mathf.Lerp(prevHandlePos.y,moveData.Position.y + handleOffset[i].y,Time.deltaTime);
						
						
						if(isMirror) {
							gem.transform.localPosition = gemPos;
							handle.transform.localPosition = handlePos[i];
							handle.transform.localRotation = Quaternion.Euler(moveData.Orientation);
						}
						else {
							gemPos.z = -gemPos.z + zOffset;
							handlePos[i].z = -handlePos[i].z + zOffset;
							if(gem != null)
								gem.transform.localPosition = gemPos;
							
							
							if(handle != null) //moveData.Buttons == MoveButton.T 
							{
								Vector3 newHandlePos = handlePos[i];
								newHandlePos.x = Mathf.Clamp(handlePos[i].x,PlayerController.m_clampMinX,PlayerController.m_clampMaxX);
								newHandlePos.y = Mathf.Clamp(handlePos[i].y,PlayerController.m_clampMinY,PlayerController.m_clampMaxY);
								newHandlePos.z = 0;
								
								//set position of handle
								handle.transform.localPosition = newHandlePos;
								
								//handle.transform.localRotation = Quaternion.LookRotation(gemPos - handlePos);
								//handle.transform.Rotate(new Vector3(0,0,moveData.Orientation.z));
								
								if(moveData.Buttons == MoveButton.T){
									Quaternion rot = Quaternion.LookRotation(gemPos - handle.transform.localPosition); 
									rot.y=0;
									rot.x=0;
									handle.transform.localRotation = rot;
									handle.transform.Rotate(new Vector3(0,0,moveData.Orientation.z));
								}
							}
							
							/* using quaternion rotation directly
					 		 * the rotations on the x and y axes are inverted - i.e. left shows up as right, and right shows up as left. This code fixes this in case 
					 	     * the object you are using is facing away from the screen. Comment out this code if you do want an inversion along these axes
							 * 
					 		 * Add by Karthik Krishnamurthy*/
							
							temp = moveData.QOrientation;
							temp.x = -moveData.QOrientation.x;
							temp.y = -moveData.QOrientation.y;
							//if(handle != null)
							//handle.transform.localRotation = temp;
						}
					}
				}
			}
		}
		
		
		
	}
	
	// Remaps x from [srcMin, srcMax] to [destMin, destMax]. Source values outside source range are clamped.
	float EnsureMapRange(float x, float srcMin, float srcMax, float destMin, float destMax)
	{
		float xMapped = (x-srcMin)/(srcMax-srcMin) * (destMax-destMin) + destMin;
		if (xMapped > Mathf.Max(destMin, destMax))
			xMapped = Mathf.Max(destMin, destMax);
		else if (xMapped < Mathf.Min(destMin, destMax))
			xMapped = Mathf.Min(destMin, destMax);
		
		return xMapped;
	}
	
	void OnGUI() {
		
		if(!PSMoveInput.IsConnected) {
			/*
			GUI.Label(new Rect(20, 45, 30, 35), "IP:");
			ipAddress = GUI.TextField(new Rect(60, 45, 120, 25), ipAddress);
			
			GUI.Label(new Rect(190, 45, 30, 35), "port:");
			port = GUI.TextField(new Rect(230, 45, 50, 25), port);
			
			if(GUI.Button(new Rect(300, 40, 100, 35), "Connect")) {
				PSMoveInput.Connect(ipAddress, int.Parse(port));
			}*/
			
		}
		else {
			
			/*
			if(GUI.Button(new Rect(20, 40, 100, 35), "Disconnect"))  {
				PSMoveInput.Disconnect();
				Reset();
			}
			
			
			GUI.Label(new Rect(10, 10, 150, 100),  "PS Move count : " + PSMoveInput.MoveCount);
			GUI.Label(new Rect(140, 10, 150, 100),  "PS Nav count : " + PSMoveInput.NavCount);
			
			//camera stream on/off
			if(GUI.Button(new Rect(5, 80, 130, 35), cameraStr)) {
				if(cameraStr == "Camera Switch On") {
					PSMoveInput.CameraFrameResume();
					cameraStr = "Camera Switch Off";
				}
				else {
					PSMoveInput.CameraFramePause();
					cameraStr = "Camera Switch On";
				}
			}
			
			//color and rumble for move number 0
			if(PSMoveInput.MoveControllers[0].Connected) {
				//Set Color and Track
				GUI.Label(new Rect(300, 50, 200,20), "R,G,B are floats that fall in 0 ~ 1");
				GUI.Label(new Rect(260, 20, 20, 20), "R");
				rStr = GUI.TextField(new Rect(280, 20, 60, 20), rStr);
				GUI.Label(new Rect(350, 20, 20, 20), "G");
				gStr = GUI.TextField(new Rect(370, 20, 60, 20), gStr);
				GUI.Label(new Rect(440, 20, 20, 20), "B");
				bStr = GUI.TextField(new Rect(460, 20, 60, 20), bStr);
				if(GUI.Button(new Rect(550, 30, 160, 35), "SetColorAndTrack")) {
					try {
						float r = float.Parse(rStr);
						float g = float.Parse(gStr);
						float b = float.Parse(bStr);
						PSMoveInput.MoveControllers[0].SetColorAndTrack(new Color(r,g,b));
					}
					catch(Exception e) {
						Debug.Log("input problem: " + e.Message);
					}
				}
				//Rumble
				rumbleStr = GUI.TextField(new Rect(805, 20, 40, 20), rumbleStr);
				GUI.Label(new Rect(800, 50, 200,20), "0 ~ 19");
				if(GUI.Button(new Rect(870, 30, 100, 35), "Rumble")) {
					try {
						int rumbleValue = int.Parse(rumbleStr);
						PSMoveInput.MoveControllers[0].SetRumble(rumbleValue);
					}
					catch(Exception e) {
						Debug.Log("input problem: " + e.Message);
					}
				}
			}
			*/
			
			//move controller information
			
			for(int i=0; i<PSMoveInput.MAX_MOVE_NUM; i++)
			{
				MoveController moveController = PSMoveInput.MoveControllers[i];
				
				if(moveController.Connected) {
					MoveData moveData = moveController.Data;
					/*
					string display = "PS Move #" + i + 
						"\nPosition:\t\t"+moveData.Position + 
						"\nVelocity:\t\t"+moveData.Velocity + 
						"\nAcceleration:\t\t"+moveData.Acceleration + 
						"\nOrientation:\t\t"+moveData.Orientation + 
						"\nAngular Velocity:\t\t"+moveData.AngularVelocity + 
						"\nAngular Acceleration:\t\t"+moveData.AngularAcceleration + 
						"\nHandle Position:\t\t"+moveData.HandlePosition + 
						"\nHandle Velocity:\t\t"+moveData.HandleVelocity + 
						"\nHandle Acceleration:\t\t"+moveData.HandleAcceleration +
						"\n" +
						"\nTrigger Value:\t\t" + moveData.ValueT +
						"\nButtons 1:\t\t" + moveData.Buttons +
						"\nSphere Color:\t\t" + moveData.SphereColor +
						"\nIs Tracking:\t\t" + moveData.IsTracking +
						"\nTracking Hue:\t\t" + moveData.TrackingHue;
					*/
					string display = "PS Move #" + i + 
						"\nButtons:\t\t"+moveData.Buttons + 
							"\nhandle Velocity:\t\t"+moveData.HandleVelocity +
							"\nhandle Accel:\t\t"+moveData.HandleAcceleration +
							"\nPosition:\t\t"+moveData.Position;
					

					if(i == 0 && isColorSet[i] == false)
					{
						moveController.SetColorAndTrack(Color.red);
						isColorSet[i] = true;
					}
					else if(i == 1 && isColorSet[i] == false)
					{
						moveController.SetColorAndTrack(Color.blue);
						isColorSet[i] = true;
					}
					else if(i == 2 && isColorSet[i] == false)
					{
						moveController.SetColorAndTrack(Color.yellow);
						isColorSet[i] = true;
					}
					
					
					
					//GUI.Label(new Rect( 10 + 650 * (i/2), 120+310*(i%2), 300, 400),   display);
				}
				
			}
			
			/*
			for(int j = 0; j < PSMoveInput.MAX_NAV_NUM; j++) {
				NavController navController = PSMoveInput.NavControllers[j];
				if(navController.Connected) {	
					NavData navData = navController.Data;
					string navDisplay = "PS Nav #" + j + 
						"\nAnalog :\t\t" + navData.ValueAnalog +
						"\nL2 Value:\t\t" + navData.ValueL2 +
						"\nButtons:\t\t" + navData.Buttons;
					GUI.Label(new Rect(400, 100 + 95 * j, 150, 95),   navDisplay);
				}
			}*/
		}
		
		
	}


	
	private void Reset() {
		cameraStr = "Camera Switch On";
		rStr = "0"; 
		gStr = "0"; 
		bStr = "0";
		rumbleStr = "0";
	}
	
	GameObject getChildObjectWithTag(GameObject parent , string tag){
		Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) {
			// do whatever with child transform here
			if (child.tag == tag)
			{
				return child.gameObject;
			}
		}
		
		return null;
	}
}
