using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class wayPointManager : MonoBehaviour {

    #region variables
    TextWriter tw;
    // Use this for initialization

    private int uCnt=0, stepCnt;

    private const int BUSCOUNT = 14;
    private const int POINTCOUNT = 50;
    private const int LINECOUNT = 52;
    private  int TIME_STEP;
    private  int UAVCOUNT;
    private const int BASECOUNT = 4;

    public GameObject wayPointPrefab;
    public GameObject linePrefab;
    public GameObject segPointPrefab;
    public GameObject uavPrefab;
    public GameObject basePrefab;


    public GameObject[] wayPointsSet = new GameObject[POINTCOUNT];
    public GameObject[,] wayPoints = new GameObject[POINTCOUNT, POINTCOUNT];
    private GameObject[] uavSet;
    public GameObject[] uavOperate;



    //public Vector2 xVal= new Vector2(-50f,50f);
    //public Vector2 zVal = new Vector2(-50f, 50f);

    public float speed = 10f;
    
    //private float[] journeyLength = new float[UAVCOUNT]; 
   
    private bool keyHit;


    private Dictionary<int,int> isBusDict = new Dictionary<int,int>();
    private Dictionary<int, int> baseDict = new Dictionary<int, int>();
    private Dictionary<int, GameObject> wayPointDict = new Dictionary<int, GameObject>();
    
    private int[,] lineSet;
    private int[,] uavPath;
    private int[,] uavPoint;
    private double[,] busCord;
    private int[,] fuelPoint;

    //static int current = 0;  

    private int[] wpRadArr;
    private int[] currPos; 
    private float[] speedUav; 
    private float[] baseSpeed;


    #endregion

    void InputFile()
    {
        TextAsset busFile = Resources.Load<TextAsset>("is_bus");
        //TextAsset uavFile = Resources.Load<TextAsset>("uav_point_14_bus");
        //TextAsset uavPathFile = Resources.Load<TextAsset>("uav_path");
        //TextAsset fuelFile = Resources.Load<TextAsset>("refuel_point");     
        TextAsset posFile = Resources.Load<TextAsset>("46_position");
        TextAsset baseFile = Resources.Load<TextAsset>("base");
        TextAsset lineFile = Resources.Load<TextAsset>("line_set_46");

        char[] delims = { ' ', ',', '\t' };

        System.IO.StreamReader sr;

        #region initialize
        sr = new System.IO.StreamReader("Input\\uav_point_14_bus.txt");        

        String totalUav;
        String[] totalUavVal;
       
        while ((totalUav = sr.ReadLine()) != null)
        {
            uCnt++;            
            //totalUav = totalUav.Trim();
            totalUavVal = totalUav.Trim().Split(delims);
            stepCnt = totalUavVal.Length;
        }
        

        UAVCOUNT = uCnt;
        TIME_STEP = stepCnt;

        Debug.Log("total UAV...." + UAVCOUNT);

        Debug.Log("total step...." + TIME_STEP);

        wpRadArr = new int[UAVCOUNT];
        currPos = new int[UAVCOUNT];
        speedUav = new float[UAVCOUNT];
        baseSpeed= new float[UAVCOUNT];

        uavSet = new GameObject[UAVCOUNT];
        uavOperate = new GameObject[UAVCOUNT];

        uavPath = new int[UAVCOUNT, TIME_STEP];
        uavPoint = new int[UAVCOUNT, TIME_STEP];
        busCord = new double[POINTCOUNT, 3];
        lineSet = new int[LINECOUNT, 2];
        fuelPoint = new int[UAVCOUNT, 5];

        for (int i = 0; i < UAVCOUNT; i++)
        {
            wpRadArr[i] = 1;
            currPos[i] = 1;
            // speedUav[i] = speed + (i*2f);
            speedUav[i] = speed;
            baseSpeed[i] = speedUav[i] * 0.6f;

            //for (int j = 0; j < 5; j++)
            //{
            //    fuelPoint[i, j] = 0;
            //}

            // Debug.Log("speed..." + speedUav[i]);
        }

        #endregion


        #region isBus

        //for(int i=1; i<=POINTCOUNT;i++)
        //{
        //    isBusDict.Add(i, 0);
        //}

        String[] busInFile = busFile.text.Split('\n');
        String[] busElement;

        foreach (String line in busInFile)
        {
            busElement = line.Trim().Split(delims);

            for (int l = 0; l < busElement.Length; l++)
            {
                //Debug.Log(busElement[l]);
                isBusDict.Add(Convert.ToInt32(busElement[l]), 1);
            }

        }

        #endregion

        #region base station

        String[] baseInFile = baseFile.text.Split('\n');
        String[] baseElement;

        int b = 47;
        foreach (String line in baseInFile)
        {
            baseElement = line.Trim().Split(delims);
            for (int l = 0; l < baseElement.Length; l++)
            {
                //Debug.Log(busElement[l]);
                baseDict.Add(Convert.ToInt32(baseElement[l]), b);
            }
            ++b;
        }
        //for (int i = 1; i <= POINTCOUNT-BASECOUNT; i++)
        //{
        //    Debug.Log("base..."+i+" " + baseDict[i]);
        //}
        #endregion


        #region fuel point
        sr = new System.IO.StreamReader("Input\\refuel_point.txt");
        String fuelInFile;
        String[] fuelElement;

        for (int a = 0; a < UAVCOUNT; a++)
        {

            if ((fuelInFile = sr.ReadLine()) == null)
            {
                //fuelPoint[a, 0] = 0;
                throw new Exception("Exit due to Insufficient/Extra Input: Initial Inputs");
            }
            fuelInFile = fuelInFile.Trim();
            fuelElement = fuelInFile.Split(delims);
            //Debug.Log("steps..." + lineVal.Length);
            for (int pt = 0; pt < fuelElement.Length; pt++)
            {
                if (String.IsNullOrEmpty(fuelElement[pt])) fuelPoint[a, pt] = 0;
                else fuelPoint[a, pt] = Convert.ToInt32(fuelElement[pt]);
            }

        }


        #endregion


        #region uav
        sr = new System.IO.StreamReader("Input\\uav_point_14_bus.txt");
        tw = new StreamWriter("Input\\uav_path.txt", false);
        String testLine;
        String[] lineVal;

        for (int a = 0; a < UAVCOUNT; a++)
        {
            //int pt = 0;

            if ((testLine = sr.ReadLine()) == null)
            {
                //Debug.Log("enter."+testLine);
                throw new Exception("Exit due to Insufficient/Extra Input: Initial Inputs");
            }
            testLine = testLine.Trim();
            lineVal = testLine.Split(delims);
            //Debug.Log("steps..." + lineVal.Length);
            for (int pt = 0; pt < lineVal.Length; pt++)
            {
                if (fuelPoint[a, 0] != 0 && (Convert.ToInt32(lineVal[pt]) == fuelPoint[a, 0] || uavPoint[a, pt] == baseDict[fuelPoint[a, 0]]))
                {


                    if ((pt + 1) < lineVal.Length && Convert.ToInt32(lineVal[pt + 1]) == 47)
                    {
                        //Debug.Log("enter 49");
                        uavPoint[a, pt + 1] = baseDict[fuelPoint[a, 0]];
                        //tw.Write(uavPath[a, l+1] + " ");
                        //Debug.Log("fuel fournds.." + baseDict[fuelPoint[a, 0]]);
                        //Debug.Log("fuel fournds.." + uavPath[a, pt+1]);
                    }
                }

                if (Convert.ToInt32(lineVal[pt]) != 47)
                {
                    uavPoint[a, pt] = Convert.ToInt32(lineVal[pt]);
                }
                tw.Write(uavPoint[a, pt] + " ");
            }

            tw.WriteLine();

        }


        //for (int i = 0; i < UAVCOUNT; i++)
        //{
        //    for (int j = 0; j < TIME_STEP; j++)
        //    {
        //         Debug.Log(i+ " " +uavPoint[i, j]);
        //        // tw.Write(uavPath[i, j]);
        //    }
        //    //tw.WriteLine("");
        //}
        tw.Close();

        #endregion

        #region uav routing
        sr = new System.IO.StreamReader("Input\\uav_path.txt");
        String route;
        String[] routeElement;

        for (int a = 0; a < UAVCOUNT; a++)
        {
            //int pt = 0;

            if ((route = sr.ReadLine()) == null)
            {
                throw new Exception("Exit due to Insufficient/Extra Input: Initial Inputs");
            }
            route = route.Trim();
            routeElement = route.Split(delims);
            //Debug.Log("steps..." + lineVal.Length);
            for (int pt = 0; pt < routeElement.Length; pt++)
            {
                uavPath[a, pt] = Convert.ToInt32(routeElement[pt]);
            }

        }

        //for (int i = 0; i < UAVCOUNT; i++)
        //{
        //    for (int j = 0; j < TIME_STEP; j++)
        //    {
        //        Debug.Log(i + " " + uavPath[i, j]);
        //        // tw.Write(uavPath[i, j]);
        //    }
        //    //tw.WriteLine("");
        //}
        //tw.Close();


        #endregion



        

        //Debug.Log(uavFile.text);
       

        
        #region postion

        String[] posInFile = posFile.text.Split('\n');
        String[] posElement;
        int p = 0;
        
        foreach (String line in posInFile)
        {
            posElement = line.Split(delims);
            //Debug.Log("poss..." + posElement.Length);
            for (int l = 0; l < posElement.Length; l++)
            {
                //Debug.Log(lineElement[l]);
                //Debug.Log(posElement[l]);
                busCord[p, l] = Convert.ToDouble(posElement[l]);

            }
            p++;
        }

        //for (int i = 0; i < POINTCOUNT; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        // Console.Write(busCord[i, j]+" ");
        //        // tw.Write(uavPath[i, j]);
        //    }
        //    //Console.WriteLine();
        //    //tw.WriteLine("");
        //}

        #endregion

        #region line

        String[] trLineInFile = lineFile.text.Split('\n');
        String[] trLineElement;
        int tr = 0;

       
        foreach (String line in trLineInFile)
        {
            trLineElement = line.Split(delims);

            for (int l = 0; l < trLineElement.Length; l++)
            {
                //Debug.Log(lineElement[l]);
                lineSet[tr, l] = Convert.ToInt32(trLineElement[l]);

            }
            tr++;
        }

        //for (int i = 0; i < LINECOUNT; i++)
        //{
        //    for (int j = 0; j < 2; j++)
        //    {
        //        //Debug.Log(lineSet[i, j]);
        //        // tw.Write(uavPath[i, j]);
        //    }
        //    //tw.WriteLine("");
        //}
        #endregion

    }

    void InstantiateWayPoint()
    {
        
        for (int i = 0; i < POINTCOUNT; ++i)
        {
            float x = (float)busCord[i, 0];
            //float y = (float)busCord[i, 1];
            float z = (float)busCord[i, 2];
            //Debug.Log("is bus...{0}" , (i + 1));
            if (isBusDict.ContainsKey(i+1))
            {
               // Debug.Log( isBusDict.ContainsKey(i + 1));
                wayPointsSet[i] = Instantiate(wayPointPrefab, new Vector3(x, 0, z), Quaternion.identity);
                wayPointDict.Add(i + 1, wayPointsSet[i]);
                //amount += 20f;
            }
            else if(i > 45)
            {
                //Debug.Log("i now...." + i);
                wayPointsSet[i] = Instantiate(basePrefab, new Vector3(x, 0, z), Quaternion.identity);
                wayPointDict.Add(i + 1, wayPointsSet[i]);
            }

            else
            {
               // Debug.Log(isBusDict.ContainsKey(i + 1));
                wayPointsSet[i] = Instantiate(segPointPrefab, new Vector3(x, 0, z), Quaternion.identity);
                wayPointDict.Add(i + 1, wayPointsSet[i]);
                //amount += 20f;
            }
            
        }
        //Debug.Log("dic point..." + wayPointDict.Count);
    }

    void InstantiateLineRend()
    {

        
        for (int i = 0; i < LINECOUNT; i++)
        {
            GameObject tempLine;
            LineRenderer lineRend;

            //Material mat = new Material(Shader.Find("Particles/Alpha Blended"));

            GameObject tempStart, tempEnd;
            int p = lineSet[i, 0];
            int q = lineSet[i, 1];
            tempStart = wayPointDict[p];
            tempEnd = wayPointDict[q];

            int count = 0;

            float x = tempStart.transform.position.x;
            float y = tempStart.transform.position.y;
            float z = tempStart.transform.position.z;

            float x2 = tempEnd.transform.position.x;
            float y2 = tempEnd.transform.position.y;
            float z2 = tempEnd.transform.position.z;
            while (count < 3)
            {
               // tempLine = Instantiate(linePrefab, tempStart.transform.position, Quaternion.identity);

                tempLine = Instantiate(linePrefab, new Vector3(x , y+ (count *2), z), Quaternion.identity);
                lineRend = tempLine.GetComponent<LineRenderer>();
               // lineRend.positionCount= 2;
                lineRend.SetPosition(0, new Vector3(x, y + (count * 2), z));
                lineRend.SetPosition(1, new Vector3(x2, y2 + (count * 2), z2));
                //Particles / Additive, Alpha Blended
               // lineRend.material = new Material(Shader.Find("Particles/Alpha Blended"));
                //lineRend.material = mat;
                lineRend.startColor= Color.black;
                lineRend.endColor = Color.black;
                lineRend.startWidth = 0.8f;
                lineRend.endWidth = 0.8f;
                lineRend.Simplify(1);
              //  Debug.Log("Line reduced from " + lineRend.positionCount + " to " + lineRend.positionCount);



                count++;
            }
          
        }
    }

    
    void InstantiateUav()
    {
        keyHit = false;
        for (int i = 0; i < UAVCOUNT; i++)
        {
            //GameObject temp;
            //uavSet[i] = Instantiate(uavPrefab, wayPoints[i,i%2].transform.position, Quaternion.identity);
            //Lerp lp=  uavSet[i].GetComponent<Lerp>();
            // lp.dest = wayPoints[i, i].transform;
            int startU = uavPath[i, 0];

            float x = wayPointDict[startU].transform.position.x;
            float y = wayPointDict[startU].transform.position.y;
            float z = wayPointDict[startU].transform.position.z;
            uavSet[i] = Instantiate(uavPrefab, new Vector3(x,y+10f,z), Quaternion.identity);
            uavOperate[i] = uavSet[i];
        }
    }

    void Start () {

        InputFile();
        InstantiateWayPoint();
        //CreateLines();
        InstantiateLineRend();
        InstantiateUav();
        //uavMoveOperation();
        
    }
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            keyHit = true;            
        }

        if (keyHit == true)
        {

            for (int i = 0; i < UAVCOUNT; i++)
            {
                //float x = wayPointDict[uavPath[0, current + 1]].transform.position.x;
                //float y = wayPointDict[uavPath[0, current + 1]].transform.position.y;
                //float z = wayPointDict[uavPath[0, current + 1]].transform.position.z;
                int point = uavPath[i, currPos[i]];
                


               

                float x = wayPointDict[point].transform.position.x;
                float y = wayPointDict[point].transform.position.y;
                float z = wayPointDict[point].transform.position.z;
                if (Vector3.Distance(new Vector3(x, y + 10f, z), uavSet[i].transform.position) < wpRadArr[i])
                {
                    (currPos[i])++;
                    //current++;
                    //++uN;
                    //Debug.Log("uav now..." + uN);
                    //Debug.Log("curr..." + currPos[0]);
                }

                if (point > 46)
                {
                    //uavOperate[0].transform.position = Vector3.MoveTowards(uavSet[0].transform.position, wayPointDict[uavPath[0, current + 1]].transform.position, Time.deltaTime * speed);
                    uavOperate[i].transform.position = Vector3.MoveTowards(uavSet[i].transform.position, new Vector3(x, y + 10f, z), Time.deltaTime * baseSpeed[i]);
                    Debug.Log("speed..." + baseSpeed[i]);
                }
                else
                {
                    //uavOperate[0].transform.position = Vector3.MoveTowards(uavSet[0].transform.position, wayPointDict[uavPath[0, current + 1]].transform.position, Time.deltaTime * speed);
                    uavOperate[i].transform.position = Vector3.MoveTowards(uavSet[i].transform.position, new Vector3(x, y + 10f, z), Time.deltaTime * speedUav[i]);

                }
                   

                if (currPos[i] >= TIME_STEP)
                {
                    currPos[i] = TIME_STEP - 1;
                    speedUav[i] = 0;
                }

            }
                     

        }



    }
}
